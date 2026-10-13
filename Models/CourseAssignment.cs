using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models;

/// <summary>
/// Represents the assignment of a Course to an Employee.
/// Tracks who assigned it, when, the due date, and overall completion status.
/// </summary>
public class CourseAssignment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    /// <summary>The employee who must complete the course.</summary>
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>The HR / Manager / Admin who assigned the course.</summary>
    public string AssignedById { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotStarted;
    public DateTime? CompletedAt { get; set; }

    // ── Navigation properties ──────────────────────────────────────────
    public Course Course { get; set; } = null!;
    public ApplicationUser Employee { get; set; } = null!;
    public ApplicationUser AssignedBy { get; set; } = null!;
    public ICollection<VideoProgress> VideoProgresses { get; set; } = new List<VideoProgress>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
