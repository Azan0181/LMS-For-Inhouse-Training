using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

[Authorize(Roles = "Trainer,Admin")]
public class VideosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public VideosController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ── UPLOAD ──────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        var nextOrder = await _db.Videos.Where(v => v.CourseId == courseId).CountAsync();

        return View(new VideoUploadViewModel
        {
            CourseId = courseId,
            OrderIndex = nextOrder
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VideoUploadViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var videoUrl = await SaveVideoAsync(model.VideoFile);

        var video = new Video
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Description = model.Description,
            VideoUrl = videoUrl,
            OrderIndex = model.OrderIndex,
            DurationSeconds = model.DurationSeconds,
            CreatedAt = DateTime.UtcNow
        };

        _db.Videos.Add(video);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Video uploaded!";
        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    // ── EDIT ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var video = await _db.Videos.FindAsync(id);
        if (video == null) return NotFound();

        return View(new VideoEditViewModel
        {
            Id = video.Id,
            CourseId = video.CourseId,
            Title = video.Title,
            Description = video.Description,
            OrderIndex = video.OrderIndex,
            DurationSeconds = video.DurationSeconds,
            ExistingVideoUrl = video.VideoUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VideoEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var video = await _db.Videos.FindAsync(model.Id);
        if (video == null) return NotFound();

        video.Title = model.Title;
        video.Description = model.Description;
        video.OrderIndex = model.OrderIndex;

        if (model.DurationSeconds > 0)
            video.DurationSeconds = model.DurationSeconds;

        if (model.VideoFile != null)
            video.VideoUrl = await SaveVideoAsync(model.VideoFile);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Video updated.";
        return RedirectToAction("Details", "Courses", new { id = video.CourseId });
    }

    // ── DELETE ──────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var video = await _db.Videos.FindAsync(id);
        if (video == null) return NotFound();

        var courseId = video.CourseId;
        _db.Videos.Remove(video);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Video deleted.";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    // ── HELPER ──────────────────────────────────────────────────────────
    private async Task<string> SaveVideoAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/videos/{fileName}";
    }
}
