using MediatR;
using wdb_backend.DTOs;

namespace wdb_backend.Notification;

public class NotificationCommandHandler : IRequestHandler<NotificationCommand>
{
    // inject the IMediator instance for decoupling
    private readonly IMediator _mediator;

    public NotificationCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(NotificationCommand cmd, CancellationToken ct)
    {
        await _mediator.Publish(new NotificationEvent(cmd.EmployerId, cmd.WorkerId, cmd.WorkerInfoId, cmd.Type, DateTime.UtcNow), ct);
    }
}
