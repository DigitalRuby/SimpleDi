namespace DigitalRuby.SimpleDi;

/// <summary>
/// Dependency injection helper for services
/// </summary>
public static class ServicesHelper
{
    /// <summary>
    /// Bind any class to service if class is annotated with either <see cref="DigitalRuby.SimpleDi.BindingAttribute" /> or <see cref="DigitalRuby.SimpleDi.ConfigurationAttribute" />.
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="namespaceFilterRegex"></param>
    public static void AddSimpleDi(this IServiceCollection services, IConfiguration configuration, string? namespaceFilterRegex = null)
    {
        BindBindingAttributes(services, namespaceFilterRegex);
        BindConfigurationAttributes(services, configuration, namespaceFilterRegex);
    }

    /// <summary>
    /// Bind an object from configuration as a singleton
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="services">Services</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="key">Key to read from configuration</param>
    public static void BindSingleton<T>(this IServiceCollection services, IConfiguration configuration, string key) where T : class, new()
    {
        T configObj = new();
        configuration.Bind(key, configObj);
        services.AddSingleton<T>(configObj);
    }

    /// <summary>
    /// Bind an object from configuration as a singleton
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="type">Type of object to bind</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="key">Key to read from configuration</param>
    public static void BindSingleton(this IServiceCollection services, Type type, IConfiguration configuration, string key)
    {
        object configObj = Activator.CreateInstance(type) ?? throw new ApplicationException("Failed to create object of type " + type.FullName);
        configuration.Bind(key, configObj);
        services.AddSingleton(type, configObj);
    }

    /// <summary>
    /// Add a singleton instance
    /// </summary>
    /// <typeparam name="T">Type of service</typeparam>
    /// <param name="services">Services collection</param>
    /// <param name="additionalTypes">Additional types to register as</param>
    public static void AddSingletonTypes<T>(this IServiceCollection services, params Type[] additionalTypes) where T : class
    {
        services.AddSingleton<T>();
        foreach (Type type in additionalTypes)
        {
            services.AddSingleton(type, (provider) => provider.GetRequiredService<T>());
        }
    }

    /// <summary>
    /// Add a hosted service as a singleton
    /// </summary>
    /// <typeparam name="T">Type of hosted service</typeparam>
    /// <param name="services">Services collection</param>
    /// <param name="additionalTypes">Additional types to register as</param>
    public static void BindSingletonHostedServiceTypes<T>(this IServiceCollection services, params Type[] additionalTypes) where T : class, IHostedService
    {
        services.AddSingleton<T>();
        services.AddHostedService<T>(provider => provider.GetRequiredService<T>());
        foreach (Type type in additionalTypes)
        {
            services.AddSingleton(type, provider => provider.GetRequiredService<T>());
        }
    }

    /// <summary>
    /// Add a hosted service as a singleton
    /// </summary>
    /// <typeparam name="T">Type of hosted service</typeparam>
    /// <param name="services">Services collection</param>
    /// <param name="factory">Factory, optional</param>
    /// <param name="additionalTypes">Additional types to register as</param>
    public static void BindingletonHostedService<T>(this IServiceCollection services, Func<IServiceProvider, T>? factory = null, params Type[] additionalTypes) where T : class, IHostedService
    {
        if (factory is null)
        {
            services.AddSingleton<T>();
        }
        else
        {
            services.AddSingleton<T>(provider => factory(provider));
        }
        services.AddHostedService<T>(provider => provider.GetRequiredService<T>());
        foreach (Type type in additionalTypes)
        {
            services.AddSingleton(type, provider => provider.GetRequiredService<T>());
        }
    }

    /// <summary>
    /// Bind all services with binding attribute
    /// </summary>
    /// <param name="services">Service collection</param>
	/// <param name="namespaceFilterRegex">Namespace filter regex to restrict to only a subset of assemblies</param>
    private static void BindBindingAttributes(this IServiceCollection services,
		string? namespaceFilterRegex = null)
    {
        Type attributeType = typeof(BindingAttribute);
        foreach (var type in ReflectionHelpers.GetAllTypes(namespaceFilterRegex))
        {
            var attr = type.GetCustomAttributes(attributeType, true);
            if (attr is not null && attr.Length != 0)
            {
                ((BindingAttribute)attr[0]).BindServiceOfType(services, type);
            }
        }
    }

    /// <summary>
    /// Bind all classes with configuration attribute from configuration as singletons
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="namespaceFilterRegex">Namespace filter regex to restrict to only a subset of assemblies</param>
    private static void BindConfigurationAttributes(this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
		IConfiguration configuration,
		string? namespaceFilterRegex = null)
	{
		var attributeType = typeof(ConfigurationAttribute);
		foreach (var type in ReflectionHelpers.GetAllTypes(namespaceFilterRegex))
		{
			var attr = type.GetCustomAttributes(attributeType, true);
			if (attr is not null && attr.Length != 0)
			{
				var path = ((ConfigurationAttribute)attr[0]).ConfigPath;
                if (path is null)
                {
                    path = type.FullName ?? throw new ArgumentNullException("Unable to get full name from type " + type);
                }
				services.BindSingleton(type, configuration, path);
			}
		}
	}
}
