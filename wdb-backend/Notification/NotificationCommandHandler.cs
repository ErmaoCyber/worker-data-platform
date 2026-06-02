using MediatR;
using wdb_backend.DTOs;

namespace wdb_backend.Notification;

/// <summary>
/// Translates a NotificationCommand into a published NotificationEvent.
/// </summary>
public class NotificationCommandHandler : IRequestHandler<NotificationCommand>
{
    private readonly IMediator _mediator;

    public NotificationCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(NotificationCommand cmd, CancellationToken ct)
    {
        await _mediator.Publish(
            new NotificationEvent(
                cmd.EmployerId,
                cmd.WorkerId,
                cmd.RequestId,
                cmd.FieldLabel,
                cmd.Type,
                DateTime.UtcNow),
            ct);
    }
}
