namespace ErrorClassLibrary;

/// <summary>
/// Test conflict of error
/// </summary>
public interface IErrorInterface { }

/// <inheritdoc />
[DigitalRuby.SimpleDi.Binding(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, DigitalRuby.SimpleDi.ConflictResolution.Error)]
public sealed class ErrorClassLibraryClass : IErrorInterface
{

}