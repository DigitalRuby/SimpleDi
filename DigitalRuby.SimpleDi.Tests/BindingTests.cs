namespace DigitalRuby.SimpleDi.Tests;

/// <summary>
/// Binding tests
/// </summary>
[TestFixture]
public sealed class BindingTests
{
    /// <summary>
    /// Test that we can use binding and configuration attributes with non-web application
    /// </summary>
    [Test]
    public async Task TestHostBuilder()
    {
        var started = false;
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureAppConfiguration(builder =>
        {
            builder.AddJsonFile("Configuration.json");
        });
        builder.ConfigureServices((context, services) =>
        {
            Assert.That(services.SimpleDiAdded(), Is.False);
            services.AddSimpleDi(context.Configuration, "digitalruby");
            Assert.That(services.SimpleDiAdded(), Is.True);
        });
        using var host = builder.Build();
        var lifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifeTime.ApplicationStarted.Register(() => started = true);
        host.RunAsync().GetAwaiter();
        while (!started)
        {
            await Task.Delay(1);
        }
        TestInternal(host.Services, false);
    }

    /// <summary>
    /// Test that we can use binding and configuration attributes with web application
    /// </summary>
    [Test]
    public async Task TestWebHostBuilder()
    {
        var started = false;
        var builder = WebApplication.CreateBuilder(new[] { "--urls", "http://localhost:54269" });
        builder.Configuration.AddJsonFile("Configuration.json");
        Assert.That(builder.Services.SimpleDiAdded(), Is.False);
        builder.Services.AddSimpleDi(builder.Configuration, "digitalruby");
        Assert.That(builder.Services.SimpleDiAdded(), Is.True);
        using var host = builder.Build();
        host.UseSimpleDi(builder.Configuration);
        var lifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifeTime.ApplicationStarted.Register(() => started = true);
        host.RunAsync().GetAwaiter();
        while (!started)
        {
            await Task.Delay(1);
        }
        TestInternal(host.Services, true);
    }
    
    /// <summary>
    /// Test that we throw an exception if conflict resolution error
    /// </summary>
    [Test]
    public void TestConflictResolutionError()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((context, services) =>
        {
            services.AddSimpleDi(context.Configuration);
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            builder.Build();
        });
    }

    private static void TestInternal(IServiceProvider services, bool isWebApp)
    {
        // service/web setup constructor works
        {
            var serviceSetup = ServiceSetup.Instance;
            ServiceSetup.Instance = null;
            var webAppSetup = WebAppSetup.Instance;
            WebAppSetup.Instance = null;

            Assert.Multiple(() =>
            {
                Assert.That(serviceSetup, Is.Not.Null);

                // web application builder constructor works
                if (isWebApp)
                {
                    Assert.That(webAppSetup, Is.Not.Null);
                }
            });
        }

        // simple service bindings work
        {
            var sayHello = services.GetRequiredService<Hello>();
            var sayHelloInterface = services.GetRequiredService<IHello>();
            Assert.Multiple(() =>
            {
                Assert.That(sayHello.SayHello(), Is.EqualTo("hello"));
                Assert.That(sayHelloInterface.SayHello(), Is.EqualTo("hello"));

                // make sure they are the same thing
                Assert.That(sayHelloInterface, Is.SameAs(sayHello));
            });
        }

        // config bindings work
        {
            var config = services.GetRequiredService<Configuration>();
            Assert.That(config.Value, Is.EqualTo("hellothere"));
        }

        // does not bind interfaces when null param in attribute
        {
            var sayHello2 = services.GetRequiredService<HelloNoInterface>();
            var sayHello2Interface = services.GetService<IHello2>();
            Assert.Multiple(() =>
            {
                Assert.That(sayHello2.SayHello(), Is.EqualTo("hello"));
                Assert.That(sayHello2Interface, Is.Null);
            });
        }

        // only binds specified interface in attribute
        {
            var sayHello3 = services.GetRequiredService<HelloOneInterface>();
            var sayHello2Interface = services.GetRequiredService<IHello3>();
            var sayHello3Interface = services.GetService<IHello4>();
            Assert.Multiple(() =>
            {
                Assert.That(sayHello3.SayHello(), Is.EqualTo("hello"));
                Assert.That(sayHello2Interface.SayHello(), Is.EqualTo("hello"));
                Assert.That(sayHello3Interface, Is.Null);

                // make sure they are the same thing
                Assert.That(sayHello2Interface, Is.SameAs(sayHello3));
            });
        }

        // test our hosted service spun up
        {
            var hostedServices = services.GetServices<IHostedService>().ToArray();
            var hostedServiceImpl = services.GetRequiredService<HelloHostedServie>();

            Assert.Multiple(() =>
            {
                Assert.That(hostedServiceImpl.Started, Is.True);

                int registerCount = hostedServices.Where(s => s == hostedServiceImpl).Count();

                // make sure they are the same thing and only a single instance is registered
                Assert.That(registerCount, Is.EqualTo(1));
            });
        }
    }
}
