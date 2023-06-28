using System;
using System.Collections.Generic;
using System.Linq;

namespace defaultnamespace
{
    public class Graph
    {
        
        int startTask;
        int endTask;
        public int sizeOfTask = 0;

        public List<int> startTimeTasks  = new List<int>();
        public List<int> finishTimeTasks  = new List<int>();
        public List<int>  allTasks = new List<int>();
        public List<int> stortedUnitTimes = new List<int>();
        public List<int> durationTasks = new List<int>();
        public List<List<int>> predenceOfTasks = new List<List<int>>();
        public List<List<int>> skillOfTask = new List<List<int>>();

        

        public Graph(
            int startTask,  int endTask, List<int> durationTasks, List<List<int>> skillOfTask, List<List<int>> predenceOfTasks){
            
            this.startTask = startTask;
            this.endTask = endTask;
            this.skillOfTask = skillOfTask;
            this.predenceOfTasks = predenceOfTasks;
            this.durationTasks = durationTasks;

            // size of task
            sizeOfTask = predenceOfTasks[0].Count;


            

        }

        public List<int> getUnassignedWorkforceIndex(int indexStartTime, int indexFinishTime, List<List<int>> assignedWorkForceOfUnitTime){
            List<int> workforceIndex = new List<int>();
            for(int i = 0; i<assignedWorkForceOfUnitTime.Count; i++){
                bool avaiable = true;
                for(int j = indexStartTime; j<=indexFinishTime; j++){
                    if (assignedWorkForceOfUnitTime[i][j] ==1){
                        avaiable = false;
                        break;
                    }
                }
                if (avaiable){
                    workforceIndex.Add(i);    
                }
                
            }
            
            return workforceIndex;
        }


        public int getWorkForceWithBestScore(List<int> requiredSkills, List<int> workforceIndex, List<List<int>> workforceOfSkill){
            
            int bestSimarityScore = 0;
            int bestWorkforce = -1;

            foreach (int index in workforceIndex){
                List<int> workforceSkills = workforceOfSkill[index];
                int  simarityScore  = 0;
                for (int i = 0; i < workforceSkills.Count; i ++){
                    if (workforceSkills[i] > 0 & requiredSkills[i] > 0 ){
                        simarityScore += 1;
                    }
                
                }
                if (simarityScore > bestSimarityScore & simarityScore >= 2 ){
                    bestWorkforce = index;
                }
            }

            return bestWorkforce;
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

        public int getStartIndex(int v){
            int index = -1;
            for (int i = 0; i < this.stortedUnitTimes.Count; i++){
                if ( this.startTimeTasks[v] == this.stortedUnitTimes[i]){
                    index = i;
                    break;
                }
            }

            return index;

        }

        public int getFinishIndex(int v){
            int index = -1;
            for (int i = 0; i < this.stortedUnitTimes.Count; i++){
                if ( this.finishTimeTasks[v] == this.stortedUnitTimes[i]){
                    index = i;
                    break;
                }
            }

            return index;

        }



        public void estimateAssignment(){
            
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
                    
                    List<int> requiredSkills = Enumerable.Repeat(0, this.skillOfTask.Count).ToList();
                    // lấy index start time và index finish time trong stortedUnitTimes
                    int indexStartTime = getStartIndex(v);
                    int indexFinishTime = getFinishIndex(v);
                    
                    for (int i = 0; i < this.skillOfTask.Count ; i++){
                        requiredSkills[i] = this.skillOfTask[i][v];
                    }

                    // Kiểm tra xem những workforce chưa được assign trong khoảng [startTime, finishTime]
                    List<int> workforceIndex =  this.getUnassignedWorkforceIndex(indexStartTime, indexFinishTime, assignedWorkForceOfUnitTime);
                    int bestWorkforceIndex = -1;
                    if (workforceIndex.Count > 0){
                        
                        // Tìm workforce có độ tương đồng cao nhất với điều kiện trùng lặp ít nhất 2 skills
                        bestWorkforceIndex = this.getWorkForceWithBestScore(requiredSkills, workforceIndex,  assignedWorkforceOfSkill);
                        
                        // Nếu có thì update workForceOfUnitTime, workforceOfTasks, skillOfWorkforces
                        if (bestWorkforceIndex != -1){
                            
                            // Cập nhật workForceOfUnitTime
                            for (int i = indexStartTime; i <= indexFinishTime; i++){
                                assignedWorkForceOfUnitTime[bestWorkforceIndex][i] = 1;
                            }

                            // Cập nhật workforceOfTasks
                            assignedWorkforceOfTask[bestWorkforceIndex][v] = 1;

                            // Cập nhật skillOfWorkforces
                            assignedWorkforceOfSkill[bestWorkforceIndex] = this.mergeHighSkill(assignedWorkforceOfSkill[bestWorkforceIndex], requiredSkills);

                        }
                    }
                   
                    // Nếu không có workforce nào thì tạo mới
                    if (bestWorkforceIndex == -1){

                        // Thêm mới row assignedUnitTime vào workForceOfUnitTime cho một workforce mới
                        List<int> assignedUnitTime = Enumerable.Repeat(0, this.stortedUnitTimes.Count).ToList();
                        List<int> assignedTask = Enumerable.Repeat(0, this.sizeOfTask).ToList();
                        List<int> skillWorkforce = Enumerable.Repeat(0, this.skillOfTask.Count).ToList();
                        
                        // Thêm mới row vào assignedUnitTime
                        for (int i = indexStartTime; i <= indexFinishTime; i++){
                            assignedUnitTime[i] = 1;
                        }
                        assignedWorkForceOfUnitTime.Add(assignedUnitTime);
                        
                        // Thêm mới row vào workforceOfTasks
                        assignedTask[v] = 1;
                        assignedWorkforceOfTask.Add(assignedTask);
                        
                        // Thêm mới row vào assignedWorkforceOfSkill
                        for (int i = 0; i < this.skillOfTask.Count; i++){
                            skillWorkforce[i] = requiredSkills[i];
                        }

                        assignedWorkforceOfSkill.Add(skillWorkforce);

                    }

                }

                // cuối cùng, thực hiện enque các node ở level tiếp theo         
                List<int> predenceTask  = this.predenceOfTasks[v];
                for (int j = 0; j < predenceTask.Count; j++){
                    if (predenceTask[j] == 1){
                        if (visited[j] == false){
                            queue.Enqueue(j);
                        }  
                    }
                    
                }  
                
            }

