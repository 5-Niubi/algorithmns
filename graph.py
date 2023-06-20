import matplotlib.pyplot as plt
import networkx as nx
from collections import defaultdict
from typing import List
from task import Task
import numpy as np
class Graph:
    def __init__(self):
        self.graph = defaultdict(list)
        self.start_node = None
        self.finish_node = None
        self.assigned_workforce = {}
        self.node_mapping = {}

    def addEdge(self, u: Task, v: Task):
        self.graph[u].append(v)
        
        self.node_mapping[u.task_id] = u
        self.node_mapping[v.task_id] = v


    def set_start_node(self, start_node):
        self.start_node = start_node

    def set_finish_node(self, finish_node):
        self.finish_node = finish_node
    
    def assign(self, task_id, workforce):
        self.assigned_workforce[task_id] = workforce

    
    


    def get_list_task(self):
        results = []
        for task in self.graph:
            results.append(task)
            results.extend(self.graph[task])

        return list(set(results))

            
    def getStartTimeNode(self, node):
        setted_time = True
        finish_times = []
        predence_nodes = self.getPrecedenceNodes(node)
        for p_node in predence_nodes:
            if p_node.start_time is None:
                setted_time = False
            finish_times.append(p_node.finish_time)

        return setted_time, finish_times

        
    def getPrecedenceNodes(self, node):
        precedence_nodes = []
        for key, value in self.graph.items():
            if node in value:
                precedence_nodes.append(key)
        return precedence_nodes

    def visualizePERTChart(self, colors, groups):

        sorted_nodes = sorted(self.graph.keys(), key=lambda x: x.start_time)

        G = nx.DiGraph()

        # Add nodes to the PERT chart
        for node in sorted_nodes:

            color = colors[np.where(groups == self.assigned_workforce[node.task_id].workforce_id)[0][0]] if node.task_id  not in ['START', 'END'] else None

            G.add_node(node.task_id, task=node, color=color, start=node.start_time, finish=node.finish_time)

        # Add edges to the PERT chart
        for u, v_list in self.graph.items():
            for v in v_list:
                G.add_edge(u.task_id, v.task_id)

        # Set node positions based on a tree-like structure
        pos = nx.nx_pydot.graphviz_layout(G, prog='dot')

        # Determine the maximum duration in the PERT chart
        max_duration = max(node.duration for node in sorted_nodes)

        # Draw the nodes as rectangles in the PERT chart
        node_labels = {}
        for node, data in G.nodes(data=True):

            task = data['task']
            color = colors[np.where(groups == self.assigned_workforce[node].workforce_id)[0][0]] if node  not in ['START', 'END'] else None

            start = task.start_time
            finish = task.finish_time
            node_labels[node] = f"{task.task_id}\n({start},{finish})"
            rect_height = max_duration
            rect_width = task.duration
            rect_x = pos[node][0] - rect_width / 2
            rect_y = pos[node][1] - max_duration / 2
            rect = plt.Rectangle((rect_x, rect_y),
                                 rect_width, rect_height,
                                 linewidth=1,
                                 edgecolor='black',
                                 facecolor=color)  # Sử dụng thuộc tính color của task để định nghĩa màu
            plt.gca().add_patch(rect)

        # Draw the labels in the PERT chart
        nx.draw_networkx_labels(G, pos, labels=node_labels, font_weight="bold")

        # Draw the edges in the PERT chart
        nx.draw_networkx_edges(G, pos, edge_color="gray")

        # Save the PERT chart as an image
        plt.axis("off")
        plt.savefig("pert_chart.png")
        plt.close()

    def visualizeGranttChart(self, colors, groups):
        sorted_nodes = sorted(self.graph.keys(), key=lambda x: x.start_time)

        # Determine the maximum finish time
        max_finish_time = max(node.finish_time for node in sorted_nodes)

        # Determine the maximum duration
        max_duration = max(node.finish_time + node.duration for node in sorted_nodes)

        # Set up the y-axis range
        plt.ylim(0, len(sorted_nodes))

        # Set up the x-axis range
        plt.xlim(0, max_duration)

        # Draw the nodes as horizontal bars in the Gantt chart
        for i, node in enumerate(sorted_nodes):
            start_time = node.start_time
            duration = node.duration

            color = colors[np.where(groups == self.assigned_workforce[node.task_id].workforce_id)[0][0]] if node.task_id  not in ['START', 'END'] else None

            plt.barh(i, width=duration, left=start_time, height=0.5, align='center', color=color, edgecolor='black')  # Sử dụng thuộc tính color của task để định nghĩa màu

            plt.text(start_time + duration / 2, i + 0.25, node.task_id, ha='center', va='center')

        # Draw the edges in the Gantt chart
        for u, v_list in self.graph.items():
            for v in v_list:
                u_index = sorted_nodes.index(u)
                v_index = sorted_nodes.index(v)

                start_x = u.finish_time
                start_y = u_index
                end_x = v.start_time
                end_y = v_index

                mid_x = (start_x + end_x) / 2
                mid_y = (start_y + end_y) / 2

                plt.plot([start_x, mid_x, end_x], [start_y, mid_y, end_y], 'k-', linewidth=1.5)

        # Set y-axis tick labels in the Gantt chart
        plt.yticks(range(len(sorted_nodes)), [node.task_id for node in sorted_nodes])

        # Set x-axis label in the Gantt chart
        plt.xlabel('Time')

        # Save the Gantt chart as an image
        plt.grid(axis='x')
        plt.savefig("gantt_chart.png")
        plt.close()

    def visualizeCharts(self):

        import matplotlib.pyplot as plt
        import numpy as np


        cmap = plt.cm.get_cmap('tab10')

        groups = np.unique([resource.workforce_id for resource in self.assigned_workforce.values()])


        colors = [cmap(i) for i in np.linspace(0, 1, len(groups))]

       


        self.visualizeGranttChart(colors, groups)
        self.visualizePERTChart(colors, groups)

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