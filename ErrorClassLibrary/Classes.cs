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

/// <summary>
/// Test conflict resolution of type error
/// </summary>
[DigitalRuby.SimpleDi.Binding(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public sealed class ErrorImplementation : ErrorClassLibrary.IErrorInterface
{
}