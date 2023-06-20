import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

import numpy as np



def get_unique_skill_lv(req_skill_tasks: dict):
    unique_skill_lvs = []
    for _, req_skill in req_skill_tasks.items():
       unique_skill_lvs.extend(list(req_skill.keys()))
    return set(unique_skill_lvs)


def skill_mapping(unique_skill, req_skill):
    return [req_skill.get(skill, 0 ) for skill in unique_skill]


def convert_skill_task2vec(req_skill_tasks, unique_skils):
    task_names = []
    skill_task_vectors = []
    for task_name, req_skill in req_skill_tasks.items():
        skill_task_vectors.append(skill_mapping(req_skill=req_skill, unique_skill=unique_skils))
        task_names.append(task_name)
    
    return task_names, skill_task_vectors


def calculate_similarity(req_skill_matrix):
    cosine_similariries = cosine_similarity(req_skill_matrix)
    euclid_similarities = []
    for i in range(len(req_skill_matrix)):
        _row = []
        for j in range(len(req_skill_matrix)):
            customize_euclid = max(1, np.linalg.norm(req_skill_matrix[j] - req_skill_matrix[i]))
            _row.append(customize_euclid)
        euclid_similarities.append(_row)

    return cosine_similariries/euclid_similarities

