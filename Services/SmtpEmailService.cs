using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace VoyageFreightLMS.Services;

/// <summary>
/// Sends real emails via SMTP using MailKit.
/// Configuration lives in appsettings.json → "EmailSettings".
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    // ── Colours / brand token used across all templates ───────────────
    private const string BrandColor = "#4f46e5";    // indigo-600
    private const string BrandName  = "VoyageFreight LMS";

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── LOW-LEVEL SEND ─────────────────────────────────────────────────
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var section   = _config.GetSection("EmailSettings");
        var host      = section["Host"]     ?? "smtp.gmail.com";
        var port      = int.Parse(section["Port"] ?? "587");
        var user      = section["Username"] ?? "";
        var pass      = section["Password"] ?? "";
        var fromEmail = !string.IsNullOrWhiteSpace(section["From"]) ? section["From"]! : user;
        var fromName  = section["FromName"] ?? BrandName;

        // 1. ALWAYS persist a local HTML preview to wwwroot/sent_emails/
        //    This guarantees developers/admins can inspect sent emails immediately even without SMTP setup.
        try
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sent_emails");
            Directory.CreateDirectory(folder);
            var safeRecipient = string.Concat(toEmail.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeRecipient}.html";
            var fullPath = Path.Combine(folder, fileName);
            await File.WriteAllTextAsync(fullPath, htmlBody);
            _logger.LogInformation("Saved preview email to {Path}", fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not write local email preview: {Msg}", ex.Message);
        }

        // 2. Check if SMTP credentials are provided
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            _logger.LogWarning("EmailSettings (Username/Password) are not configured in appsettings.json. Skipping network SMTP delivery to {To}. (A copy was saved to wwwroot/sent_emails/ for review).", toEmail);
            return;
        }

        // 3. Dispatch real email via SMTP
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, string.IsNullOrWhiteSpace(fromEmail) ? user : fromEmail));
        message.To.Add(new MailboxAddress(string.IsNullOrWhiteSpace(toName) ? toEmail : toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            client.Timeout = 10000; // 10s timeout to prevent hanging on slow connections

            var socketOptions = port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                25  => SecureSocketOptions.None,
                _   => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(host, port, socketOptions);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email successfully dispatched via SMTP to {To}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} via SMTP ({Host}:{Port}). Verify Username, Password, and TLS settings in appsettings.json.", toEmail, host, port);
        }
    }

    // ── COURSE ASSIGNED ────────────────────────────────────────────────
    public Task SendCourseAssignedAsync(
        string employeeEmail, string employeeName,
        string courseTitle, string assignedByName,
        DateTime? dueDate, string courseUrl)
    {
        var dueLine = dueDate.HasValue
            ? $"<p><strong>Due date:</strong> {dueDate.Value:MMMM dd, yyyy}</p>"
            : "";

        var html = BuildEmail(
            previewText: $"You've been assigned a new training: {courseTitle}",
            heading:     "New Training Assigned 🎓",
            body: $@"
                <p>Hi <strong>{employeeName}</strong>,</p>
                <p>You have been assigned a new training course by <strong>{assignedByName}</strong>.</p>
                <table class=""course-card"">
                  <tr><td><strong>Course:</strong></td><td>{courseTitle}</td></tr>
                </table>
                {dueLine}
                <p>Click the button below to start your training:</p>",
            ctaUrl:   courseUrl,
            ctaLabel: "Start Training →"
        );

        return SendAsync(employeeEmail, employeeName,
            $"[{BrandName}] New Course Assigned: {courseTitle}", html);
    }

    // ── COURSE COMPLETED → EMPLOYEE ────────────────────────────────────
    public Task SendCourseCompletedToEmployeeAsync(
        string employeeEmail, string employeeName,
        string courseTitle, DateTime completedAt, string reportUrl)
    {
        var html = BuildEmail(
            previewText: $"Congratulations! You've completed {courseTitle}.",
            heading:     "Course Completed! 🏆",
            body: $@"
                <p>Hi <strong>{employeeName}</strong>,</p>
                <p>🎉 Congratulations! You have successfully completed:</p>
                <table class=""course-card"">
                  <tr><td><strong>Course:</strong></td><td>{courseTitle}</td></tr>
                  <tr><td><strong>Completed:</strong></td><td>{completedAt:MMMM dd, yyyy 'at' HH:mm} UTC</td></tr>
                </table>
                <p>Your progress report is ready to view and download.</p>",
            ctaUrl:   reportUrl,
            ctaLabel: "View My Report →"
        );

        return SendAsync(employeeEmail, employeeName,
            $"[{BrandName}] Course Completed: {courseTitle}", html);
    }

    // ── COURSE COMPLETED → ADMIN ───────────────────────────────────────
    public Task SendCourseCompletedToAdminAsync(
        string adminEmail, string adminName,
        string employeeName, string courseTitle,
        DateTime completedAt, string reportUrl)
    {
        var html = BuildEmail(
            previewText: $"{employeeName} has completed {courseTitle}.",
            heading:     "Training Completed by Employee 📋",
            body: $@"
                <p>Hi <strong>{adminName}</strong>,</p>
                <p>An employee you enrolled has finished their training:</p>
                <table class=""course-card"">
                  <tr><td><strong>Employee:</strong></td><td>{employeeName}</td></tr>
                  <tr><td><strong>Course:</strong></td><td>{courseTitle}</td></tr>
                  <tr><td><strong>Completed:</strong></td><td>{completedAt:MMMM dd, yyyy 'at' HH:mm} UTC</td></tr>
                </table>
                <p>Click below to view the full progress report:</p>",
            ctaUrl:   reportUrl,
            ctaLabel: "View Progress Report →"
        );

        return SendAsync(adminEmail, adminName,
            $"[{BrandName}] {employeeName} completed: {courseTitle}", html);
    }

    // ── TEMPLATE BUILDER ───────────────────────────────────────────────
    private static string BuildEmail(
        string previewText, string heading,
        string body, string ctaUrl, string ctaLabel)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width,initial-scale=1""/>
