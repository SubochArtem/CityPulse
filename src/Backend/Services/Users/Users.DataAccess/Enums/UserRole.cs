namespace Users.DataAccess.Enums;

public enum UserRole
{
    None = 0,
    User = 1 << 0,
    Admin = 1 << 1,
    CityManager = 1 << 2
}
