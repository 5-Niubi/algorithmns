using defaultnamespace;
using Gurobi;
using System.Collections;
using System.Diagnostics;
public class Program{

    

    public static void Main(string[] args){

        DataReader reader = new DataReader();
        

        List<int> durationOfTasks = new List<int> (reader.TaskDuration);

        List<List<int>> taskOfSkill = new List<List<int>>();

        for (int i = 0; i < reader.TaskExper.GetLength(0); i++)
        {
            List<int> rowList = new List<int>();
            for (int j = 0; j < reader.TaskExper.GetLength(1); j++)
            {
                rowList.Add(reader.TaskExper[i, j]);
            }
            taskOfSkill.Add(rowList);
        }

        List<List<int>> predenceTasks = new List<List<int>>();

        for (int i = 0; i < reader.TaskAdjacency.GetLength(0); i++)
        {
            List<int> rowList = new List<int>();
            for (int j = 0; j < reader.TaskAdjacency.GetLength(1); j++)
            {
                rowList.Add(reader.TaskAdjacency[i, j]);
            }
            predenceTasks.Add(rowList);
        }
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ScheduleEstimator estimator = new ScheduleEstimator( durationOfTasks, taskOfSkill, predenceTasks);
        estimator.ForwardMethod();
        estimator.Fit();

        stopwatch.Stop();

        TimeSpan elapsedTime = stopwatch.Elapsed;
        Console.WriteLine("Elapsed Time: " + elapsedTime.TotalMilliseconds + " milliseconds");
        
    }
}