import networkx as nx
from networkx.drawing.nx_pydot import graphviz_layout
import matplotlib.pyplot as plt
from collections import deque
import numpy as np
import seaborn as sns
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity



class ScheduleGraph(nx.DiGraph):
    
    def __init__(self, *agrs, **kawgs):
        
        self.task_list = None
        self.workforce_list = None
        self.schedule_data = None
        self.workforce_data = None

        self.task_similarity_matrix = []
        self.workforce_timing = {}
        self.workforce_assignment= {}
        self.task_skills_requqired = []

        super(ScheduleGraph, self).__init__(*agrs, **kawgs)
        self.setup()
        
    def setup(self):
        self.workforce_timing = {f"W_default_{node}": [0] for node in self.nodes}
        self.workforce_assignment= {node: f"W_default_{node}" for node in self.nodes}
    

    def load_schedule(self, schedule_data: dict) -> None:

        for t_name in schedule_data.keys():
            required_skills =  {skill.split("_")[0]: int(skill.split("_")[1]) for skill in schedule_data[t_name]['required_skills']}
            self.add_node(
                schedule_data[t_name]['task_id'],
                duration =  schedule_data[t_name]['duration'], 
                required_skills = required_skills,
                start_time = 0,
                finish_time = 0
                )
            
            predencors = schedule_data[t_name]['predence']
            if predencors != []:
                for predencor in predencors:
                    self.add_edge(predencor, schedule_data[t_name]['task_id'])
            
            # add required skills node's to task skills requqired graph's
            self.task_skills_requqired.extend(required_skills)
            self.task_skills_requqired = list(set(self.task_skills_requqired))

        # calculate task task similarity matrix, create m x m (m is number task)
        executed_tasks = [node for node in self.nodes(data=True) if node[0] not in ['START', 'END']]

        for i, task_data in enumerate(executed_tasks):  
            task_id, data = task_data[0], task_data[1]
            if task_id not in ['START', 'END']:
                similar_vector  = []
                # mapping required skills task to vector with shape (1 x len(task_skills_requqired))
                required_skills_vector = [data['required_skills'].get(skill, 0) for skill in self.task_skills_requqired]
                for _, data in executed_tasks:
                    # mapping required skills task to vector with shape (1 x len(task_skills_requqired))
                    _required_skills_vector = [ data['required_skills'].get(skill, 0) for skill in self.task_skills_requqired]

                    
                    similarity_score = cosine_similarity(np.array([required_skills_vector, _required_skills_vector]))[1][0]
                    similar_vector.append(similarity_score)

                self.task_similarity_matrix.append(similar_vector)
        
        self.schedule_data = schedule_data
        self.task_list = list([node for node in self.nodes() if node not in ['START', 'END']])

    def load_workforce(self, workforce_data: dict):

        # 1 similarity score of each skills workforce and each requied skills task
        # 2 calculate cost workforce
        for node, data in self.nodes(data=True):
            workforce_similarity, workforce_cost, available_workforce = {}, {}, {}
            # 1.1 mapping required skills task to vector with shape (1 x len(task_skills_requqired))
            required_skills_vector = [data['required_skills'].get(skill, 0) for skill in self.task_skills_requqired]
            
            if node not in ['START', 'END']:
                for wf in workforce_data:
                    # 1.2 mapping required skills task to vector with shape (1 x len(task_skills_requqired))
                    workforce_skills = {skill.split("_")[0]: int(skill.split("_")[1]) for skill in  workforce_data[wf]['skills']}
                    workforce_skills_vector  = [workforce_skills.get(skill, 0) for skill in self.task_skills_requqired]
                    
                    # similarity score between workforce and task
                    similarity_score = cosine_similarity(np.array([required_skills_vector, workforce_skills_vector]))[1][0]
                    workforce_similarity[wf] = similarity_score
                    # avaibile workforce 
                    available_workforce[wf] = min([item1 - item2 for item1, item2 in zip(workforce_skills_vector, required_skills_vector)]) >= 0

                    # cost workforce 
                    workforce_cost[wf]  = workforce_data[wf]['salary_unit'] * data['duration']

                nx.set_node_attributes(self,  {node: {"workforce_similarity": workforce_similarity, "workforce_cost": workforce_cost, "available_workforce": available_workforce}})
        
        # update workforce list
        self.workforce_list = list(workforce_data.keys())
       
        # update workforce data to graph
        self.workforce_data = workforce_data

        # init workforce timming
        self.init_workforce_timing()

    def init_workforce_timing(self):
        # set default working timeing:
        for wf in self.workforce_data:
            self.workforce_timing[wf] = [0]


    def apply_assignment(self, assignment_data: dict = {}, assignment_list: list = []) -> None:
        
        overall_cost = 0
        overall_skill_similarity = 0
        overall_task_similarity = 0
        task_queues = {}
   
        if assignment_list != []: 
            self.workforce_assignment = {list(self.nodes)[i+1]:list(self.workforce_data.keys())[assignment_list[i]] for i in  range(len(assignment_list))}
        else:
            self.workforce_assignment = assignment_data
        # init workforce timing
        self.init_workforce_timing()
    
        # calculate cost & skill similarity  assigned by each workforce
        for task in self.task_list:
            
            data = self.nodes[task]
            workforce = self.workforce_assignment[task]

            workforce_cost = data['workforce_cost']
            workforce_similarity = data['workforce_similarity']
            available_workforce = data['available_workforce']
            
            # 1. calculate cost
            # 1.1 check avaible workforce
            if available_workforce[workforce] == True:
                overall_skill_similarity += workforce_similarity[workforce]
            else:
                overall_skill_similarity += -999999

            # 2. calculate skill similarity
            overall_cost += workforce_cost[workforce]


            if self.workforce_assignment[task] not in task_queues:
                task_queues[workforce] = []
            
            task_queues[workforce].append(task)
        

        # calculate overall task similarity assigned by workforce
        # calculate overall task similarity assigned by workforce
        for task in task_queues:
            task_queue = task_queues[workforce]
            for node in task_queue:
                
                task_i = self.task_list.index(node)
                for task in task_queue:
                    task_j = self.task_list.index(node)
                    overall_task_similarity += self.task_similarity_matrix[task_i][task_j]/len(task_queue)

        # calculate completed_time
        self.schedule(start_node = 'START')
        completed_time = self.nodes['END']['finish_time']

        results = {
            "overall_cost": overall_cost, 
            "overall_skill_similarity": overall_skill_similarity, 
            "overall_task_similarity": overall_task_similarity, 
            "completed_time": completed_time

        }

        return results
         

    def pertchart(self):
        graph_copy = self.copy()
        for node in graph_copy.nodes():
            if 'workforce_cost' in graph_copy.nodes[node]:
                del graph_copy.nodes[node]['workforce_cost']

            if 'workforce_similarity' in graph_copy.nodes[node]:
                del graph_copy.nodes[node]['workforce_similarity']
            
            if 'required_skills' in graph_copy.nodes[node]:
                del graph_copy.nodes[node]['required_skills']

            if 'available_workforce' in graph_copy.nodes[node]:
                del graph_copy.nodes[node]['available_workforce']
                
        pos = graphviz_layout(graph_copy, prog="dot")
        for k,v in pos.items():
            pos[k]=(-v[1],v[0])
        nx.draw_networkx_nodes(graph_copy,pos = pos, node_shape = 's', node_size = 200, 
                            node_color = 'none', edgecolors='k')
        nx.draw_networkx_edges(graph_copy,pos = pos, 
                            node_shape = 's', width = 1,  node_size = 200)
        nx.draw_networkx_labels(graph_copy,pos = pos, font_size = 5)
        plt.savefig("pert_chart.png")
        plt.close()

    
    def schedule(self, start_node: str):
        visited = set()
        queue = deque([start_node])

        while queue:
            node = queue.popleft()
            if node not in visited:

                if node != start_node:
                    predecessors = list(self.predecessors(node))
                    # check predecessors node's was visited
                    if len(set(predecessors) & set(visited)) == len(predecessors):
                        visited.add(node)

                        early_start_time = max([self.nodes[node]['finish_time'] + 1 for node in predecessors])
                        finish_time =  early_start_time +  self.nodes[node]["duration"]

                        if node != 'END':
                            workforce = self.workforce_assignment[node]
                            working_days = self.workforce_timing[workforce]

                            # check working time in this task overlap with running time worker's?
                            if len(set(range(early_start_time, finish_time + 1)) & set(working_days)) > 0:
                                early_start_time = max(working_days) + 1
                                finish_time =  early_start_time +  self.nodes[node]["duration"]

                            self.workforce_timing[workforce].extend(range(early_start_time, finish_time + 1))
                        nx.set_node_attributes(self,  {node: {"start_time": early_start_time, "finish_time": finish_time}})
                    
                elif node == start_node:
                    visited.add(node)
                
                for neighbor in list(self.neighbors(node)):
                    if neighbor not in visited:
                        queue.append(neighbor)

    def granttchart(self):
        sorted_nodes = sorted(self.nodes(data=True), key=lambda x: x[1]['start_time'])
        sorted_nodes, _ = zip(*sorted_nodes)
        # Determine the maximum finish time
        max_duration = self.nodes['END']['finish_time']

        # Set up the y-axis range
        plt.ylim(0, len(sorted_nodes))

        # Set up the x-axis range
        plt.xlim(0, max_duration)

        # Draw the nodes as horizontal bars in the Gantt chart
        for i, node in enumerate(sorted_nodes):
            
            start_time = self.nodes[node]['start_time']
            duration = self.nodes[node]['duration']

            plt.barh(i, width=duration, left=start_time, height=0.5, align='center', color="green", edgecolor='black')  # Sử dụng thuộc tính color của task để định nghĩa màu

            plt.text(start_time + duration / 2, i + 0.25, node, ha='center', va='center')

            # Draw the edges in the Gantt chart
            for v in self.neighbors(node):
                u_index = sorted_nodes.index(node)
                v_index = sorted_nodes.index(v)

                start_x = self.nodes[node]['finish_time']
                start_y = u_index
                end_x = self.nodes[v]['start_time']
                end_y = v_index

                mid_x = (start_x + end_x) / 2
                mid_y = (start_y + end_y) / 2

                plt.plot([start_x, mid_x, end_x], [start_y, mid_y, end_y], 'k-', linewidth=1.5)

        # Set y-axis tick labels in the Gantt chart
        plt.yticks(range(len(sorted_nodes)), [node for node in sorted_nodes])

        # Set x-axis label in the Gantt chart
        plt.xlabel('Time')

        # Save the Gantt chart as an image
        plt.grid(axis='x')
        plt.savefig("gantt_chart.png")
        plt.close()
    
    def task_similarity_heatmap(self):
        # Create a heatmap using Seaborn
        sns.heatmap(
            np.round(self.task_similarity_matrix, 2), 
            annot=True, 
            cmap="YlGnBu", 
            xticklabels=[node for node in self.nodes() if node not in ['START', 'END']], 
            yticklabels=[node for node in self.nodes() if node not in ['START', 'END']])


        # Set up the figure size
        plt.figure(figsize=(10, 6))
        # Display the heatmap
        plt.show()

    def visualize(self):
        self.granttchart()
        self.pertchart()

        # Display both the Gantt chart and PERT chart images
        gantt_chart_img = plt.imread("gantt_chart.png")
        pert_chart_img = plt.imread("pert_chart.png")

        fig, axes = plt.subplots(1, 2, figsize=(12, 6))  # Increase the figsize parameter to adjust the size of the displayed images

        axes[0].imshow(gantt_chart_img)
        axes[0].axis('off')
        axes[0].set_title("Gantt Chart")

        axes[1].imshow(pert_chart_img)
        axes[1].axis('off')
        axes[1].set_title("PERT Chart")

        plt.tight_layout()
        plt.show()