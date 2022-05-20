namespace DigitalRuby.SimpleDi;

/// <summary>
/// Register a class for DI from configuration
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationAttribute : Attribute
{
	/// <summary>
	/// Config path
	/// </summary>
	public string? ConfigPath { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configPath">Config path to bind from configuration. This can be left null to use the FullName from the type this attribute annotates.</param>
	public ConfigurationAttribute(string? configPath = null)
	{
		ConfigPath = configPath;
	}
}
