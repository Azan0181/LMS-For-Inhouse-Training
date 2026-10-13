using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

[Authorize(Roles = "Trainer,Admin")]
public class CheckpointsController : Controller
{
    private readonly ApplicationDbContext _db;

    public CheckpointsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── MANAGE (list checkpoints & questions for a video) ───────────────
    public async Task<IActionResult> Manage(int videoId)
    {
        var video = await _db.Videos
            .Include(v => v.Course)
            .FirstOrDefaultAsync(v => v.Id == videoId);
        if (video == null) return NotFound();

        var checkpoints = await _db.QuizCheckpoints
            .Include(c => c.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .Where(c => c.VideoId == videoId)
            .OrderBy(c => c.TimestampSeconds)
            .ToListAsync();

        return View(new ManageCheckpointsViewModel
        {
            Video = video,
            Course = video.Course,
            Checkpoints = checkpoints
        });
    }

    // ── CREATE CHECKPOINT ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult CreateCheckpoint(int videoId)
    {
        return View(new CheckpointCreateViewModel { VideoId = videoId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCheckpoint(CheckpointCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var checkpoint = new QuizCheckpoint
        {
            VideoId = model.VideoId,
            Title = model.Title,
            TimestampSeconds = model.TimestampSeconds,
            PassingScorePercent = model.PassingScorePercent
        };

        _db.QuizCheckpoints.Add(checkpoint);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Checkpoint created! Now add questions.";
        return RedirectToAction("Manage", new { videoId = model.VideoId });
    }

    // ── DELETE CHECKPOINT ──────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCheckpoint(int id)
    {
        var checkpoint = await _db.QuizCheckpoints.FindAsync(id);
        if (checkpoint == null) return NotFound();

        var videoId = checkpoint.VideoId;
        _db.QuizCheckpoints.Remove(checkpoint);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Checkpoint deleted.";
        return RedirectToAction("Manage", new { videoId });
    }

    // ── CREATE QUESTION ────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateQuestion(int checkpointId)
    {
        var checkpoint = await _db.QuizCheckpoints.Include(c => c.Video).FirstOrDefaultAsync(c => c.Id == checkpointId);
        if (checkpoint == null) return NotFound();

        ViewBag.Checkpoint = checkpoint;
        return View(new QuestionCreateViewModel { CheckpointId = checkpointId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(QuestionCreateViewModel model)
    {
        // Remove empty options
        model.Options = model.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

        if (model.Options.Count < 2)
        {
            ModelState.AddModelError("Options", "Please provide at least 2 answer options.");
            var cp = await _db.QuizCheckpoints.Include(c => c.Video).FirstOrDefaultAsync(c => c.Id == model.CheckpointId);
            ViewBag.Checkpoint = cp;
            return View(model);
        }

        if (model.CorrectOptionIndex < 0 || model.CorrectOptionIndex >= model.Options.Count)
        {
            ModelState.AddModelError("CorrectOptionIndex", "Please select a valid correct answer.");
            var cp = await _db.QuizCheckpoints.Include(c => c.Video).FirstOrDefaultAsync(c => c.Id == model.CheckpointId);
            ViewBag.Checkpoint = cp;
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            var cp = await _db.QuizCheckpoints.Include(c => c.Video).FirstOrDefaultAsync(c => c.Id == model.CheckpointId);
            ViewBag.Checkpoint = cp;
            return View(model);
        }

        var nextOrder = await _db.Questions.Where(q => q.CheckpointId == model.CheckpointId).CountAsync();

        var question = new Question
        {
            CheckpointId = model.CheckpointId,
            QuestionText = model.QuestionText,
            Points = model.Points,
            OrderIndex = nextOrder
        };

        for (int i = 0; i < model.Options.Count; i++)
        {
            question.AnswerOptions.Add(new AnswerOption
            {
                OptionText = model.Options[i],
                IsCorrect = i == model.CorrectOptionIndex,
                OrderIndex = i
            });
        }

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        var checkpoint2 = await _db.QuizCheckpoints.FindAsync(model.CheckpointId);
        TempData["Success"] = "Question added.";
        return RedirectToAction("Manage", new { videoId = checkpoint2!.VideoId });
    }

    // ── DELETE QUESTION ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _db.Questions
            .Include(q => q.Checkpoint)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null) return NotFound();

        var videoId = question.Checkpoint.VideoId;
        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Question deleted.";
        return RedirectToAction("Manage", new { videoId });
    }
}
