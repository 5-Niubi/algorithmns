using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurobi
{
    public class DataReader
    {
        public int DayEffort { get; set; }
        public int Deadline { get; set; }
        public int Budget { get; set; }
        public int NumOfTasks { get; set; }
        public int NumOfWorkers { get; set; }
        public int NumOfSkills { get; set; }
        public int NumOfEquipments { get; set; }
        public int NumOfFunctions { get; set; }
        public int[] TaskDuration { get;set; } = new int[500];
        public int[,] TaskAdjacency { get; set; } = new int[500, 500];
        public int[,] TaskSimilarity { get; set; } = new int[500, 500];
        public int[,] TaskExper { get; set; } = new int[500, 32];
        public int[,] TaskFunction { get; set; } = new int[500, 500];
        public int[,] TaskFunctionTime { get; set; } = new int[500, 500];
        public int[,] WorkerExper { get; set; } = new int[500, 500];
        public int[,] WorkerEffort { get; set; } = new int[500, 10000];
        public int[] WorkerSalary { get; set; } = new int[500];
        public int[,] EquipmentFunction { get; set; } = new int[500, 500];
        public int[] EquipmentCost { get; set; } = new int[500];
        private const string FilePath = 
            @"/Users/pvm/Desktop/algorithmns/pharse_one/ScheduleEstimator/data/data.txt";

         private const string dataPath = 
            @"/Users/pvm/Desktop/algorithmns/1688024482";
        
        private const int BufferSize = 128;
        public DataReader(){
            
            String TaskAdjacencyPath = Path.Combine(dataPath, "task_matrix.txt");
            String TaskExperPath = Path.Combine(dataPath, "skill.txt");
            String TaskDurationPath = Path.Combine(dataPath, "duration.txt");

            // Read TaskExper file
            using (var taskExperFileStream = File.OpenRead(TaskExperPath))
            using (var streamReader = new StreamReader(taskExperFileStream, Encoding.UTF8))
            {
                string line;
                List<List<int>> taskExperList = new List<List<int>>();

                int i = 0;
                // Convert TaskExper to matrix
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] arrString = line.Split(" ");
                    for(int j = 0; j < arrString.Length; j++){
                        TaskExper[i, j] = Convert.ToInt32(arrString[j]);
                    }
                    i += 1;
                    
                }

            }

            // Read TaskAdjacency file
            using (var taskAdjacencyFileStream = File.OpenRead(TaskAdjacencyPath))
            using (var streamReader = new StreamReader(taskAdjacencyFileStream, Encoding.UTF8))
            {
                string line;
                string[] arrString;
                
                int i = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    arrString = line.Split(" ");

                    for (int j = 0; j < arrString.Length; j++)
                    {
                        TaskAdjacency[i, j] = Convert.ToInt32(arrString[j]);
                    }
                    i += 1;
                }


            }

            // Read TaskDuration file
            using (var taskDurationFileStream = File.OpenRead(TaskDurationPath))
            using (var streamReader = new StreamReader(taskDurationFileStream, Encoding.UTF8))
            {
                string line;
                string[] arrString;
               
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                
                for (int i = 0; i < arrString.Length; i++){
                    int duration = Convert.ToInt32(arrString[i]);
                    TaskDuration[i] = duration;

                }

            }


        }
        

        
      

        private void Initializer()
        {
            using(var fileStream = File.OpenRead(FilePath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                string[] arrString;

                // doc effort 1 ngay
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                DayEffort = Convert.ToInt32(arrString[0]);

                // doc so luong tasks
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                NumOfTasks = Convert.ToInt32(arrString[0]);

                // doc duration cua task
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    TaskDuration[i] = Convert.ToInt32(arrString[0]);
                }

                // doc so luong skill
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                NumOfSkills = Convert.ToInt32(arrString[0]);

                // doc so luong candidate
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                NumOfWorkers = Convert.ToInt32(arrString[0]);

                // doc so luong equipmentType
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                NumOfFunctions = Convert.ToInt32(arrString[0]);

                // doc so luong equipment
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                NumOfEquipments = Convert.ToInt32(arrString[0]);

                // doc ma tran precedences
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfTasks; j++)
                    {
                        TaskAdjacency[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc ma tran candidate experiences
                for (int i = 0; i < NumOfWorkers; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfSkills; j++)
                    {
                        WorkerExper[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc ma tran equipment type
                for (int i = 0; i < NumOfEquipments; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfFunctions; j++)
                    {
                        EquipmentFunction[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc ma tran required skill
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfSkills; j++)
                    {
                        TaskExper[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc ma tran required equipmentType
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfFunctions; j++)
                    {
                        TaskFunction[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc ma tran required equipmentTime
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfFunctions; j++)
                    {
                        TaskFunctionTime[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc salary cua candidate
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                for (int i = 0; i < NumOfWorkers; i++)
                {
                    WorkerSalary[i] = Convert.ToInt32(arrString[i]);
                }

                // doc cost equipmenttype
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                for (int i = 0; i < NumOfEquipments; i++)
                {
                    EquipmentCost[i] = Convert.ToInt32(arrString[i]);
                }

                // ma tran tuong dong
                for (int i = 0; i < NumOfTasks; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < NumOfTasks; j++)
                    {
                        TaskSimilarity[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // deadline
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                Deadline = Convert.ToInt32(arrString[0]);

                // doc candidate concentration
                for (int i = 0; i < NumOfWorkers; i++)
                {
                    line = streamReader.ReadLine();
                    arrString = line.Split(" ");
                    for (int j = 0; j < Deadline; j++)
                    {
                        WorkerEffort[i, j] = Convert.ToInt32(arrString[j]);
                    }
                }

                // doc budget
                line = streamReader.ReadLine();
                arrString = line.Split(" ");
                Budget = Convert.ToInt32(arrString[0]);

                //Utils.GenerateTaskSimilarityMatrix(AllTasks, AllSkills, AllEquipmentTypes, TaskExperiences, TaskEquipmentTime);

            }
        }
    }
}