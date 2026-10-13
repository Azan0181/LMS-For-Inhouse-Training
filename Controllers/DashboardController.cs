using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Controllers;

[Authorize(Roles = "Admin,HR,Manager")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── MAIN DASHBOARD ─────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var employees = await _userManager.GetUsersInRoleAsync("Employee");
        var assignments = await _db.CourseAssignments
            .Include(a => a.Course)
                .ThenInclude(c => c.Videos)
            .Include(a => a.Employee)
            .Include(a => a.VideoProgresses)
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

        var courses = await _db.Courses.Include(c => c.Assignments).ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalEmployees = employees.Count,
            TotalCourses = courses.Count(c => c.IsPublished),
            TotalAssignments = assignments.Count,
            CompletedAssignments = assignments.Count(a => a.Status == AssignmentStatus.Completed),
            OverallCompletionRate = assignments.Count > 0
                ? Math.Round((double)assignments.Count(a => a.Status == AssignmentStatus.Completed) / assignments.Count * 100, 1)
                : 0,
            Employees = employees.Select(e =>
            {
                var empAssignments = assignments.Where(a => a.EmployeeId == e.Id).ToList();
                return new EmployeeSummary
                {
                    Employee = e,
                    AssignedCourses = empAssignments.Count,
                    CompletedCourses = empAssignments.Count(a => a.Status == AssignmentStatus.Completed),
                    CompletionRate = empAssignments.Count > 0
                        ? Math.Round((double)empAssignments.Count(a => a.Status == AssignmentStatus.Completed) / empAssignments.Count * 100, 1)
                        : 0
                };
            }).ToList(),
            Courses = courses.Where(c => c.IsPublished).Select(c => new CourseSummary
            {
                Course = c,
                TotalAssignments = c.Assignments.Count,
                CompletedCount = c.Assignments.Count(a => a.Status == AssignmentStatus.Completed),
                CompletionRate = c.Assignments.Count > 0
                    ? Math.Round((double)c.Assignments.Count(a => a.Status == AssignmentStatus.Completed) / c.Assignments.Count * 100, 1)
                    : 0
            }).ToList(),
            RecentAssignments = assignments.OrderByDescending(a => a.AssignedAt).Take(10).ToList()
        };

        return View(vm);
    }

    // ── EMPLOYEE DETAIL ────────────────────────────────────────────────
    public async Task<IActionResult> EmployeeDetail(string id)
    {
        var employee = await _userManager.FindByIdAsync(id);
        if (employee == null) return NotFound();

        var assignments = await _db.CourseAssignments
            .Include(a => a.Course)
                .ThenInclude(c => c.Videos)
            .Include(a => a.VideoProgresses)
            .Include(a => a.QuizAttempts)
            .Where(a => a.EmployeeId == id)
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

        var vm = new EmployeeDetailViewModel
        {
            Employee = employee,
            TotalAssigned = assignments.Count,
            TotalCompleted = assignments.Count(a => a.Status == AssignmentStatus.Completed),
            Assignments = assignments.Select(a => new AssignmentDetail
            {
                Assignment = a,
                TotalVideos = a.Course.Videos.Count,
                VideosCompleted = a.VideoProgresses.Count(vp => vp.IsCompleted),
                ProgressPercent = a.Course.Videos.Count > 0
                    ? Math.Round((double)a.VideoProgresses.Count(vp => vp.IsCompleted) / a.Course.Videos.Count * 100, 1)
                    : 0,
                AverageQuizScore = a.QuizAttempts.Count > 0
                    ? Math.Round(a.QuizAttempts.Where(qa => qa.MaxScore > 0).Average(qa => (double)qa.Score / qa.MaxScore * 100), 1)
                    : 0
            }).ToList()
        };

        return View(vm);
    }
}
