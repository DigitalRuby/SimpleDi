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
	/// Dynamic, can be loaded at runtime and hot reloaded
	/// </summary>
	public bool IsDynamic { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configPath">Config path to bind from configuration. This can be left null to use the FullName from the type this attribute annotates.</param>
	/// <param name="isDynamic">True (dynamic) can reload values at runtime, false (static) loads values once at startup.</param>
	public ConfigurationAttribute(string? configPath = null, bool isDynamic = false)
	{
		ConfigPath = configPath;
		IsDynamic = isDynamic;
	}
}
