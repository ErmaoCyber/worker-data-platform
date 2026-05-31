using MediatR;
using wdb_backend.Common;

namespace wdb_backend.DTOs;

public record NotificationInfo(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId);

public record NotificationCommand(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId, LegacyNotificationType Type) : IRequest;

public record NotificationEvent(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId, LegacyNotificationType Type, DateTime CreateAt) : INotification;

// dto to hold the readable info
public record NotificationFormat(string? EmployerName, string? WorkerName, string NotificationType, string? WorkInfoDesc, DateTime NotificationTime);
public record NotificationFormatComponent(Guid Id, string? EmployerName, string? WorkerName, string NotificationType, string? WorkerInfoDesc, DateTime NotificationTime);
