using defaultnamespace;

public class Program{
    public static void Main(string[] args){
        List<int> durationOfTasks = new List<int>() {  0, 6, 3, 4, 3, 2, 2, 0};
        List<List<int>> skillOfTasks = new List<List<int>>()
        {
            new List<int>() { 0, 2, 2, 1, 0, 0, 5 , 0},
            new List<int>() { 0, 1, 3, 0, 0, 1, 0 , 0},
            new List<int>() { 0, 0, 0, 0, 2, 2, 0 , 0},
            new List<int>() { 0, 1, 0, 4, 0, 0, 0 , 0},
            new List<int>() { 0, 1, 1, 2, 0, 0, 0 , 0},
        
            
        };

        List<List<int>> predenceTasks = new List<List<int>>()
        {

            new List<int>() { 0, 1, 1, 0, 0, 0, 0 , 0},
            new List<int>() { 0, 0, 0, 1, 1, 0, 0 , 0},
            new List<int>() { 0, 0, 0, 0, 0, 1, 0 , 0},
            new List<int>() { 0, 0, 0, 0, 0, 0, 1 , 0},
            new List<int>() { 0, 0, 0, 0, 0, 0, 1 , 0},
            new List<int>() { 0, 0, 0, 0, 0, 0, 0 , 1},
            new List<int>() { 0, 0, 0, 0, 0, 0, 0 , 1},
            new List<int>() { 0, 0, 0, 0, 0, 0, 0 , 0},

        };
        
        Graph graph = new Graph(0, 7, durationOfTasks, skillOfTasks, predenceTasks);
        graph.UpdateTaskSchedule();
        graph.estimateAssignment();



       
    }
}