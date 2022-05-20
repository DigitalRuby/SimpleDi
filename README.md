# SimpleDi
## Declarative Dependency Injection and Configuration for .NET
---

SimpleDi allows you to inject interfaces and types using attributes. No need for complex frameworks or manually adding injections to your startup code.

Setup:
```cs
using DigitalRuby.SimpleDi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSimpleDi();
```
---
Implementation:

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
---
Simple di takes care of hosted services too.

```cs
[Binding(BindingType.Singleton)]
public sealed class MyHostedClass : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```
---
You can choose whether to bind no interfaces or only some interfaces:

```cs
// only register MyClass concrete type
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
---
Configuration:

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
```

You can inject your `Configuration` class into any constructor as normal.

Make sure to add your config file to your configuration builder:

```cs
var builder = WebApplication.CreateBuilder();
builder.Configuration.AddJsonFile("config.json");
```

You can create multiple keys in your configuration file for each class annotated with the `Configuration` attribute, or use separate files.

Instead of custom files you can also just use your `appsettings.json` file, which is added by .NET automatically.

---
Thank you for visiting!

-- Jeff