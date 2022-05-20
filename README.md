SimpleDi allows you to inject interfaces and types very easily. No need for complex frameworks or manually adding injections to your startup code.

Simplest example:

```cs
using DigitalRuby.SimpleDi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSimpleDi();
```

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

Simple di takes care of hosted services too

```cs
[Binding(BindingType.Singleton)]
public sealed class MyHostedClass : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

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
