using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models.ViewModels;

public class CourseCreateViewModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Display(Name = "Difficulty Level")]
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

    [Display(Name = "Estimated Duration (minutes)")]
    [Range(1, 10000)]
    public int EstimatedDurationMinutes { get; set; }

    public IFormFile? Thumbnail { get; set; }
}

public class CourseEditViewModel : CourseCreateViewModel
{
    public int Id { get; set; }
    public string? ExistingThumbnailUrl { get; set; }
}

public class CourseDetailsViewModel
{
    public Course Course { get; set; } = null!;
    public List<Video> Videos { get; set; } = new();

    /// <summary>Only populated for employees viewing their own progress.</summary>
    public CourseAssignment? Assignment { get; set; }
    public Dictionary<int, VideoProgress?> VideoProgressMap { get; set; } = new();
    public int CompletedVideos { get; set; }
    public int TotalVideos { get; set; }
    public double OverallProgressPercent { get; set; }
    public List<CourseAssignment> EnrolledAssignments { get; set; } = new();
}
