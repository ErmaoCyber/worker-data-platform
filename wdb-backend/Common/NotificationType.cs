namespace wdb_backend.Common;

/// <summary>
/// Legacy notification type enum, retained for the old NotificationDtos and
/// NotificationCommandHandler pending the notification subsystem refactor.
/// Production code should use the string constants in
/// wdb_backend.Models.NotificationType instead.
/// </summary>
public enum LegacyNotificationType
{
    Access,
    Request
}
