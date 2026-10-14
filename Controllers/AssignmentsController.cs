using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;
using VoyageFreightLMS.Services;

namespace VoyageFreightLMS.Controllers;

[Authorize(Roles = "Admin,HR,Manager")]
public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public AssignmentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailService email,
        IConfiguration config)
    {
        _db          = db;
        _userManager = userManager;
        _email       = email;
        _config      = config;
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
        var assigner      = await _userManager.FindByIdAsync(currentUserId);
        var course        = await _db.Courses.FindAsync(model.CourseId);
        int created = 0;

        // Track newly created assignments for email notifications
        var notifyList = new List<ApplicationUser>();

        foreach (var employeeId in model.SelectedEmployeeIds)
        {
            // Skip if already assigned
            bool exists = await _db.CourseAssignments
                .AnyAsync(a => a.CourseId == model.CourseId && a.EmployeeId == employeeId);
            if (exists) continue;

            _db.CourseAssignments.Add(new CourseAssignment
            {
                CourseId     = model.CourseId,
                EmployeeId   = employeeId,
                AssignedById = currentUserId,
                AssignedAt   = DateTime.UtcNow,
                DueDate      = model.DueDate
            });
            created++;

            var emp = await _userManager.FindByIdAsync(employeeId);
            if (emp != null) notifyList.Add(emp);
        }

        await _db.SaveChangesAsync();

        // ── Send assignment notification emails ────────────────────────
        if (course != null && assigner != null && notifyList.Count > 0)
        {
            var baseUrl = _config["AppBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl) || baseUrl.Contains("7001"))
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }

            var assignerName = assigner.FullName;
            var courseTitle  = course.Title;
            var dueDate      = model.DueDate;
            var courseUrl    = $"{baseUrl}/Courses/Details/{course.Id}";

            // Capture plain strings per employee, then fire all in parallel
            var emailTasks = notifyList
                .Where(emp => !string.IsNullOrWhiteSpace(emp.Email))
                .Select(emp => _email.SendCourseAssignedAsync(
                    emp.Email!,
                    emp.FullName,
                    courseTitle,
                    assignerName,
                    dueDate,
                    courseUrl))
                .ToList();

            // Fire all in parallel; don't block the redirect on slow SMTP
            _ = Task.WhenAll(emailTasks);
        }

        TempData["Success"] = $"Course assigned to {created} employee(s).";
        return RedirectToAction("Index");
    }

    // ── KICK OUT / DELETE ASSIGNMENT ────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var assignment = await _db.CourseAssignments
            .Include(a => a.Employee)
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return NotFound();

        var empName     = assignment.Employee?.FullName ?? "Employee";
        var courseTitle = assignment.Course?.Title ?? "Course";

        _db.CourseAssignments.Remove(assignment);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{empName} has been removed from '{courseTitle}'.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }
}
