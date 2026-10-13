using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models.ViewModels;

public class VideoUploadViewModel
{
    public int CourseId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Video File")]
    public IFormFile VideoFile { get; set; } = null!;

    [Display(Name = "Order")]
    public int OrderIndex { get; set; }

    public int DurationSeconds { get; set; }
}

public class VideoEditViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Order")]
    public int OrderIndex { get; set; }

    public int DurationSeconds { get; set; }

    public string? ExistingVideoUrl { get; set; }

    [Display(Name = "Replace Video (optional)")]
    public IFormFile? VideoFile { get; set; }
}

public class VideoPlayerViewModel
{
    public Video Video { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public CourseAssignment Assignment { get; set; } = null!;
    public VideoProgress? Progress { get; set; }
    public List<CheckpointWithStatus> Checkpoints { get; set; } = new();
    public Video? PreviousVideo { get; set; }
    public Video? NextVideo { get; set; }
}

public class CheckpointWithStatus
{
    public QuizCheckpoint Checkpoint { get; set; } = null!;
    public bool IsPassed { get; set; }
    public List<Question> Questions { get; set; } = new();
}
