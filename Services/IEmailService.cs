namespace VoyageFreightLMS.Services;

/// <summary>
/// Abstraction for sending transactional emails from the LMS.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends an HTML email to a single recipient.</summary>
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);

    /// <summary>Fires the "course assigned" email to the employee.</summary>
    Task SendCourseAssignedAsync(
        string employeeEmail,
        string employeeName,
        string courseTitle,
        string assignedByName,
        DateTime? dueDate,
        string courseUrl);

    /// <summary>Fires the "course completed" email to the employee.</summary>
    Task SendCourseCompletedToEmployeeAsync(
        string employeeEmail,
        string employeeName,
        string courseTitle,
        DateTime completedAt,
        string reportUrl);

    /// <summary>Fires the "course completed" email to the admin/HR who assigned it.</summary>
    Task SendCourseCompletedToAdminAsync(
        string adminEmail,
        string adminName,
        string employeeName,
        string courseTitle,
        DateTime completedAt,
        string reportUrl);
}
