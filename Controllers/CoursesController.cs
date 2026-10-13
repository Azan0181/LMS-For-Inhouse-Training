using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public CoursesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    // ── INDEX (all users can browse published courses) ──────────────────
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var isTrainer = User.IsInRole("Trainer") || User.IsInRole("Admin");

        IQueryable<Course> query = _db.Courses.Include(c => c.CreatedBy).Include(c => c.Videos);

        if (isTrainer)
        {
            // Trainers see their own courses (all states)
            query = query.Where(c => c.CreatedById == userId);
        }
        else
        {
            // Employees / HR / Managers see only published courses
            query = query.Where(c => c.IsPublished);
        }

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        // Get assignments for the current employee
        var assignments = await _db.CourseAssignments
            .Where(a => a.EmployeeId == userId)
            .ToDictionaryAsync(a => a.CourseId);

        ViewBag.Assignments = assignments;
        ViewBag.IsTrainer = isTrainer;
        return View(courses);
    }

    // ── CREATE ──────────────────────────────────────────────────────────
    [Authorize(Roles = "Trainer,Admin")]
    public IActionResult Create() => View(new CourseCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            DifficultyLevel = model.DifficultyLevel,
            EstimatedDurationMinutes = model.EstimatedDurationMinutes,
            CreatedById = _userManager.GetUserId(User)!,
            CreatedAt = DateTime.UtcNow
        };

        if (model.Thumbnail != null)
            course.ThumbnailUrl = await SaveFileAsync(model.Thumbnail, "thumbnails");

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Course created! Now add videos.";
        return RedirectToAction("Details", new { id = course.Id });
    }

    // ── DETAILS ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Videos.OrderBy(v => v.OrderIndex))
                .ThenInclude(v => v.Checkpoints)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.Employee)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.VideoProgresses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        // Reconcile completion statuses for all assignments in this course
        bool hasChanges = false;
        foreach (var a in course.Assignments)
        {
            if (a.Status != AssignmentStatus.Completed)
            {
                int totalVids = course.Videos.Count;
                int compVids = a.VideoProgresses.Count(vp => vp.IsCompleted);
                if (totalVids > 0 && compVids >= totalVids)
                {
                    a.Status = AssignmentStatus.Completed;
                    a.CompletedAt ??= DateTime.UtcNow;
                    hasChanges = true;
                }
            }
        }
        if (hasChanges) await _db.SaveChangesAsync();

        var userId = _userManager.GetUserId(User);
        var assignment = await _db.CourseAssignments
            .FirstOrDefaultAsync(a => a.CourseId == id && a.EmployeeId == userId);

        var videoProgressMap = new Dictionary<int, VideoProgress?>();
        int completedVideos = 0;

        // If the logged in user has an assignment, show their progress
        if (assignment != null)
        {
            var progresses = await _db.VideoProgresses
                .Where(vp => vp.CourseAssignmentId == assignment.Id)
                .ToDictionaryAsync(vp => vp.VideoId);

            foreach (var video in course.Videos)
            {
                progresses.TryGetValue(video.Id, out var vp);
                videoProgressMap[video.Id] = vp;
                if (vp?.IsCompleted == true) completedVideos++;
            }
        }
        else if (User.IsInRole("Admin") || User.IsInRole("Trainer"))
        {
            // For admins/trainers previewing, link to the first enrolled assignment if available so "Watch" works
            assignment = course.Assignments.FirstOrDefault();
            if (assignment != null)
            {
                var progresses = await _db.VideoProgresses
                    .Where(vp => vp.CourseAssignmentId == assignment.Id)
                    .ToDictionaryAsync(vp => vp.VideoId);

                foreach (var video in course.Videos)
                {
                    progresses.TryGetValue(video.Id, out var vp);
                    videoProgressMap[video.Id] = vp;
                    if (vp?.IsCompleted == true) completedVideos++;
                }
            }
        }

        var vm = new CourseDetailsViewModel
        {
            Course = course,
            Videos = course.Videos.OrderBy(v => v.OrderIndex).ToList(),
            Assignment = assignment,
            VideoProgressMap = videoProgressMap,
            CompletedVideos = completedVideos,
            TotalVideos = course.Videos.Count,
            OverallProgressPercent = course.Videos.Count > 0
                ? Math.Round((double)completedVideos / course.Videos.Count * 100, 1)
                : 0,
            EnrolledAssignments = course.Assignments.OrderByDescending(a => a.AssignedAt).ToList()
        };

        ViewBag.IsTrainer = User.IsInRole("Trainer") || User.IsInRole("Admin");
        return View(vm);
    }

    // ── EDIT ────────────────────────────────────────────────────────────
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        if (course.CreatedById != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            return Forbid();

        return View(new CourseEditViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            DifficultyLevel = course.DifficultyLevel,
            EstimatedDurationMinutes = course.EstimatedDurationMinutes,
            ExistingThumbnailUrl = course.ThumbnailUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> Edit(CourseEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var course = await _db.Courses.FindAsync(model.Id);
        if (course == null) return NotFound();

        course.Title = model.Title;
        course.Description = model.Description;
        course.Category = model.Category;
        course.DifficultyLevel = model.DifficultyLevel;
        course.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
        course.UpdatedAt = DateTime.UtcNow;

        if (model.Thumbnail != null)
            course.ThumbnailUrl = await SaveFileAsync(model.Thumbnail, "thumbnails");

        await _db.SaveChangesAsync();
        TempData["Success"] = "Course updated.";
        return RedirectToAction("Details", new { id = course.Id });
    }

    // ── PUBLISH / UNPUBLISH ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        course.IsPublished = !course.IsPublished;
        course.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = course.IsPublished ? "Course published!" : "Course unpublished.";
        return RedirectToAction("Details", new { id });
    }

    // ── DELETE ──────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Course deleted.";
        return RedirectToAction("Index");
    }

    // ── HELPER ──────────────────────────────────────────────────────────
    private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{subFolder}/{fileName}";
    }
}
