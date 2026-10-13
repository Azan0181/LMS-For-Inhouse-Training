using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// A training course created by a Trainer.
/// Contains one or more Videos, each of which may have QuizCheckpoints.
/// </summary>
public class Course
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    /// <summary>Foreign key → the Trainer who created this course.</summary>
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Only published courses are visible to employees.</summary>
    public bool IsPublished { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

    public int EstimatedDurationMinutes { get; set; }

    // ── Navigation properties ──────────────────────────────────────────
    public ApplicationUser CreatedBy { get; set; } = null!;
    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<CourseAssignment> Assignments { get; set; } = new List<CourseAssignment>();
}
