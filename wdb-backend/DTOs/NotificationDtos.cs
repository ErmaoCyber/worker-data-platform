using MediatR;
using wdb_backend.Common;

namespace wdb_backend.DTOs;

public record NotificationInfo(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId);

public record NotificationCommand(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId, NotificationType Type) : IRequest;

public record NotificationEvent(Guid EmployerId, Guid WorkerId, Guid WorkerInfoId, NotificationType Type, DateTime CreateAt) : INotification;
