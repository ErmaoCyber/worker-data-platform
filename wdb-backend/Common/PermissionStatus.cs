namespace wdb_backend.Common;

/// <summary>
/// Permission lifecycle states. Stored as int in the permission.status column.
///
/// Pending  - awaiting worker decision
/// Approved - worker granted access
/// Rejected - worker denied (was never approved)
/// Revoked  - worker withdrew a previously approved access
/// </summary>
public static class PermissionStatus
{
    public const int Pending  = 0;
    public const int Approved = 1;
    public const int Rejected = 2;
    public const int Revoked  = 3;
}
