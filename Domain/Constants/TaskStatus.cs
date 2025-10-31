namespace Domain.Constants
{
    /// <summary>
    /// Task Status constant
    /// </summary>
    /// <remarks>This class Sets and validate statuses for tasks</remarks>
    public static class TaskStatus
    {

        public const string NonStarted = "Non Started";

        public const string InProgress = "In Progress";

        public const string Paused = "Paused";

        public const string Late = "Late";

        public const string Finished = "Finished";

        // Valid statuses array
        public static readonly string[] All =
        [
            NonStarted,
            InProgress,
            Paused,
            Late,
            Finished
        ];

        // Validate if a status is valid
        public static bool IsValid(string status)
        {
            return All.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
