from task import Task

class WorkforceResource:
    def __init__(self, workforce_id, salary_unit, skills):
        self.workforce_id = workforce_id
        self.skills = skills
        self.salary_unit = salary_unit
        self.workload = []
        self.color = None

    def set_color(self, color):
        self.color = color
        
    def info(self):
        return {
            "workforce_id": self.workforce_id,
            "skills": self.skills,
            "salary_unit": self.salary_unit,
            "workforce_id": self.workforce_id
    
        }
    def add_task2workload(self, task: Task):
        self.workload.append(task)
