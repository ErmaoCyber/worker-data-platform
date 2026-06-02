using MediatR;

namespace wdb_backend.DTOs;

/// <summary>
/// Notification event published via MediatR.
/// RequestId + FieldLabel replace the old WorkerInfoId.
/// </summary>
public record NotificationEvent(
    Guid EmployerId,
    Guid WorkerId,
    Guid? RequestId,
    string? FieldLabel,
    string Type,
    DateTime CreateAt
) : INotification;

/// <summary>Inbound DTO from the notification API endpoints.</summary>
public record NotificationInfo(Guid EmployerId, Guid WorkerId, Guid? RequestId);

/// <summary>Command dispatched by NotificationController to NotificationCommandHandler.</summary>
public record NotificationCommand(
    Guid EmployerId,
    Guid WorkerId,
    Guid? RequestId,
    string? FieldLabel,
    string Type
) : IRequest;

/// <summary>Human-readable notification data for display.</summary>
public record NotificationFormat(
    string? EmployerName,
    string? WorkerName,
    string NotificationType,
    string? WorkInfoDesc,
    DateTime NotificationTime
);

public record NotificationFormatComponent(
    Guid Id,
    string? EmployerName,
    string? WorkerName,
    string NotificationType,
    string? WorkerInfoDesc,
    DateTime NotificationTime
);
