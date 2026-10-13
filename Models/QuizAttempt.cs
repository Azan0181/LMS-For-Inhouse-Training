namespace VoyageFreightLMS.Models;

/// <summary>
/// Records one attempt at a QuizCheckpoint by an employee.
/// Stores the score they earned and whether they passed the required threshold.
/// </summary>
public class QuizAttempt
{
    public int Id { get; set; }

    public int CourseAssignmentId { get; set; }
    public int CheckpointId { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Total points the employee earned.</summary>
    public int Score { get; set; }

    /// <summary>Maximum possible points for this checkpoint.</summary>
    public int MaxScore { get; set; }

    /// <summary>True if Score/MaxScore ≥ PassingScorePercent on the checkpoint.</summary>
    public bool IsPassed { get; set; }

    // ── Navigation properties ──────────────────────────────────────────
    public CourseAssignment CourseAssignment { get; set; } = null!;
    public QuizCheckpoint Checkpoint { get; set; } = null!;
    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}
