namespace VoyageFreightLMS.Models;

/// <summary>
/// Tracks an employee's viewing progress for a single Video within a CourseAssignment.
/// Stores the last playback position so they can resume, and whether they finished the video.
/// </summary>
public class VideoProgress
{
    public int Id { get; set; }

    public int CourseAssignmentId { get; set; }
    public int VideoId { get; set; }

    /// <summary>The furthest second the employee has watched to.</summary>
    public int LastWatchedPositionSeconds { get; set; }

    /// <summary>True once the employee has watched the entire video and passed all checkpoints.</summary>
    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation properties ──────────────────────────────────────────
    public CourseAssignment CourseAssignment { get; set; } = null!;
    public Video Video { get; set; } = null!;
}
