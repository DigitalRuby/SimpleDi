namespace DigitalRuby.SimpleDi.Tests;

/// <summary>
/// Config class that binds to key DigitalRuby.SimpleDi.Tests.Configuration
/// </summary>
[Configuration]
public sealed class Configuration
{
    /// <summary>
    /// Example value
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
