# from concurrent.futures import ThreadPoolExecutor
# from concurrent.futures import  wait
# from concurrent import futures
# from random import random
# from time import sleep

# def non_dominated_sorting_thread(population_size, chroms_obj_record, partition):
#     # population_size, chroms_obj_record, partition
#     # sleep for less than a second
#     # print(f"Bắt đầu non_dominated_sorting_thread: {partition}")
#     print(f"partition: {partition[0], partition[1]}")
#     for p in range(partition[0], partition[1]):
#         solution[p] = []
#         non_dominate[p] = 0
#         for q in range(population_size * 2):
#             is_dominated = False
#             dominated_count = 0
#             for obj in range(len(chroms_obj_record[p])):
#                 if chroms_obj_record[p][obj] > chroms_obj_record[q][obj]:
#                     dominated_count += 1
#                 elif chroms_obj_record[p][obj] < chroms_obj_record[q][obj]:
#                     is_dominated = True
#                     break
            
#             if is_dominated:
#                 if q not in s[p]:
#                     solution[p].append(q)

#             elif not is_dominated and dominated_count == len(chroms_obj_record[p]):
#                 non_dominate[p] = non_dominate[p] + 1

    
#         if non_dominate[p] == 0:
#             rank[p] = 0
#             if p not in front[0]:
#                 front[0].append(p)
#     print(f"Done!!!")
#     return 1

# def non_dominated_sorting_multithread(population_size, chroms_obj_record):
    
#     global solution
#     global non_dominate
#     global rank
#     global front
    
#     solution, non_dominate = {}, {}

#     front, rank = {}, {}
#     front[0] = []
#     partitions = []
#     num_partitions = 8

#     partition_size = population_size *2 // num_partitions
#     remainder = population_size % num_partitions

#     start = 0
#     for i in range(num_partitions):
#         end = start + partition_size
#         if remainder > 0:
#             end += 1
#             remainder -= 1
#         partitions.append((start, end))
#         start = end

#     args = [(population_size, chroms_obj_record, d) for d in partitions]
   
#     # create a thread pool
#     with ThreadPoolExecutor(4) as executor:
#         executor.map(lambda p: non_dominated_sorting_thread(*p), args)
#         # wait for all tasks to complete
#         executor.shutdown()
    
#     i = 0
#     while front[i] != []:
#         Q = []
#         for p in front[i]:
#             for q in solution[p]:
#                 non_dominate[q] = non_dominate[q] - 1
#                 if non_dominate[q] == 0:
#                     rank[q] = i + 1
#                     if q not in Q:
#                         Q.append(q)
#         i = i + 1
#         front[i] = Q

#     del front[len(front) - 1]
#     return front


# def calculate_crowding_distance(front,chroms_obj_record):
    
#     distance={m:0 for m in front}
#     for o in range(4):
#         obj={m:chroms_obj_record[m][o] for m in front}
#         sorted_keys=sorted(obj, key=obj.get)
#         distance[sorted_keys[0]]=distance[sorted_keys[len(front)-1]]=999999999999
#         for i in range(1,len(front)-1):
#             if len(set(obj.values()))==1:
#                 distance[sorted_keys[i]]=distance[sorted_keys[i]]
#             else:
#                 distance[sorted_keys[i]]=distance[sorted_keys[i]]+(obj[sorted_keys[i+1]]-obj[sorted_keys[i-1]])/(obj[sorted_keys[len(front)-1]]-obj[sorted_keys[0]])
            
#     return distance  