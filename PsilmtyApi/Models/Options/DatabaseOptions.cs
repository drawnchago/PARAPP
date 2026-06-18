namespace PsilmtyApi.Models.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string Host { get; set; } = "";
    public uint Port { get; set; } = 3306;
    public string Name { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}
