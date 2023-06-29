using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections;
namespace defaultnamespace
{
    public class ScheduleEstimator
    {
        
        int startTask;
        int finishTask;
        public int sizeOfTask = 0;

        public List<int> stortedUnitTimes = new List<int>();
        public List<int> durationTasks = new List<int>();
        public List<List<int>> TaskAdjacency = new List<List<int>>();
        public List<List<int>> taskOfSkill = new List<List<int>>();
        public List<List<int>> taskOfStartFinishTime;
        public List<List<int>> taskOfSortedUnitTime;
    
        public ScheduleEstimator(List<int> durationTasks, List<List<int>> taskOfSkill, List<List<int>> TaskAdjacency){
            this.taskOfSkill = taskOfSkill;
            this.TaskAdjacency = TaskAdjacency;
            this.durationTasks = durationTasks;

            this.Init();

        }

        private void Init(){

            // size of task
            sizeOfTask = this.TaskAdjacency[0].Count;

            // matrix task x start_finish
            taskOfStartFinishTime  = Enumerable.Repeat(new List<int>(), this.sizeOfTask).Select(x => Enumerable.Repeat(0, 2).ToList()).ToList();
            
            // start task, finish task
            this.startTask = 0;
            this.finishTask = sizeOfTask - 1;

        }

        private bool isVailableWorkforce(List<int> taskUnitTime, List<int> workForcefUnitTime){
            bool isAvailable = true;
            Parallel.ForEach(
                    Partitioner.Create(0, workForcefUnitTime.Count),
                    (range) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            int result = taskUnitTime[i] * workForcefUnitTime[i];
                            if (result > 0){
                                isAvailable = false;
                                break;
                            }
                        }
                    });

