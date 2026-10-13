using Microsoft.AspNetCore.Identity;

namespace VoyageFreightLMS.Models;

/// <summary>
/// Extends the default Identity user with employee profile information.
/// Every person in the LMS (Employee, Trainer, HR, Manager, Admin) is an ApplicationUser.
/// Their role is determined by ASP.NET Identity Roles, not a property on this class.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? HireDate { get; set; }
    public string? ProfileImageUrl { get; set; }

    /// <summary>Helper to display "John Doe" in the UI.</summary>
    public string FullName => $"{FirstName} {LastName}";

    // ── Navigation properties ──────────────────────────────────────────
    public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
    public ICollection<CourseAssignment> Assignments { get; set; } = new List<CourseAssignment>();
    public ICollection<CourseAssignment> AssignedByMe { get; set; } = new List<CourseAssignment>();
}
