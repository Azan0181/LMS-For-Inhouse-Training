using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;

namespace VoyageFreightLMS.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        // ── Employee dashboard data ────────────────────────────────────
        if (User.IsInRole("Employee"))
        {
            var assignments = await _db.CourseAssignments
                .Include(a => a.Course)
                    .ThenInclude(c => c.Videos)
                .Include(a => a.VideoProgresses)
                .Where(a => a.EmployeeId == userId)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

            ViewBag.Assignments = assignments;
            ViewBag.CompletedCount = assignments.Count(a => a.Status == AssignmentStatus.Completed);
            ViewBag.InProgressCount = assignments.Count(a => a.Status == AssignmentStatus.InProgress);
            ViewBag.PendingCount = assignments.Count(a => a.Status == AssignmentStatus.NotStarted);
        }

        // ── Trainer dashboard data ─────────────────────────────────────
        if (User.IsInRole("Trainer"))
        {
            var myCourses = await _db.Courses
                .Include(c => c.Videos)
                .Include(c => c.Assignments)
                .Where(c => c.CreatedById == userId)
                .ToListAsync();

            ViewBag.MyCourses = myCourses;
        }

        // ── Admin / HR / Manager → redirect to dashboard ───────────────
        if (User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager"))
        {
            ViewBag.TotalCourses = await _db.Courses.CountAsync(c => c.IsPublished);
            ViewBag.TotalAssignments = await _db.CourseAssignments.CountAsync();
            ViewBag.CompletedAssignments = await _db.CourseAssignments.CountAsync(a => a.Status == AssignmentStatus.Completed);
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
