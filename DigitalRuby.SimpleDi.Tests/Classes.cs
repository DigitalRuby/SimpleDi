namespace DigitalRuby.SimpleDi.Tests;

/// <summary>
/// Sample interface
/// </summary>
public interface IHello
{
    /// <summary>
    /// Return hello
    /// </summary>
    /// <returns>hello</returns>
    string SayHello();
}

/// <summary>
/// Another interface
/// </summary>
public interface IHello2
{
    /// <summary>
    /// Return hello
    /// </summary>
    /// <returns>hello</returns>
    string SayHello();
}

/// <summary>
/// Another interface
/// </summary>
public interface IHello3
{
    /// <summary>
    /// Return hello
    /// </summary>
    /// <returns>hello</returns>
    string SayHello();
}

/// <summary>
/// Another interface
/// </summary>
public interface IHello4
{
    /// <summary>
    /// Return hello
    /// </summary>
    /// <returns>hello</returns>
    string SayHello();
}

/// <summary>
/// Another interface
/// </summary>
public interface IHello5
{
    /// <summary>
    /// Return hello
    /// </summary>
    /// <returns>hello</returns>
    string SayHello();
}

/// <summary>
/// Sample implementation that is bound at runtime
/// </summary>
[Binding(ServiceLifetime.Singleton)]
public sealed class Hello : IHello
{
    /// <inheritdoc />
    public string SayHello() { return "hello"; }
}

/// <summary>
/// Sample implementation that does not bind the interface
/// </summary>
[Binding(ServiceLifetime.Singleton, null)]
public sealed class HelloNoInterface : IHello2
{
    /// <inheritdoc />
    public string SayHello() { return "hello"; }
}

/// <summary>
/// Sample implementation that does one interface
/// </summary>
[Binding(ServiceLifetime.Singleton, typeof(IHello3))]
public sealed class HelloOneInterface : IHello3, IHello4
{
    /// <inheritdoc />
    public string SayHello() { return "hello"; }
}

/// <summary>
/// Sample hosted service
/// </summary>
[Binding(ServiceLifetime.Singleton)]
public sealed class HelloHostedServie : IHostedService, IHello5
{
    /// <summary>
    /// Have we started?
    /// </summary>
    public bool Started { get; private set; }

    /// <inheritdoc />
    public string SayHello() { return "hello"; }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Started = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}