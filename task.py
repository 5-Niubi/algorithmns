# Define Task
class Task:
    def __init__(self, task_id, duration, required_skills, required_equipments, pred_tasks):
        self.task_id = task_id
        self.duration = duration
        self.required_skills = required_skills
        self.required_equipments = required_equipments
        self.start_time = None
        self.finish_time = None
        self.color = None
        self.pred_tasks = pred_tasks

        if task_id == 'START':
            self.start_time = 0
            self.finish_time = 0

    def info(self):
        
        return {
            "task_id": self.task_id,
            "duration": self.duration,
            "required_skills": self.required_skills,
            "required_equipments": self.required_equipments,
            "start_time": self.start_time,
            "finish_time": self.finish_time
        }