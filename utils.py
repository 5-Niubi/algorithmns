
from graph import Graph
from task import Task
from collections import deque

def bfs(graph: Graph, start_node: Task):
    visited = set()
    queue = deque([start_node])
    while queue:
        node = queue.popleft()
        if node not in visited:
            parent_nodes = graph.getPrecedenceNodes(node)
            
            if len(list(set(parent_nodes) & set(visited))) == len(parent_nodes):
                visited.add(node)
            
            # Check predence
            ## Add the node to the tasks list

            # print(f"{node}, start_node = {start_node}")
            if node != start_node :
                setted_time, finish_times =  graph.getStartTimeNode(node = node)
                
                if setted_time == True:
                    node.start_time = max(finish_times) + 1
                    node.finish_time = node.start_time + node.duration

            for neighbor in graph.graph[node]:
                if neighbor not in visited:
                    queue.append(neighbor)
    # for task in graph.get_list_task():
    #     print(task.info())

def get_task_overlap_workforce(graph: Graph, root_task: Task):
    start_time = root_task.start_time
    finish_time = root_task.finish_time
    
    task_overlap = []
    workload = None
    # Check workforce is used by task
    if root_task.task_id not in ['START', 'END']:
        workforce = graph.assigned_workforce[root_task.task_id]
        workload = []
        for task_id in graph.assigned_workforce:
            if workforce == graph.assigned_workforce[task_id]:
                workload.append(graph.node_mapping[task_id])

        if root_task in workload:
            for task in workload:
                if task != root_task:
                    overlap_unit_times = list(set(list(range(task.start_time,task.finish_time + 1))) & set(list(range(start_time,finish_time+1))))
                    if len(overlap_unit_times) > 0:
                        task_overlap.append(task)

    return task_overlap
    

def update_overlap_bfs(graph: Graph, start_node: Task, list_workforce: list):

    visited = set()
    queue = deque([start_node])
    tasks = []
    
    completed_time = 0
    while queue:
        node = queue.popleft()
        if node not in visited:
            visited.add(node)

            overlaped_task = get_task_overlap_workforce(graph = graph, root_task = node)
            for task in overlaped_task:
                task.start_time =  node.finish_time + 1
                task.finish_time = task.start_time + task.duration
                bfs(start_node = task, graph = graph)
  

            for neighbor in graph.graph[node]:
                if neighbor not in visited:
                    queue.append(neighbor)

        if node.task_id == 'END':
            completed_time = node.finish_time
    
    return completed_time

def get_completed_time(graph: Graph, start_node: Task, list_workforce: list):
    completed_time = update_overlap_bfs(graph, start_node, list_workforce)
    return completed_time


