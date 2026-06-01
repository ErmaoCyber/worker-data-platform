namespace wdb_backend.DTOs;

// Readable representation produced for the worker bell / notification list UI.
// Built by NotificationRepoImpl.FormatNotificationPipeline.
public record NotificationFormat(
    string? EmployerName,
    string? WorkerName,
    string NotificationType,
    string? WorkInfoDesc,
    DateTime NotificationTime);

public record NotificationFormatComponent(
    Guid Id,
    string? EmployerName,
    string? WorkerName,
    string NotificationType,
    string? WorkerInfoDesc,
    DateTime NotificationTime);