            for (int i = 0; i < assignedWorkforceOfSkill.Count; i ++){
                Console.WriteLine("W_" + i + ": [" + string.Join(",", assignedWorkforceOfSkill[i]) + "]");
            }

            
        }

        public void UpdateTaskSchedule()
        {
            bool[] visited = new bool[sizeOfTask];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(this.startTask);

            this.startTimeTasks  = Enumerable.Repeat(0, sizeOfTask).ToList();
            this.finishTimeTasks = Enumerable.Repeat(0, sizeOfTask).ToList();

            // Start Task (Task định danh bắt đầu)
            this.startTimeTasks[this.startTask] = 0;
            this.finishTimeTasks[this.startTask] = 0;

            List<int> unitTimes = new List<int>(); //  là start time, end time của tất cả các task
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                
                // 1. kiểm tra xem các task trước v đã được duyệt chưa
                bool isVisitedPredencors = true;
                int maxFinishTime = 0;
                for (int i = 0; i < sizeOfTask; ++i){
                    if (this.predenceOfTasks[i][v] == 1){
                        
                        if (visited[i] == false){
                            isVisitedPredencors = false;
                            break;
                        }
                        else{
                            if (maxFinishTime < this.finishTimeTasks[i]){
                                maxFinishTime = this.finishTimeTasks[i];
                            }
                        }
                    }
                }
                
                // nếu toàn bộ task trước v đã được duyệt:
                // thêm task đó vào visited 
                // Cập nhật start time của task hiện tại =  finish time muộn nhất của Predencors + 1
                // Cập nhật finish time của task hiện tại =  start time + duration - 1
                
                if (isVisitedPredencors == true & visited[v] == false){
                    visited[v] = true; // thêm task đó vào visited 
                    if (v != this.startTask){
                        this.startTimeTasks[v] = maxFinishTime + 1; // Cập nhật start time
                        if (v == this.endTask){
                            
                            this.finishTimeTasks[v] = this.startTimeTasks[v]  + this.durationTasks[v]; // Cập nhật finish time 
                        }
                        else{
                            this.finishTimeTasks[v] = this.startTimeTasks[v]  + this.durationTasks[v] - 1; // Cập nhật finish time 

                        }
                    }

                    // add to unitTimes
                    if (!unitTimes.Contains(this.finishTimeTasks[v]))
                        {
                            unitTimes.Add(this.finishTimeTasks[v]);
                        }

                    if (!unitTimes.Contains(this.startTimeTasks[v]))
                        {
                            unitTimes.Add(this.startTimeTasks[v]);
                        }
                }

                // cuối cùng, thực hiện enque các node ở level tiếp theo
                List<int> predenceTask  = this.predenceOfTasks[v];
                for (int j = 0; j < predenceTask.Count; j++){
                    if (predenceTask[j] == 1){
                        if (visited[j] == false){
                            queue.Enqueue(j);
                        }  
                    }
                    
                }
            }

            // sort unitTimes
            unitTimes.Sort();
            this.stortedUnitTimes = unitTimes;
        }
    }
}
