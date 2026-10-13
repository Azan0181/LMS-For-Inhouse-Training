using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// One selectable option for a multiple-choice Question.
/// Exactly one option per question should have IsCorrect = true.
/// </summary>
public class AnswerOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    [Required, StringLength(500)]
    public string OptionText { get; set; } = string.Empty;

    /// <summary>True if this is the correct answer.</summary>
    public bool IsCorrect { get; set; }

    /// <summary>Display order within the question (0, 1, 2 …).</summary>
    public int OrderIndex { get; set; }

    // ── Navigation properties ──────────────────────────────────────────
    public Question Question { get; set; } = null!;
}
