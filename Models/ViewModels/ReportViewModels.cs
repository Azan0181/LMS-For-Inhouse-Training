namespace VoyageFreightLMS.Models.ViewModels;

/// <summary>
/// View-model for the per-employee progress report page.
/// Contains everything needed to render charts + downloadable content.
/// </summary>
public class EmployeeProgressReportViewModel
{
    public ApplicationUser Employee { get; set; } = null!;

    /// <summary>Overall stats across all assigned courses.</summary>
    public int TotalAssigned  { get; set; }
    public int TotalCompleted { get; set; }
    public double OverallCompletionPercent { get; set; }

    /// <summary>Per-course detail rows.</summary>
    public List<CourseProgressRow> Courses { get; set; } = new();

    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CourseProgressRow
{
    public CourseAssignment Assignment { get; set; } = null!;

    public string CourseTitle   { get; set; } = string.Empty;
    public int    TotalVideos   { get; set; }
    public int    VideosWatched { get; set; }

    /// <summary>0-100 percentage of videos completed.</summary>
    public double CompletionPercent { get; set; }

    /// <summary>Average quiz score across all passed attempts (0-100).</summary>
    public double AvgQuizScore { get; set; }

    public AssignmentStatus Status      { get; set; }
    public DateTime?        DueDate     { get; set; }
    public DateTime?        CompletedAt { get; set; }

    /// <summary>CSS colour class for the status badge.</summary>
    public string StatusBadgeClass => Status switch
    {
        AssignmentStatus.Completed  => "bg-success",
        AssignmentStatus.InProgress => "bg-warning text-dark",
        _                           => "bg-secondary"
    };

    /// <summary>Label text for the status badge.</summary>
    public string StatusLabel => Status switch
    {
        AssignmentStatus.Completed  => "Completed",
        AssignmentStatus.InProgress => "In Progress",
        _                           => "Not Started"
    };
}
