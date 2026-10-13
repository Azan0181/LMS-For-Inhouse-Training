namespace VoyageFreightLMS.Models.ViewModels;

public class DashboardViewModel
{
    // ── Summary Cards ──────────────────────────────────────────────────
    public int TotalEmployees { get; set; }
    public int TotalCourses { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public double OverallCompletionRate { get; set; }

    // ── Tables ─────────────────────────────────────────────────────────
    public List<EmployeeSummary> Employees { get; set; } = new();
    public List<CourseSummary> Courses { get; set; } = new();
    public List<CourseAssignment> RecentAssignments { get; set; } = new();
}

public class EmployeeSummary
{
    public ApplicationUser Employee { get; set; } = null!;
    public int AssignedCourses { get; set; }
    public int CompletedCourses { get; set; }
    public double AverageScore { get; set; }
    public double CompletionRate { get; set; }
}

public class CourseSummary
{
    public Course Course { get; set; } = null!;
    public int TotalAssignments { get; set; }
    public int CompletedCount { get; set; }
    public double AverageScore { get; set; }
    public double CompletionRate { get; set; }
}

public class EmployeeDetailViewModel
{
    public ApplicationUser Employee { get; set; } = null!;
    public List<AssignmentDetail> Assignments { get; set; } = new();
    public double OverallAverageScore { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalAssigned { get; set; }
}

public class AssignmentDetail
{
    public CourseAssignment Assignment { get; set; } = null!;
    public double ProgressPercent { get; set; }
    public int VideosCompleted { get; set; }
    public int TotalVideos { get; set; }
    public double AverageQuizScore { get; set; }
}

public class AssignCourseViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<string> SelectedEmployeeIds { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public List<ApplicationUser> AvailableEmployees { get; set; } = new();
}
