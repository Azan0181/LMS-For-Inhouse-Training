using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// A training video that belongs to a Course.
/// Videos are ordered within a course and may have QuizCheckpoints at specific timestamps.
/// </summary>
public class Video
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Path or URL to the video file (stored in wwwroot/uploads/videos/).</summary>
    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    /// <summary>Controls the playback order within the course (0, 1, 2 …).</summary>
    public int OrderIndex { get; set; }

    /// <summary>Total duration of the video in seconds.</summary>
    public int DurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ──────────────────────────────────────────
    public Course Course { get; set; } = null!;
    public ICollection<QuizCheckpoint> Checkpoints { get; set; } = new List<QuizCheckpoint>();
    public ICollection<VideoProgress> VideoProgresses { get; set; } = new List<VideoProgress>();
}