<title>{heading}</title>
<style>
  body{{margin:0;padding:0;background:#f3f4f6;font-family:'Segoe UI',Arial,sans-serif;color:#1f2937;}}
  .wrapper{{max-width:600px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);}}
  .header{{background:{BrandColor};padding:32px 40px;text-align:center;}}
  .header h1{{color:#fff;margin:0;font-size:22px;letter-spacing:.5px;}}
  .header p{{color:rgba(255,255,255,.75);margin:6px 0 0;font-size:13px;}}
  .body{{padding:36px 40px;}}
  .body h2{{font-size:20px;margin:0 0 16px;color:{BrandColor};}}
  .body p{{line-height:1.7;margin:0 0 14px;font-size:15px;}}
  table.course-card{{width:100%;border-collapse:collapse;background:#f9fafb;border-radius:8px;margin:16px 0;overflow:hidden;}}
  table.course-card td{{padding:10px 16px;font-size:14px;border-bottom:1px solid #e5e7eb;}}
  table.course-card tr:last-child td{{border-bottom:none;}}
  .cta{{text-align:center;margin:28px 0;}}
  .cta a{{background:{BrandColor};color:#fff;text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:600;font-size:15px;display:inline-block;}}
  .footer{{background:#f9fafb;padding:20px 40px;text-align:center;font-size:12px;color:#9ca3af;border-top:1px solid #e5e7eb;}}
</style>
</head>
<body>
<div style=""display:none;max-height:0;overflow:hidden;"">{previewText}</div>
<div class=""wrapper"">
  <div class=""header"">
    <h1>🚢 {BrandName}</h1>
    <p>Training &amp; Learning Management</p>
  </div>
  <div class=""body"">
    <h2>{heading}</h2>
    {body}
    <div class=""cta""><a href=""{ctaUrl}"">{ctaLabel}</a></div>
  </div>
  <div class=""footer"">
    &copy; {DateTime.UtcNow.Year} {BrandName}. This is an automated notification.
  </div>
</div>
</body>
</html>";
    }
}
