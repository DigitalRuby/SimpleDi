namespace DigitalRuby.SimpleDi.Tests;

public sealed class SubClass
{
    /// <summary>
    /// Value 1
    /// </summary>
    public string Value1 { get; init; } = "1";

    /// <summary>
    /// Value 2
    /// </summary>
    public int Value2 { get; init; } = 2;
    
    /// <summary>
    /// Value 3
    /// </summary>
    public TimeSpan Value3 { get; set; } = TimeSpan.FromDays(1.0);

    /// <summary>
    /// Value 4
    /// </summary>
    public DateTime Value4 { get; set; } = new DateTime(2021, 1, 1, 2, 2, 2);
}

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

    /// <summary>
    /// Sub class
    /// </summary>
    public SubClass Value2 { get; set; } = new();
}

/// <summary>
/// Config class that binds to key DigitalRuby.SimpleDi.Tests.ConfigurationDynamic
/// </summary>
[Configuration(isDynamic: true)]
public sealed class ConfigurationDynamic
{
    /// <summary>
    /// Example value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Sub class
    /// </summary>
    public SubClass Value2 { get; set; } = new();
}