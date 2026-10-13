using System.ComponentModel.DataAnnotations;

namespace VoyageFreightLMS.Models.ViewModels;

public class CheckpointCreateViewModel
{
    public int VideoId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Timestamp (seconds)")]
    [Range(1, 360000)]
    public int TimestampSeconds { get; set; }

    [Display(Name = "Passing Score %")]
    [Range(0, 100)]
    public int PassingScorePercent { get; set; } = 100;
}

public class QuestionCreateViewModel
{
    public int CheckpointId { get; set; }

    [Required, StringLength(1000)]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Points { get; set; } = 1;

    /// <summary>The answer option texts. At least 2 required.</summary>
    public List<string> Options { get; set; } = new() { "", "", "", "" };

    /// <summary>Index (0-based) of the correct option.</summary>
    [Display(Name = "Correct Answer")]
    public int CorrectOptionIndex { get; set; }
}

public class ManageCheckpointsViewModel
{
    public Video Video { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public List<QuizCheckpoint> Checkpoints { get; set; } = new();
}

/// <summary>JSON payload sent from the video player when an employee submits a quiz.</summary>
public class QuizSubmitModel
{
    public int CheckpointId { get; set; }
    public int AssignmentId { get; set; }
    public List<QuizAnswerSubmit> Answers { get; set; } = new();
}

public class QuizAnswerSubmit
{
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
}

/// <summary>JSON response returned after quiz submission.</summary>
public class QuizResultModel
{
    public bool Passed { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public int ScorePercent { get; set; }
    public int RequiredPercent { get; set; }
    public List<QuizAnswerResult> AnswerResults { get; set; } = new();
}

public class QuizAnswerResult
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int CorrectOptionId { get; set; }
}
