<h1 align='center'>SimpleDi</a>

## Declarative Dependency Injection and Configuration for .NET

SimpleDi allows you to inject interfaces, types and configuration using attributes. No need for complex frameworks or manually adding injections to your startup code.

You can also put service configuration and web application builder setup code in classes, allowing your class libraries to automatically be part of these processes.

## Setup
```cs
using DigitalRuby.SimpleDi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSimpleDi(builder.Configuration);

// for web apps (not needed for non-web apps):
var host = builder.Build();
host.UseSimpleDi(builder.Configuration);
```

## Assembly Scanning
By default, only assemblies with names prefixed with the first part of your entry assembly name will be  included for memory optimization purposes. You can change this in the `AddSimpleDi` and `UseSimpleDi` by passing a regex string to match assembly names.

```cs
builder.Services.AddSimpleDi(builder.Configuration, "assembly1|assembly2");
var host = builder.Build();
host.UseSimpleDi(host.Configuration, "assembly1|assembly2");
```

## Implementation

Create a class, `MyInterfaceImplementation`
```cs
// Interface will be available in constructors
public interface IMyInterface
{
}

// bind the class to simpledi, implementing IMyInterface
[Binding(BindingType.Singleton)]
public sealed class MyInterfaceImplementation : IMyInterface
{
}
```

Inject the interface into the constructor of another class:
```cs
[Binding(BindingType.Singleton)]
public sealed class MyClass
{
	public MyClass(IMyInterface myInterface)
	{
	}
}
```
## Hosted Services

Hosted services must be bound as a singleton, otherwise an exception is thrown

```cs
[Binding(BindingType.Singleton)]
public sealed class MyHostedClass : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```
## Select Interfaces
```cs
// only register MyClass concrete type by passing null as second argument
[Binding(BindingType.Singleton, null)]
public sealed class MyClass1 : IInterface1, IInterface2
{
}

// only register MyClass concrete type and IInterface1
[Binding(BindingType.Singleton, typeof(IInterface1))]
public sealed class MyClass2 : IInterface1, IInterface2
{
}
```

## Conflict Resolution

In a complex codebase, sometimes multiple implementations will try to register for the same interface.

When using the `BindingAttribute` you can specify an optional conflict resolution:
`public BindingAttribute(ServiceLifetime scope, ConflictResolution conflict, params Type[]? interfaces)`

```cs
// will always add the class as an implementation regardless of other bindings
[Binding(BindingType.Singleton, ConflictResolution.Add)]
public sealed class MyClass1 : IInterface1
{
}

// will always replace the other class as an implementation regardless of other bindings
[Binding(BindingType.Singleton, ConflictResolution.Replace)]
public sealed class MyClass2 : IInterface1
{
}

// if an implementation for interface is already registered, does nothing, otherwise add the class as an implementation of the interface
[Binding(BindingType.Singleton, ConflictResolution.Skip)]
public sealed class MyClass3 : IInterface1
{
}

// throw an exception if a binding already exists for the interface
[Binding(BindingType.Singleton, ConflictResolution.Error)]
public sealed class MyClass4 : IInterface1
{
}
```

Conflict resolution has four possible values:

```cs
/// <summary>
/// What to do if there is a conflict when registering services
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Add. This will result in multiple services for an interface if more than one are added.
    /// </summary>
    Add = 0,

    /// <summary>
    /// Replace. This will make sure only one implementation exists for an interface.
    /// </summary>
    Replace,

    /// <summary>
    /// Skip. Do not register this service if another implementation exits for the interface.
    /// </summary>
    Skip,

    /// <summary>
    /// Throw an exception if another class is registered for the interface.
    /// </summary>
    Error
}
```

If you need access to multiple bindings in a constructor, use `System.Collections.Generic.IEnumerable<T>`, where `T` is your interface.

## Configuration

Given a json file in your project `config.json` (set as content and copy newer in properties):
```json
{
  "DigitalRuby.SimpleDi.Tests.Configuration":
  {
    "Value": "hellothere"
  }
}
```

And a class with the same namespace as in the file...

```cs
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
    public string Value { get; set; } = string.Empty; // overriden from config
}

// appsettings.json:
{
  "DigitalRuby.SimpleDi.Tests.Configuration": { Value: "test" }
}

// instead of the default configuration path which is namespace.classname, you can override:
[Configuration("myconfig")]
public sealed class Configuration
{
    /// <summary>
    /// Example value
    /// </summary>
    public string Value { get; set; } = string.Empty; // overriden from config
}

// appsettings.json for override:
{
  "myconfig": { Value: "test" }
}
```

You can inject your `Configuration` class into any constructor as normal.

Make sure to add your config file to your configuration builder:

```cs
var builder = WebApplication.CreateBuilder();
builder.Configuration.AddJsonFile("config.json");
```

You can create multiple keys in your configuration file for each class annotated with the `Configuration` attribute, or use separate files.

Instead of custom files you can also just use your `appsettings.json` file, which is added by .NET automatically.

## Service setup code
You can create classes in your project or even class library to run service setup code. Simply inherit from `DigitalRuby.SimpleDi.IServiceSetup` and provide a private constructor with this exact signature:
```cs
internal class ServiceSetup : IServiceSetup
{
    private ServiceSetup(IServiceCollection services, IConfiguration configuration)
    {
        // perform service setup
    }
}
```

## Web app setup code
Similar to service setup code, you can create classes to run web app setup code:
```cs
internal class AppSetup : IWebAppSetup
{
    private AppSetup(IApplicationBuilder appBuilder, IConfiguration configuration)
    {
        // perform app setup
    }
}
```

---

Thank you for visiting!

-- Jeff

https://www.digitalruby.com
