using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VoyageFreightLMS.Models;

namespace VoyageFreightLMS.Data;

/// <summary>
/// The main database context for the Voyage Freight LMS.
/// Inherits from IdentityDbContext so ASP.NET Identity tables (users, roles, claims …)
/// are created automatically alongside the LMS-specific tables.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ── LMS Tables ─────────────────────────────────────────────────────
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<QuizCheckpoint> QuizCheckpoints => Set<QuizCheckpoint>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<CourseAssignment> CourseAssignments => Set<CourseAssignment>();
    public DbSet<VideoProgress> VideoProgresses => Set<VideoProgress>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);   // Identity configuration

        // ── Course ─────────────────────────────────────────────────────
        builder.Entity<Course>(e =>
        {
            e.HasOne(c => c.CreatedBy)
             .WithMany(u => u.CreatedCourses)
             .HasForeignKey(c => c.CreatedById)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(c => c.IsPublished);
        });

        // ── Video ──────────────────────────────────────────────────────
        builder.Entity<Video>(e =>
        {
            e.HasOne(v => v.Course)
             .WithMany(c => c.Videos)
             .HasForeignKey(v => v.CourseId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(v => new { v.CourseId, v.OrderIndex });
        });

        // ── QuizCheckpoint ─────────────────────────────────────────────
        builder.Entity<QuizCheckpoint>(e =>
        {
            e.HasOne(qc => qc.Video)
             .WithMany(v => v.Checkpoints)
             .HasForeignKey(qc => qc.VideoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(qc => new { qc.VideoId, qc.TimestampSeconds });
        });

        // ── Question ───────────────────────────────────────────────────
        builder.Entity<Question>(e =>
        {
            e.HasOne(q => q.Checkpoint)
             .WithMany(qc => qc.Questions)
             .HasForeignKey(q => q.CheckpointId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AnswerOption ───────────────────────────────────────────────
        builder.Entity<AnswerOption>(e =>
        {
            e.HasOne(a => a.Question)
             .WithMany(q => q.AnswerOptions)
             .HasForeignKey(a => a.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CourseAssignment ───────────────────────────────────────────
        builder.Entity<CourseAssignment>(e =>
        {
            e.HasOne(ca => ca.Course)
             .WithMany(c => c.Assignments)
             .HasForeignKey(ca => ca.CourseId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ca => ca.Employee)
             .WithMany(u => u.Assignments)
             .HasForeignKey(ca => ca.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ca => ca.AssignedBy)
             .WithMany(u => u.AssignedByMe)
             .HasForeignKey(ca => ca.AssignedById)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(ca => new { ca.CourseId, ca.EmployeeId }).IsUnique();
        });

        // ── VideoProgress ──────────────────────────────────────────────
        builder.Entity<VideoProgress>(e =>
        {
            e.HasOne(vp => vp.CourseAssignment)
             .WithMany(ca => ca.VideoProgresses)
             .HasForeignKey(vp => vp.CourseAssignmentId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vp => vp.Video)
             .WithMany(v => v.VideoProgresses)
             .HasForeignKey(vp => vp.VideoId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(vp => new { vp.CourseAssignmentId, vp.VideoId }).IsUnique();
        });

        // ── QuizAttempt ────────────────────────────────────────────────
        builder.Entity<QuizAttempt>(e =>
        {
            e.HasOne(qa => qa.CourseAssignment)
             .WithMany(ca => ca.QuizAttempts)
             .HasForeignKey(qa => qa.CourseAssignmentId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(qa => qa.Checkpoint)
             .WithMany(qc => qc.Attempts)
             .HasForeignKey(qa => qa.CheckpointId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── QuizAnswer ─────────────────────────────────────────────────
        builder.Entity<QuizAnswer>(e =>
        {
            e.HasOne(a => a.QuizAttempt)
             .WithMany(qa => qa.Answers)
             .HasForeignKey(a => a.QuizAttemptId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Question)
             .WithMany()
             .HasForeignKey(a => a.QuestionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.SelectedAnswerOption)
             .WithMany()
             .HasForeignKey(a => a.SelectedAnswerOptionId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
