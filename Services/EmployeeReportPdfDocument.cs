using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VoyageFreightLMS.Models;
using VoyageFreightLMS.Models.ViewModels;

namespace VoyageFreightLMS.Services;

/// <summary>
/// Generates a simple, clean, and appealing A4 PDF progress report for an employee.
/// Uses QuestPDF fluent layout engine.
/// </summary>
public class EmployeeReportPdfDocument : IDocument
{
    // ── Professional Brand Palette ──────────────────────────────────────
    private static readonly string BrandDark   = "#1e293b";   // deep slate
    private static readonly string BrandIndigo = "#4f46e5";   // vibrant indigo
    private static readonly string Success     = "#059669";   // emerald green
    private static readonly string Warning     = "#d97706";   // warm amber
    private static readonly string Danger      = "#dc2626";   // rose red
    private static readonly string TextDark    = "#0f172a";   // slate-900 (primary text)
    private static readonly string TextMuted   = "#64748b";   // slate-500 (secondary text)
    private static readonly string BgLight     = "#f8fafc";   // slate-50 (card background)
    private static readonly string BgSubtle    = "#f1f5f9";   // slate-100 (table header / divider)
    private static readonly string BorderColor = "#e2e8f0";   // slate-200 (clean borders)

    private readonly EmployeeProgressReportViewModel _model;

