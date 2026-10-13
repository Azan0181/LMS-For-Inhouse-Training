using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

[Authorize(Roles = "Admin,HR,Manager")]
public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── LIST ALL ASSIGNMENTS ───────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var assignments = await _db.CourseAssignments
            .Include(a => a.Course)
                .ThenInclude(c => c.Videos)
            .Include(a => a.Employee)
            .Include(a => a.AssignedBy)
            .Include(a => a.VideoProgresses)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        bool hasChanges = false;
        foreach (var a in assignments)
        {
            if (a.Status != AssignmentStatus.Completed && a.Course != null)
            {
                int totalVids = a.Course.Videos.Count;
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

        return View(assignments);
    }

    // ── ASSIGN COURSE ──────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create(int? courseId)
    {
        var employees = await _userManager.GetUsersInRoleAsync("Employee");
        var courses = await _db.Courses.Where(c => c.IsPublished).ToListAsync();

        ViewBag.Courses = courses;

        return View(new AssignCourseViewModel
        {
            CourseId = courseId ?? 0,
            CourseTitle = courseId.HasValue
                ? (await _db.Courses.FindAsync(courseId.Value))?.Title ?? ""
                : "",
            AvailableEmployees = employees.ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignCourseViewModel model)
    {
        if (model.CourseId == 0 || !model.SelectedEmployeeIds.Any())
        {
            ModelState.AddModelError("", "Please select a course and at least one employee.");
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var courses = await _db.Courses.Where(c => c.IsPublished).ToListAsync();
            ViewBag.Courses = courses;
            model.AvailableEmployees = employees.ToList();
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User)!;
        int created = 0;

        foreach (var employeeId in model.SelectedEmployeeIds)
        {
            // Skip if already assigned
            bool exists = await _db.CourseAssignments
                .AnyAsync(a => a.CourseId == model.CourseId && a.EmployeeId == employeeId);
            if (exists) continue;

            _db.CourseAssignments.Add(new CourseAssignment
            {
                CourseId = model.CourseId,
                EmployeeId = employeeId,
                AssignedById = currentUserId,
                AssignedAt = DateTime.UtcNow,
                DueDate = model.DueDate
            });
            created++;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Course assigned to {created} employee(s).";
        return RedirectToAction("Index");
    }

    // ── DELETE ASSIGNMENT ──────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await _db.CourseAssignments.FindAsync(id);
        if (assignment == null) return NotFound();

        _db.CourseAssignments.Remove(assignment);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Assignment removed.";
        return RedirectToAction("Index");
    }
}
