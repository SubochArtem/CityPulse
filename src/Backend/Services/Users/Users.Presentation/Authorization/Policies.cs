namespace Users.Presentation.Authorization;

public static class Policies
{
    public const string ReadUser = "UsersReadPolicy";
    public const string UpdateUser = "UsersUpdatePolicy";
    public const string DeleteUser = "UsersDeletePolicy";
    public const string ActivateUser = "UsersActivatePolicy";
    public const string DeactivateUser = "UsersDeactivatePolicy";
}