            return isAvailable;
        }


        private int mappingScore(List<int> keySkills, List<int> querySkills){
            int overallScore = 0;
            for (int i =0; i < querySkills.Count; i++)
            {
                if (keySkills[i] > 0 & querySkills[i] > 0){
                    overallScore += 1;
                }
                
            }

            return overallScore;
        }

        private List<int> getAvailableWorkforceIndexes(List<int> taskUnitTime, List<List<int>> assignedWorkForceOfUnitTime){
            List<int> workforceIndexes = new List<int>();

            // Perform element-wise addition
            Parallel.ForEach(
                Partitioner.Create(0, assignedWorkForceOfUnitTime.Count),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        bool isVailable = isVailableWorkforce(taskUnitTime, assignedWorkForceOfUnitTime[i]);
                        if (isVailable){
                            workforceIndexes.Add(i);
                        }
                    }
                    
                });

            return workforceIndexes;
        }


        private int getBestWorkforceIndex(List<int> requiredSkills, List<int> indexes,  List<List<int>> workforceOfSkill){
            
            List<int> scores = Enumerable.Repeat(0, indexes.Count).ToList();

            // Perform element-wise addition
            Parallel.ForEach(
                Partitioner.Create(0, indexes.Count),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        int score = mappingScore(requiredSkills, workforceOfSkill[indexes[i]]);
                        scores[i]  = score;
                    }
                    
                });

            
            int maxScore = scores.Max();
            
            if (maxScore < 2){
                return -1;
            }
            
            int maxIndex = scores.IndexOf(maxScore);
            return indexes[maxIndex];
        }

        public List<int> mergeHighSkill(List<int> workforceSkills, List<int> requiredSkills){
            List<int> mergedWorkforceSkills = Enumerable.Repeat(0, workforceSkills.Count).ToList();
            for (int i = 0; i < workforceSkills.Count; i ++){
                if (workforceSkills[i] < requiredSkills[i]){
                    mergedWorkforceSkills[i] = requiredSkills[i];
                }
                else{
                    mergedWorkforceSkills[i]  = workforceSkills[i];
                }


            }
            return mergedWorkforceSkills;
        }

        public void Fit(){
            
            bool[] visited = new bool[sizeOfTask];
            
            List<List<int>> assignedWorkforceOfTask = new List<List<int>>(); 
            List<List<int>> assignedWorkforceOfSkill = new List<List<int>>(); 
            List<List<int>> assignedWorkForceOfUnitTime = new List<List<int>>(); 
            
      
            Queue<int> queue = new Queue<int>();

            queue.Enqueue(this.startTask);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();

                if (visited[v] == false){
                    visited[v] = true; 
                    
                    int bestIndex = -1;
                    List<int> taskUnitTime = taskOfSortedUnitTime[v];
                    if (assignedWorkForceOfUnitTime.Count > 0){
                        // Kiểm tra xem những workforce chưa được assign trong khoảng [startTime, finishTime]
                        List<int> indexes = getAvailableWorkforceIndexes(taskUnitTime, assignedWorkForceOfUnitTime);

                        
                        if (indexes.Count > 0){
                            // Tìm workforce có độ tương đồng cao nhất với điều kiện trùng lặp ít nhất 2 skills
                            bestIndex = getBestWorkforceIndex(taskOfSkill[v], indexes, assignedWorkforceOfSkill);
                        }

                    }
                    
                    // Nếu có thì update workForceOfUnitTime, workforceOfTasks, skillOfWorkforces
                    if (bestIndex != -1){
                        // Cập nhật workForceOfUnitTime
                        for (int i = 0; i < taskUnitTime.Count; i++){
                            if (taskUnitTime[i] == 1){
                                assignedWorkForceOfUnitTime[bestIndex][i] = 1;
                            }
                        }
                        // Cập nhật workforceOfTasks
                        assignedWorkforceOfTask[bestIndex][v] = 1;
                        // Cập nhật skillOfWorkforces
                        assignedWorkforceOfSkill[bestIndex] = this.mergeHighSkill(assignedWorkforceOfSkill[bestIndex], taskOfSkill[v]);

                    }

                    // Nếu không có workforce nào thì tạo mới
                    if (bestIndex == -1){

                        // Thêm mới row assignedUnitTime vào workForceOfUnitTime cho một workforce mới
                        List<int> assignedUnitTime = Enumerable.Repeat(0, this.stortedUnitTimes.Count).ToList();
                        List<int> assignedTask = Enumerable.Repeat(0, this.sizeOfTask).ToList();
                        List<int> skillWorkforce = Enumerable.Repeat(0, this.taskOfSkill[0].Count).ToList();
                        
                        // Thêm mới row vào assignedUnitTime
                        assignedWorkForceOfUnitTime.Add(taskUnitTime);
                        
                        // Thêm mới row vào workforceOfTasks
                        assignedTask[v] = 1;
                        assignedWorkforceOfTask.Add(assignedTask);
                
                        assignedWorkforceOfSkill.Add(this.taskOfSkill[v]);

                    }

                }

                // cuối cùng, thực hiện enque các node ở level tiếp theo         
                for (int j = 0; j < this.TaskAdjacency[v].Count; j++){
                    if (this.TaskAdjacency[j][v] == 1){
                        if (visited[j] == false){
                            queue.Enqueue(j);
                        }  
                    }
                    
                }  
                
            }

            for (int i = 0; i < assignedWorkforceOfSkill.Count; i ++){
                Console.WriteLine("[" + string.Join(",", assignedWorkforceOfSkill[i]) + "]");
            }

            
        }

        public void ForwardMethod(){

            List<int> unitTimes = new List<int>();
            
            // BFS
            bool[] visited = new bool[sizeOfTask];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(this.startTask);
            

            while (queue.Count > 0){
                int v = queue.Dequeue();
                bool isVisitedAllPredencors = true;
                int ES = this.taskOfStartFinishTime[v][0];
                int EF = this.taskOfStartFinishTime[v][1];
                int duration =  durationTasks[v];
                
                
                // 1. kiểm tra xem các task trước v đã được duyệt chưa
                if (v != 0){
                    for (int i = 1; i < sizeOfTask; ++i){
                        if ( this.TaskAdjacency[v][i] == 1){                 
                            if (visited[i] == false){
                                isVisitedAllPredencors = false;
                                break;
                            }
                            else{
                                if (ES <  this.taskOfStartFinishTime[i][1] + 1){
                                    ES =  this.taskOfStartFinishTime[i][1] + 1;
                                }
                            }
                        }
                    }
                
                }

                // nếu toàn bộ task trước v đã được duyệt
                // thêm task đó vào visited 
                // Cập nhật start time của task hiện tại =  finish time muộn nhất của Predencors + 1
                // Cập nhật finish time của task hiện tại =  start time + duration - 1
                if (isVisitedAllPredencors == true & visited[v] == false){
                    if (v != 0){
                        // here
                    }
                    visited[v] = true; // thêm task đó vào visited 
           
                    // if (v != this.startTask){
                    EF = ES + duration; 
                    if (v != this.finishTask & v != this.startTask){
                        EF = ES + duration - 1; // Cập nhật early finish  
                    }
                    // }

                    if (!unitTimes.Contains(EF))
                    {
                         unitTimes.Add(EF);
                    }

                    if (!unitTimes.Contains(ES))
                    {
                         unitTimes.Add(ES);
                    }
                }

                // Update
                this.taskOfStartFinishTime[v][0] = ES;
                this.taskOfStartFinishTime[v][1] = EF;

                // cuối cùng, thực hiện enque các node ở level tiếp theo
                for (int i = 0; i <  this.TaskAdjacency[v].Count; i++){
                    if (  this.TaskAdjacency[i][v] == 1){
                        if (visited[i] == false){
                            queue.Enqueue(i);
                        }  
                    }
                    
                }
                
            }

            unitTimes.Sort();
            this.stortedUnitTimes = unitTimes;
            
            // setup matrix task x unit time
            setupTaskOfUnitTime();
        }

        private void setupTaskOfUnitTime(){

            // matrix task x start_finish
            taskOfSortedUnitTime  = Enumerable.Repeat(new List<int>(), this.sizeOfTask).Select(x => Enumerable.Repeat(0, this.stortedUnitTimes.Count).ToList()).ToList();

            for (int i = 0; i < this.TaskAdjacency.Count; i++){
                for (int j = 0; j < this.stortedUnitTimes.Count; j++){
                    if (this.stortedUnitTimes[j] >=this.taskOfStartFinishTime[i][0] & this.stortedUnitTimes[j] <= this.taskOfStartFinishTime[i][1]){
                        taskOfSortedUnitTime[i][j] = 1;
                    }
                    
                }
            }


        }

    }
}
