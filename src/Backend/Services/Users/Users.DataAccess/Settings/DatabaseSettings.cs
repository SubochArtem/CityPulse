namespace Users.DataAccess.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    public bool ApplyMigrationsOnStartup { get; init; }
}
