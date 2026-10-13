namespace VoyageFreightLMS.Models;

/// <summary>
/// Tracks where an employee is in their assigned course.
/// </summary>
public enum AssignmentStatus
{
    NotStarted,
    InProgress,
    Completed
}

/// <summary>
/// Difficulty rating shown on course cards.
/// </summary>
public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
