namespace wdb_backend.Common;

/// <summary>
/// Permission lifecycle status values stored as int in the database.
/// 0=Pending, 1=Approved, 2=Rejected, 3=Revoked
/// </summary>
public static class PermissionStatus
{
    public const int Pending = 0;
    public const int Approved = 1;
    public const int Rejected = 2;
    public const int Revoked = 3;
}
