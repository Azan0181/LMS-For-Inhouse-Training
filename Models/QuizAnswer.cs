namespace VoyageFreightLMS.Models;

/// <summary>
/// The answer an employee selected for one Question within a QuizAttempt.
/// </summary>
public class QuizAnswer
{
    public int Id { get; set; }

    public int QuizAttemptId { get; set; }
    public int QuestionId { get; set; }

    /// <summary>Which option the employee selected (null if they skipped).</summary>
    public int? SelectedAnswerOptionId { get; set; }

    /// <summary>Snapshot of whether this answer was correct at submission time.</summary>
    public bool IsCorrect { get; set; }

    // ── Navigation properties ──────────────────────────────────────────
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public AnswerOption? SelectedAnswerOption { get; set; }
}
