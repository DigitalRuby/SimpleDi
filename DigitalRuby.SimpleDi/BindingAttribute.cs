namespace DigitalRuby.SimpleDi;

/// <summary>
/// Register a class for DI
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BindingAttribute : Attribute
{
    /// <summary>
    /// Service scope
    /// </summary>
    public ServiceLifetime Scope { get; }

    /// <summary>
    /// The interfaces to bind or null for all interfaces
    /// </summary>
    public IReadOnlyCollection<Type>? Interfaces { get; }

    private static readonly IReadOnlyCollection<Type> globalInterfacesToIgnore = new HashSet<Type>
    {
        typeof(IDisposable),
        typeof(IAsyncDisposable),
        typeof(ICloneable),
        typeof(IComparable),
        typeof(IComparer),
        typeof(IConvertible),
        typeof(IEnumerable),
        typeof(IEnumerator),
        typeof(IEqualityComparer),
        typeof(IEquatable<>),
        typeof(IList),
        typeof(IOrderedEnumerable<>),
        typeof(IOrderedQueryable),
        typeof(IOrderedQueryable<>),
        typeof(IQueryable),
        typeof(IQueryable<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyDictionary<,>),
        typeof(IReadOnlyList<>),
        typeof(IReadOnlySet<>),
        typeof(ISet<>)
    };

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scope">Lifetime of this service</param>
    /// <param name="interfaces">The interfaces to bind, or pass null array for no interfaces</param>
    public BindingAttribute(ServiceLifetime scope, params Type[]? interfaces)
    {
        Scope = scope;
        Interfaces = interfaces;
    }

    /// <summary>
    /// Bind a service given the life time
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="type">Type of service</param>
    public void BindServiceOfType(IServiceCollection services, Type type)
    {
        if (type.IsAbstract ||
            (Scope != ServiceLifetime.Singleton && Scope != ServiceLifetime.Scoped && Scope != ServiceLifetime.Transient))
        {
            return;
        }

        // register the concrete type
        services.Add(new ServiceDescriptor(type, type, Scope));

        if (Interfaces is not null)
        {
            var interfacesToBind = (Interfaces.Count == 0 ? type.GetInterfaces() : Interfaces);
            foreach (var interfaceToBind in interfacesToBind)
            {
                if (!globalInterfacesToIgnore.Contains(interfaceToBind))
                {
                    if (interfaceToBind == typeof(IHostedService))
                    {
                        if (Scope != ServiceLifetime.Singleton)
                        {
                            throw new InvalidOperationException("Hosted services should always be singletons");
                        }
                    }
                    services.Add(new ServiceDescriptor(interfaceToBind, provider => provider.GetRequiredService(type), Scope));
                }
            }
        }
    }
}
