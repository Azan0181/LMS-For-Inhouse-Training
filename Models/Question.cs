using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// A multiple-choice question that belongs to a QuizCheckpoint.
/// Each question has several AnswerOptions, exactly one of which is correct.
/// </summary>
public class Question
{
    public int Id { get; set; }

    public int CheckpointId { get; set; }

    [Required, StringLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>Display order within the checkpoint (0, 1, 2 …).</summary>
    public int OrderIndex { get; set; }

    /// <summary>How many points this question is worth.</summary>
    public int Points { get; set; } = 1;

    // ── Navigation properties ──────────────────────────────────────────
    public QuizCheckpoint Checkpoint { get; set; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
