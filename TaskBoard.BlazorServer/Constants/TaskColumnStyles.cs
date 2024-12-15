namespace TaskBoard.BlazorServer.Constants
{
    public static class TaskColumnStyles
    {
        public const string BaseHeader = "task-column-header text-white";
        public const string TodoHeader = "task-column-header-todo";
        public const string InProgressHeader = "task-column-header-in-progress";
        public const string DoneHeader = "task-column-header-done";

        public static string GetHeaderClass(Domain.Enums.TaskState state)
        {
            return state switch
            {
                Domain.Enums.TaskState.Todo => $"{BaseHeader} {TodoHeader}",
                Domain.Enums.TaskState.InProgress => $"{BaseHeader} {InProgressHeader}",
                Domain.Enums.TaskState.Done => $"{BaseHeader} {DoneHeader}",
                _ => BaseHeader
            };
        }
    }
}