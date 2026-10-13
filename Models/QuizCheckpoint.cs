using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// A quiz checkpoint inserted at a specific timestamp within a Video.
/// When the video reaches this timestamp it pauses and forces the employee to answer
/// the attached Questions before they can continue watching.
/// </summary>
public class QuizCheckpoint
{
    public int Id { get; set; }

    public int VideoId { get; set; }

    /// <summary>The second in the video when the checkpoint fires (e.g. 120 = 2:00).</summary>
    [Required]
    public int TimestampSeconds { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>If true the employee must pass this checkpoint to continue.</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>Minimum score percentage needed to pass (0–100).</summary>
    [Range(0, 100)]
    public int PassingScorePercent { get; set; } = 100;

    // ── Navigation properties ──────────────────────────────────────────
    public Video Video { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
