using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using VoyageFreightLMS.Data;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;
using VoyageFreightLMS.Services;

namespace VoyageFreightLMS.Controllers;

/// <summary>
/// Generates detailed employee progress reports.
/// Accessible by Admin, HR, and Manager roles.
/// </summary>
[Authorize(Roles = "Admin,HR,Manager")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── LIST (select employee) ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var employees = await _userManager.GetUsersInRoleAsync("Employee");

        var assignments = await _db.CourseAssignments
            .GroupBy(a => a.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                Total      = g.Count(),
                Completed  = g.Count(a => a.Status == AssignmentStatus.Completed)
            })
            .ToListAsync();

        var rows = employees.Select(e =>
        {
            var stats = assignments.FirstOrDefault(a => a.EmployeeId == e.Id);
            return new EmployeeReportListRow
            {
                Employee          = e,
                TotalAssigned     = stats?.Total     ?? 0,
                TotalCompleted    = stats?.Completed ?? 0,
                CompletionPercent = stats is { Total: > 0 }
                    ? Math.Round((double)stats.Completed / stats.Total * 100, 1)
                    : 0
            };
        }).OrderByDescending(r => r.CompletionPercent).ToList();

        return View(rows);
    }

    // ── DETAIL REPORT (web view) ───────────────────────────────────────
    [Route("Reports/Employee/{employeeId}")]
    public async Task<IActionResult> Employee(string employeeId)
    {
        var vm = await BuildViewModel(employeeId);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // ── DOWNLOAD PDF ───────────────────────────────────────────────────
    [Route("Reports/Employee/{employeeId}/Download")]
    public async Task<IActionResult> DownloadPdf(string employeeId)
    {
        var vm = await BuildViewModel(employeeId);
        if (vm == null) return NotFound();

        var pdfBytes = new EmployeeReportPdfDocument(vm).GeneratePdf();
        var fileName = $"Progress-Report-{vm.Employee.FirstName}-{vm.Employee.LastName}-{DateTime.UtcNow:yyyyMMdd}.pdf";

        // FileResult with a filename triggers browser "Save As" / auto-download
        return File(pdfBytes, "application/pdf", fileName);
    }

    // ── SHARED BUILDER ────────────────────────────────────────────────
    private async Task<EmployeeProgressReportViewModel?> BuildViewModel(string employeeId)
    {
        var employee = await _userManager.FindByIdAsync(employeeId);
        if (employee == null) return null;

        var assignments = await _db.CourseAssignments
            .Include(a => a.Course)
                .ThenInclude(c => c.Videos)
            .Include(a => a.VideoProgresses)
            .Include(a => a.QuizAttempts)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        // Auto-complete check
        bool dirty = false;
        foreach (var a in assignments)
        {
            if (a.Status != AssignmentStatus.Completed && a.Course.Videos.Count > 0
                && a.VideoProgresses.Count(vp => vp.IsCompleted) >= a.Course.Videos.Count)
            {
                a.Status      = AssignmentStatus.Completed;
                a.CompletedAt ??= DateTime.UtcNow;
                dirty = true;
            }
        }
        if (dirty) await _db.SaveChangesAsync();

        var rows = assignments.Select(a =>
        {
            int total   = a.Course.Videos.Count;
            int watched = a.VideoProgresses.Count(vp => vp.IsCompleted);
            var passed  = a.QuizAttempts.Where(qa => qa.IsPassed && qa.MaxScore > 0).ToList();

            return new CourseProgressRow
            {
                Assignment        = a,
                CourseTitle       = a.Course.Title,
                TotalVideos       = total,
                VideosWatched     = watched,
                CompletionPercent = total > 0 ? Math.Round((double)watched / total * 100, 1) : 0,
                AvgQuizScore      = passed.Count > 0
                    ? Math.Round(passed.Average(qa => (double)qa.Score / qa.MaxScore * 100), 1) : 0,
                Status      = a.Status,
                DueDate     = a.DueDate,
                CompletedAt = a.CompletedAt
            };
        }).ToList();

        int totalAssigned  = assignments.Count;
        int totalCompleted = assignments.Count(a => a.Status == AssignmentStatus.Completed);

        return new EmployeeProgressReportViewModel
        {
            Employee                 = employee,
            TotalAssigned            = totalAssigned,
            TotalCompleted           = totalCompleted,
            OverallCompletionPercent = totalAssigned > 0
                ? Math.Round((double)totalCompleted / totalAssigned * 100, 1) : 0,
            Courses           = rows,
            ReportGeneratedAt = DateTime.UtcNow
        };
    }
}

/// <summary>Simple DTO used by the listing page.</summary>
public class EmployeeReportListRow
{
    public ApplicationUser Employee { get; set; } = null!;
    public int    TotalAssigned     { get; set; }
    public int    TotalCompleted    { get; set; }
    public double CompletionPercent { get; set; }
}
