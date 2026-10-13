using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

/// <summary>
/// The video player controller.
/// Serves the watch page and handles AJAX calls for progress saving & quiz submission.
/// </summary>
[Authorize]
public class PlayerController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlayerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── WATCH ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Watch(int videoId, int assignmentId)
    {
        var userId = _userManager.GetUserId(User);

        var assignment = await _db.CourseAssignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && (a.EmployeeId == userId || User.IsInRole("Admin") || User.IsInRole("Trainer")));
        if (assignment == null) return NotFound();

        var video = await _db.Videos
            .Include(v => v.Course)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.CourseId == assignment.CourseId);
        if (video == null) return NotFound();

        // Mark assignment as in-progress
        if (assignment.Status == AssignmentStatus.NotStarted)
        {
            assignment.Status = AssignmentStatus.InProgress;
            await _db.SaveChangesAsync();
        }

        // Load checkpoints with questions
        var checkpoints = await _db.QuizCheckpoints
            .Include(c => c.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .Where(c => c.VideoId == videoId)
            .OrderBy(c => c.TimestampSeconds)
            .ToListAsync();

        // Load passed checkpoint IDs for this assignment
        var passedCheckpointIds = await _db.QuizAttempts
            .Where(qa => qa.CourseAssignmentId == assignmentId && qa.IsPassed)
            .Select(qa => qa.CheckpointId)
            .Distinct()
            .ToListAsync();

        // Load progress
        var progress = await _db.VideoProgresses
            .FirstOrDefaultAsync(vp => vp.CourseAssignmentId == assignmentId && vp.VideoId == videoId);

        // Previous / Next videos
        var allVideos = await _db.Videos
            .Where(v => v.CourseId == assignment.CourseId)
            .OrderBy(v => v.OrderIndex)
            .ToListAsync();

        var currentIndex = allVideos.FindIndex(v => v.Id == videoId);

        var vm = new VideoPlayerViewModel
        {
            Video = video,
            Course = video.Course,
            Assignment = assignment,
            Progress = progress,
            PreviousVideo = currentIndex > 0 ? allVideos[currentIndex - 1] : null,
            NextVideo = currentIndex < allVideos.Count - 1 ? allVideos[currentIndex + 1] : null,
            Checkpoints = checkpoints.Select(c => new CheckpointWithStatus
            {
                Checkpoint = c,
                IsPassed = passedCheckpointIds.Contains(c.Id),
                Questions = c.Questions.OrderBy(q => q.OrderIndex).ToList()
            }).ToList()
        };

        return View(vm);
    }

    // ── SAVE PROGRESS (AJAX) ───────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressModel model)
    {
        var userId = _userManager.GetUserId(User);
        var assignment = await _db.CourseAssignments
            .FirstOrDefaultAsync(a => a.Id == model.AssignmentId && (a.EmployeeId == userId || User.IsInRole("Admin") || User.IsInRole("Trainer")));
        if (assignment == null) return NotFound();

        var progress = await _db.VideoProgresses
            .FirstOrDefaultAsync(vp => vp.CourseAssignmentId == model.AssignmentId && vp.VideoId == model.VideoId);

        if (progress == null)
        {
            progress = new VideoProgress
            {
                CourseAssignmentId = model.AssignmentId,
                VideoId = model.VideoId
            };
            _db.VideoProgresses.Add(progress);
        }

        progress.LastWatchedPositionSeconds = model.PositionSeconds;
        progress.LastUpdatedAt = DateTime.UtcNow;

        if (model.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt ??= DateTime.UtcNow;
        }

        // If video doesn't have a duration stored yet, update it from client video duration
        if (model.DurationSeconds > 0)
        {
            var video = await _db.Videos.FindAsync(model.VideoId);
            if (video != null && video.DurationSeconds == 0)
            {
                video.DurationSeconds = model.DurationSeconds;
            }
        }

        // Save progress to database FIRST so count queries can see it
        await _db.SaveChangesAsync();

        // Check if all videos in the course are now completed
        bool courseCompleted = false;
        if (progress.IsCompleted)
        {
            courseCompleted = await CheckCourseCompletionAsync(assignment);
            await _db.SaveChangesAsync();
        }

        return Ok(new { 
            success = true, 
            videoCompleted = progress.IsCompleted, 
            courseCompleted = courseCompleted || assignment.Status == AssignmentStatus.Completed 
        });
    }

    // ── SUBMIT QUIZ (AJAX) ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmitModel model)
    {
        var userId = _userManager.GetUserId(User);
        var assignment = await _db.CourseAssignments
            .FirstOrDefaultAsync(a => a.Id == model.AssignmentId && (a.EmployeeId == userId || User.IsInRole("Admin") || User.IsInRole("Trainer")));
        if (assignment == null) return NotFound();

        var checkpoint = await _db.QuizCheckpoints
            .Include(c => c.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(c => c.Id == model.CheckpointId);
        if (checkpoint == null) return NotFound();

        // Grade the quiz
        int totalPoints = 0;
        int earnedPoints = 0;
        var answerResults = new List<QuizAnswerResult>();

        var attempt = new QuizAttempt
        {
            CourseAssignmentId = model.AssignmentId,
            CheckpointId = model.CheckpointId,
            AttemptedAt = DateTime.UtcNow
        };

        foreach (var question in checkpoint.Questions)
        {
            totalPoints += question.Points;
            var submittedAnswer = model.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            var correctOption = question.AnswerOptions.FirstOrDefault(a => a.IsCorrect);

            bool isCorrect = submittedAnswer != null &&
                             correctOption != null &&
                             submittedAnswer.SelectedOptionId == correctOption.Id;

            if (isCorrect) earnedPoints += question.Points;

            attempt.Answers.Add(new QuizAnswer
            {
                QuestionId = question.Id,
                SelectedAnswerOptionId = submittedAnswer?.SelectedOptionId,
                IsCorrect = isCorrect
            });

            answerResults.Add(new QuizAnswerResult
            {
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                CorrectOptionId = correctOption?.Id ?? 0
            });
        }

        int scorePercent = totalPoints > 0 ? (int)Math.Round((double)earnedPoints / totalPoints * 100) : 0;
        bool passed = scorePercent >= checkpoint.PassingScorePercent;

        attempt.Score = earnedPoints;
        attempt.MaxScore = totalPoints;
        attempt.IsPassed = passed;

        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        return Json(new QuizResultModel
        {
            Passed = passed,
            Score = earnedPoints,
            MaxScore = totalPoints,
            ScorePercent = scorePercent,
            RequiredPercent = checkpoint.PassingScorePercent,
            AnswerResults = answerResults
        });
    }

    // ── RETAKE QUIZ (AJAX) ──────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> RetakeQuiz([FromBody] RetakeQuizModel model)
    {
        var userId = _userManager.GetUserId(User);
        var assignment = await _db.CourseAssignments
            .FirstOrDefaultAsync(a => a.Id == model.AssignmentId && (a.EmployeeId == userId || User.IsInRole("Admin") || User.IsInRole("Trainer")));
        if (assignment == null) return NotFound();

        var checkpoint = await _db.QuizCheckpoints
            .FirstOrDefaultAsync(c => c.Id == model.CheckpointId);
        if (checkpoint == null) return NotFound();

        // Delete all previous quiz attempts (and their child answers via cascade) for this checkpoint + assignment
        var previousAttempts = await _db.QuizAttempts
            .Include(qa => qa.Answers)
            .Where(qa => qa.CourseAssignmentId == model.AssignmentId && qa.CheckpointId == model.CheckpointId)
            .ToListAsync();

        if (previousAttempts.Any())
        {
            _db.QuizAttempts.RemoveRange(previousAttempts);
        }

        // Reset VideoProgress.IsCompleted for the video this checkpoint belongs to
        // so the module won't show as "completed" while the quiz is pending again
        var videoProgress = await _db.VideoProgresses
            .FirstOrDefaultAsync(vp => vp.CourseAssignmentId == model.AssignmentId && vp.VideoId == checkpoint.VideoId);

        if (videoProgress != null)
        {
            videoProgress.IsCompleted = false;
            videoProgress.CompletedAt = null;
        }

        // Revert assignment status if it was Completed
        if (assignment.Status == AssignmentStatus.Completed)
        {
            assignment.Status = AssignmentStatus.InProgress;
            assignment.CompletedAt = null;
        }

        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ── HELPER ──────────────────────────────────────────────────────────
    private async Task<bool> CheckCourseCompletionAsync(CourseAssignment assignment)
    {
        var totalVideos = await _db.Videos.CountAsync(v => v.CourseId == assignment.CourseId);
        var completedVideos = await _db.VideoProgresses
            .CountAsync(vp => vp.CourseAssignmentId == assignment.Id && vp.IsCompleted);

        if (totalVideos > 0 && completedVideos >= totalVideos)
        {
            assignment.Status = AssignmentStatus.Completed;
            assignment.CompletedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}

public class SaveProgressModel
{
    public int AssignmentId { get; set; }
    public int VideoId { get; set; }
    public int PositionSeconds { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
}

public class RetakeQuizModel
{
    public int AssignmentId { get; set; }
    public int CheckpointId { get; set; }
}