    public EmployeeReportPdfDocument(EmployeeProgressReportViewModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title        = $"Employee Training Progress Report – {_model.Employee.FullName}",
        Author       = "VoyageFreight LMS",
        Subject      = "Training Completion & Performance Record",
        CreationDate = DateTimeOffset.UtcNow
    };

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    // ── PAGE SETUP ──────────────────────────────────────────────────────
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginVertical(36);
            page.MarginHorizontal(40);
            page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily(Fonts.Arial).FontColor(TextDark));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(14).Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ── 1. HEADER ───────────────────────────────────────────────────────
    private void ComposeHeader(IContainer c)
    {
        c.Column(col =>
        {
            // Top Bar: Logo & Report Title
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(brand =>
                {
                    brand.Item().Text("VOYAGEFREIGHT LMS")
                        .FontSize(15).Bold().FontColor(BrandIndigo);
                    brand.Item().PaddingTop(2)
                        .Text("Employee Training & Progress Report")
                        .FontSize(9).FontColor(TextMuted);
                });

                row.ConstantItem(180).AlignRight().Column(meta =>
                {
                    meta.Item().Text($"Report Date: {_model.ReportGeneratedAt:MMM dd, yyyy}")
                        .FontSize(9).SemiBold().FontColor(BrandDark);
                    meta.Item().PaddingTop(2)
                        .Text("Status: Official Record")
                        .FontSize(8).FontColor(TextMuted);
                });
            });

            // Thin Divider
            col.Item().PaddingTop(10).PaddingBottom(12)
                .LineHorizontal(1).LineColor(BorderColor);

            // Employee Card
            col.Item()
                .Border(1).BorderColor(BorderColor)
                .Background(BgLight)
                .Padding(12)
                .Row(row =>
                {
                    // Initials Badge
                    var initials = $"{_model.Employee.FirstName.FirstOrDefault()}{_model.Employee.LastName.FirstOrDefault()}".ToUpper();
                    row.ConstantItem(44).AlignMiddle()
                        .Width(38).Height(38)
                        .Background(BrandIndigo)
                        .AlignCenter().AlignMiddle()
                        .Text(initials)
                        .FontSize(13).Bold().FontColor("#ffffff");

                    row.ConstantItem(12);

                    // Name & Details
                    row.RelativeItem().AlignMiddle().Column(info =>
                    {
                        info.Item().Text(_model.Employee.FullName).FontSize(13).Bold().FontColor(TextDark);
                        info.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span("Department: ").FontSize(8.5f).SemiBold().FontColor(TextMuted);
                            t.Span($"{(!string.IsNullOrWhiteSpace(_model.Employee.Department) ? _model.Employee.Department : "General")}  •  ").FontSize(8.5f).FontColor(TextDark);
                            t.Span("Role: ").FontSize(8.5f).SemiBold().FontColor(TextMuted);
                            t.Span($"{(!string.IsNullOrWhiteSpace(_model.Employee.JobTitle) ? _model.Employee.JobTitle : "Staff")}  •  ").FontSize(8.5f).FontColor(TextDark);
                            t.Span("Email: ").FontSize(8.5f).SemiBold().FontColor(TextMuted);
                            t.Span($"{_model.Employee.Email}").FontSize(8.5f).FontColor(TextDark);
                        });
                    });

                    // Overall Status Badge
                    row.ConstantItem(120).AlignMiddle().AlignRight().Column(status =>
                    {
                        var isComplete = _model.TotalAssigned > 0 && _model.TotalCompleted >= _model.TotalAssigned;
                        var badgeBg    = isComplete ? "#dcfce7" : _model.OverallCompletionPercent > 0 ? "#fef3c7" : "#f1f5f9";
                        var badgeColor = isComplete ? Success   : _model.OverallCompletionPercent > 0 ? Warning   : TextMuted;
                        var badgeText  = isComplete ? "Fully Completed" : _model.OverallCompletionPercent > 0 ? "In Progress" : "Not Started";

                        status.Item().Background(badgeBg).PaddingVertical(4).PaddingHorizontal(10).AlignCenter()
                            .Text(badgeText).FontSize(8.5f).Bold().FontColor(badgeColor);

                        status.Item().PaddingTop(3).AlignCenter()
                            .Text($"{_model.OverallCompletionPercent:F1}% Completed")
                            .FontSize(8).FontColor(TextMuted);
                    });
                });
        });
    }

    // ── 2. CONTENT ──────────────────────────────────────────────────────
    private void ComposeContent(IContainer c)
    {
        c.Column(col =>
        {
            // ── A. 4 Key Stat Metric Boxes ──────────────────────────────
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(StatCard("COURSES ASSIGNED", _model.TotalAssigned.ToString(), BrandIndigo));
                row.ConstantItem(10);
                row.RelativeItem().Element(StatCard("COMPLETED", _model.TotalCompleted.ToString(), Success));
                row.ConstantItem(10);
                row.RelativeItem().Element(StatCard("PENDING / IN PROGRESS", Math.Max(0, _model.TotalAssigned - _model.TotalCompleted).ToString(), Warning));
                row.ConstantItem(10);
                row.RelativeItem().Element(StatCard("COMPLETION RATE", $"{_model.OverallCompletionPercent:F1}%", ColorForPct(_model.OverallCompletionPercent)));
            });

            // ── B. Visual Overall Progress Bar ──────────────────────────
            col.Item().PaddingTop(10).Column(prog =>
            {
                prog.Item().Row(r =>
                {
                    r.RelativeItem().Text("Overall Training Completion").FontSize(8).SemiBold().FontColor(TextMuted);
                    r.ConstantItem(60).AlignRight().Text($"{_model.OverallCompletionPercent:F1}%").FontSize(8).Bold().FontColor(ColorForPct(_model.OverallCompletionPercent));
                });

                prog.Item().PaddingTop(3).Height(6).Background(BgSubtle).Row(r =>
                {
                    var pct = (float)Math.Clamp(_model.OverallCompletionPercent, 0, 100);
                    if (pct > 0)
                    {
                        r.RelativeItem(pct).Background(ColorForPct(_model.OverallCompletionPercent));
                    }
                    if (pct < 100)
                    {
                        r.RelativeItem(100 - pct);
                    }
                });
            });

            // ── C. Course-by-Course Table ───────────────────────────────
            col.Item().PaddingTop(18).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Text("Course Breakdown & Progress")
                    .FontSize(11).Bold().FontColor(TextDark);
                row.ConstantItem(120).AlignRight()
                    .Text($"{_model.Courses.Count} course(s) listed")
                    .FontSize(8.5f).FontColor(TextMuted);
            });

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3.2f); // Course
                    cols.RelativeColumn(1.4f); // Status
                    cols.RelativeColumn(1.1f); // Modules
                    cols.RelativeColumn(1.2f); // Completion
                    cols.RelativeColumn(1.1f); // Quiz Score
                    cols.RelativeColumn(1.3f); // Due Date
                });

                // Header
                table.Header(hdr =>
                {
                    hdr.Cell().Element(HeaderCell).Text("Course Title").FontSize(8).Bold().FontColor(TextMuted);
                    hdr.Cell().Element(HeaderCell).Text("Status").FontSize(8).Bold().FontColor(TextMuted);
                    hdr.Cell().Element(HeaderCell).Text("Modules").FontSize(8).Bold().FontColor(TextMuted);
                    hdr.Cell().Element(HeaderCell).Text("Progress").FontSize(8).Bold().FontColor(TextMuted);
                    hdr.Cell().Element(HeaderCell).Text("Avg Quiz").FontSize(8).Bold().FontColor(TextMuted);
                    hdr.Cell().Element(HeaderCell).Text("Due Date").FontSize(8).Bold().FontColor(TextMuted);
                });

                // Data Rows
                if (_model.Courses.Count == 0)
                {
                    table.Cell().ColumnSpan(6).Background(BgLight).Padding(16).AlignCenter()
                        .Text("No courses currently assigned to this employee.").FontSize(9).FontColor(TextMuted);
                }
                else
                {
                    bool alt = false;
                    foreach (var course in _model.Courses)
                    {
                        var rowBg = alt ? BgLight : "#ffffff";

                        // Course Title
                        table.Cell().Element(c => BodyCell(c, rowBg)).Column(titleCol =>
                        {
                            titleCol.Item().Text(course.CourseTitle).FontSize(9).SemiBold().FontColor(TextDark);
                            if (course.CompletedAt.HasValue)
                            {
                                titleCol.Item().PaddingTop(1)
                                    .Text($"Completed on {course.CompletedAt.Value:MMM dd, yyyy}")
                                    .FontSize(7.5f).FontColor(Success);
                            }
                        });

                        // Status Badge
                        table.Cell().Element(c => BodyCell(c, rowBg))
                            .Text(course.StatusLabel)
                            .FontSize(8.5f).Bold().FontColor(StatusColor(course.Status));

                        // Modules / Videos Watched
                        table.Cell().Element(c => BodyCell(c, rowBg))
                            .Text($"{course.VideosWatched} / {course.TotalVideos}")
                            .FontSize(8.5f).FontColor(TextMuted);

                        // Completion Percentage
                        table.Cell().Element(c => BodyCell(c, rowBg))
                            .Text($"{course.CompletionPercent:F0}%")
                            .FontSize(8.5f).Bold().FontColor(ColorForPct(course.CompletionPercent));

                        // Quiz Score
                        table.Cell().Element(c => BodyCell(c, rowBg))
                            .Text(course.AvgQuizScore > 0 ? $"{course.AvgQuizScore:F0}%" : "—")
                            .FontSize(8.5f).FontColor(course.AvgQuizScore > 0 ? ColorForPct(course.AvgQuizScore) : TextMuted);

                        // Due Date
                        table.Cell().Element(c => BodyCell(c, rowBg))
                            .Text(course.DueDate.HasValue ? course.DueDate.Value.ToString("MMM dd, yyyy") : "No deadline")
                            .FontSize(8.5f).FontColor(TextMuted);

                        alt = !alt;
                    }
                }
            });

            // ── D. Performance Summary Note ─────────────────────────────
            col.Item().PaddingTop(16)
                .BorderLeft(3).BorderColor(BrandIndigo)
                .Background(BgLight)
                .PaddingVertical(10).PaddingHorizontal(12)
                .Column(summary =>
                {
                    summary.Item().Text("Performance Summary & Standing").FontSize(9.5f).Bold().FontColor(BrandDark);
                    summary.Item().PaddingTop(3).Text(t =>
                    {
                        t.Span($"As of {_model.ReportGeneratedAt:MMMM dd, yyyy}, ").FontSize(8.5f).FontColor(TextDark);
                        t.Span($"{_model.Employee.FullName} ").SemiBold().FontSize(8.5f).FontColor(TextDark);
                        t.Span($"has successfully completed {_model.TotalCompleted} out of {_model.TotalAssigned} assigned course(s), representing an overall completion standing of ").FontSize(8.5f).FontColor(TextDark);
                        t.Span($"{_model.OverallCompletionPercent:F1}%").Bold().FontSize(8.5f).FontColor(ColorForPct(_model.OverallCompletionPercent));
                        t.Span(". ").FontSize(8.5f).FontColor(TextDark);

                        if (_model.OverallCompletionPercent >= 100)
                        {
                            t.Span("All mandatory assigned coursework is fully up to date.").FontSize(8.5f).FontColor(Success);
                        }
                        else if (_model.OverallCompletionPercent >= 50)
                        {
                            t.Span("Coursework is actively progressing. Please review pending modules before the designated due dates.").FontSize(8.5f).FontColor(TextMuted);
                        }
                        else
                        {
                            t.Span("Training is currently pending initiation or in early stages. Please allocate scheduled training time.").FontSize(8.5f).FontColor(Warning);
                        }
                    });
                });
        });
    }

    // ── 3. FOOTER ───────────────────────────────────────────────────────
    private void ComposeFooter(IContainer c)
    {
        c.PaddingTop(10).BorderTop(1).BorderColor(BorderColor).Row(row =>
        {
            row.RelativeItem()
                .Text($"VoyageFreight LMS  •  Confidential Employee Record  •  {_model.ReportGeneratedAt:yyyy-MM-dd HH:mm} UTC")
                .FontSize(7.5f).FontColor(TextMuted);

            row.ConstantItem(80).AlignRight().Text(t =>
            {
                t.Span("Page ").FontSize(7.5f).FontColor(TextMuted);
                t.CurrentPageNumber().FontSize(7.5f).FontColor(TextMuted);
                t.Span(" of ").FontSize(7.5f).FontColor(TextMuted);
                t.TotalPages().FontSize(7.5f).FontColor(TextMuted);
            });
        });
    }

    // ── COMPONENT BUILDERS ──────────────────────────────────────────────
    private static Action<IContainer> StatCard(string label, string value, string color) =>
        container => container
            .Border(1).BorderColor(BorderColor)
            .Background(BgLight)
            .PaddingVertical(10).PaddingHorizontal(10)
            .Column(c =>
            {
                c.Item().Text(value).FontSize(18).Bold().FontColor(color);
                c.Item().PaddingTop(2).Text(label).FontSize(7).Bold().FontColor(TextMuted);
            });

    private static IContainer HeaderCell(IContainer c) =>
        c.Background(BgSubtle)
         .BorderBottom(1).BorderColor(BorderColor)
         .PaddingVertical(6).PaddingHorizontal(8);

    private static IContainer BodyCell(IContainer c, string bg) =>
        c.Background(bg)
         .BorderBottom(1).BorderColor(BorderColor)
         .PaddingVertical(7).PaddingHorizontal(8);

    private static string StatusColor(AssignmentStatus s) => s switch
    {
        AssignmentStatus.Completed  => Success,
        AssignmentStatus.InProgress => Warning,
        _                           => TextMuted
    };

    private static string ColorForPct(double pct) =>
        pct >= 80 ? Success :
        pct >= 40 ? Warning : Danger;
}
