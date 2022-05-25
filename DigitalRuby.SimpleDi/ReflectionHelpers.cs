namespace DigitalRuby.SimpleDi;

/// <summary>
/// Delegate for object activator
/// </summary>
/// <typeparam name="T">Type of object</typeparam>
/// <param name="args">Args</param>
/// <returns>Created object</returns>
public delegate T ObjectActivator<T>(params object[] args);

/// <summary>
/// Reflection helper methods
/// </summary>
public static class ReflectionHelpers
{
	private static HashSet<Assembly>? allAssemblies;
    private static readonly ConcurrentDictionary<string, Type?> typeCache = new();

    /// <summary>
    /// Parses the properties of an object into a dictionary, unless the object is a dictionary in which case it is simply returned.
    /// </summary>
    /// <param name="obj">Object</param>
    /// <returns>Dictionary, or empty dictionary if obj is null</returns>
    public static IDictionary<string, object?> ParseProperties(this object? obj)
	{
		if (obj is IDictionary<string, object?> objAsDictionary)
		{
			return objAsDictionary;
		}
		else if (obj is IDictionary<string, string?> objAsStringDictionary)
		{
			return objAsStringDictionary.ToDictionary(x => x.Key, x => (object?)x.Value);
		}

		Dictionary<string, object?> dictionary = new();
		if (obj is not null)
		{
			var properties = TypeDescriptor.GetProperties(obj);
			foreach (PropertyDescriptor? property in properties)
			{
				if (property != null)
				{
					dictionary.Add(property.Name, property.GetValue(obj));
				}
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Returns the property value of an object.
	/// </summary>
	/// <param name="obj">Object</param>
	/// <param name="propertyName">Property name</param>
	/// <returns>Value or null if obj is null or no property exists</returns>
	public static object? GetPropertyValue(this object? obj, string propertyName)
	{
		if (obj is null || string.IsNullOrWhiteSpace(propertyName))
		{
			return null;
		}

		PropertyInfo? propertyInfo = obj.GetType().GetProperty(propertyName);
		if (propertyInfo is null)
		{
			return null;
		}

		return propertyInfo.GetValue(obj, null);
	}

#nullable disable

	/// <summary>
	/// Returns the value of a given selector on an object if it is not null, otherwise returns the default of the selected value.
	/// </summary>
	/// <param name="obj">Object</param>
	/// <param name="selector">Selector</param>
	/// <returns>Property</returns>
	public static TProperty ValueOrDefault<T, TProperty>(this T obj, Func<T, TProperty> selector)
	{
		return EqualityComparer<T>.Default.Equals(obj, default) ? default : selector(obj);
	}

#nullable restore

	/// <summary>
	/// Returns the default value for a given Type.
	/// </summary>
	/// <param name="type">Type</param>
	/// <returns>Object or null if type is null</returns>
	/// <exception cref="ArgumentException">Invalid type is specified</exception>
	public static object? GetDefault(this Type? type)
	{
		// If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
		if (type is null || !type.IsValueType || type == typeof(void))
		{
			return null;
		}

		// If the supplied Type has generic parameters, its default value cannot be determined
		if (type.ContainsGenericParameters)
		{
			throw new ArgumentException($"The type '{type}' contains generic parameters, so the default value cannot be retrieved.");
		}

		// If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
		//  default instance of the value type
		if (type.IsPrimitive || !type.IsNotPublic)
		{
			try
			{
				return Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				throw new ArgumentException($"Could not create a default instance of the type '{type}'.", ex);
			}
		}

		// Fail with exception
		throw new ArgumentException($"The type '{type}' is not a publicly-visible type, so the default value cannot be retrieved.");
	}

	/// <summary>
	/// Get all assemblies, including referenced assemblies. This method will be cached beyond the first call.
	/// </summary>
	/// <returns>All referenced assemblies</returns>
	public static IEnumerable<Assembly> GetAllAssemblies()
	{
		// this method can be called concurrently it will eventually settle
		if (allAssemblies is not null)
		{
			return allAssemblies;
		}
        var allAssembliesHashSet = new HashSet<Assembly>();
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().ToArray())
		{
			allAssembliesHashSet.Add(assembly);
			AssemblyName[] references = assembly.GetReferencedAssemblies();
			foreach (AssemblyName reference in references)
			{
				try
				{
					Assembly referenceAssembly = Assembly.Load(reference);
					allAssembliesHashSet.Add(referenceAssembly);
				}
				catch
				{
					// don't care, if the assembly can't be loaded there's nothing more to be done
				}
			}
		}

		// get referenced assemblies does not include every assembly if no code was referenced from that assembly
		string path = AppContext.BaseDirectory;

		// grab all dll files in case they were not automatically referenced by the app domain
		foreach (string dllFile in Directory.GetFiles(path).Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
		{
			try
			{
				bool exists = false;
				foreach (Assembly assembly in allAssembliesHashSet)
				{
					try
					{
						exists = assembly.Location.Equals(dllFile, StringComparison.OrdinalIgnoreCase);
						if (exists)
						{
							break;
						}
					}
					catch
					{
						// some assemblies will throw upon attempt to access Location property...
					}
				}
				if (!exists)
				{
					allAssembliesHashSet.Add(Assembly.LoadFrom(dllFile));
				}
			}
			catch
			{
				// nothing to be done
			}
		}

		return allAssemblies = allAssembliesHashSet;
	}

    private static string GetDefaultNamespaceFilter()
    {
		string entryName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

		// special case for tests, just load everything
		if (entryName.Equals("testhost", StringComparison.OrdinalIgnoreCase))
		{
			return string.Empty;
		}

        // only auto bind from assemblies related to our root namespace
        string? namespaceFilter = entryName;
        int dot = namespaceFilter.IndexOf('.');
        if (dot >= 0)
        {
			namespaceFilter = namespaceFilter[..dot];
        }
        return "^" + namespaceFilter.Replace(".", "\\.");
    }

	private static IReadOnlyCollection<Assembly> GetAssemblies(string? namespaceFilterRegex)
	{
		namespaceFilterRegex ??= GetDefaultNamespaceFilter();
		var allAssemblies = GetAllAssemblies();
		return allAssemblies.Where(a =>

			// no filter, grab the types
			string.IsNullOrWhiteSpace(namespaceFilterRegex) ||

			// no full name, grab the types
			a.FullName is null ||

			// assembly name matches filter, grab the types
			Regex.IsMatch(a.FullName, namespaceFilterRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline)).ToArray();
	}

	/// <summary>
	/// Get all types from all assemblies
	/// </summary>
	/// <param name="namespaceFilterRegex">Optional namespace filter (regex), null for default filter, empty for all assemblies (this will increase memory usage)</param>
	/// <returns>All types</returns>
	public static IReadOnlyCollection<Type> GetAllTypes(string? namespaceFilterRegex = null)
	{
		namespaceFilterRegex ??= GetDefaultNamespaceFilter();
		var allTypesList = new List<Type>();
		var matchedAssemblies = GetAssemblies(namespaceFilterRegex);
		foreach (Assembly a in matchedAssemblies)
		{
			try
			{
				foreach (Type t in a.GetTypes())
				{
					allTypesList.Add(t);
				}
			}
			catch
			{
				// ignore, assemblies like intellitrace throw
			}
		}
		return allTypesList;
	}

    /// <summary>
    /// Search all assemblies for a type. Results, including not found types, will be cached permanently in memory.
    /// </summary>
    /// <param name="fullName">Type full name</param>
    /// <param name="namespaceFilterRegex">Optional namespace filter (regex), null for default filter, empty for all assemblies (this will increase memory usage)</param>
    /// <returns>Type or null if none found</returns>
    public static Type? GetType(string fullName, string? namespaceFilterRegex = null)
	{
		Type? type = typeCache.GetOrAdd(fullName + "|" + namespaceFilterRegex, _key =>
		{
			_key = _key[.._key.IndexOf('|')];
			Type? type = Type.GetType(_key);
			if (type is not null)
			{
				return type;
			}
			foreach (Assembly assembly in GetAssemblies(namespaceFilterRegex))
			{
				type = assembly.GetType(_key);
				if (type is not null)
				{
					return type;
				}
			}
			return null;
		});
		return type;
	}

	/// <summary>
	/// Get an object creator that is much faster than Activator.CreateInstance
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	/// <param name="ctor">Constructor to use</param>
	/// <returns>Activator</returns>
	/// <remarks>You can get a constructor by doing <code>typeof(T)?.GetConstructor(types);</code></remarks>
	public static ObjectActivator<T>? GetActivator<T>(this ConstructorInfo ctor)
	{
		if (ctor is null)
		{
			return null;
		}

		ParameterInfo[] paramsInfo = ctor.GetParameters();

		// create a single param of type object[]
		ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
		Expression[] argsExp = new Expression[paramsInfo.Length];

		// pick each arg from the params array and create a typed expression of them
		for (int i = 0; i < paramsInfo.Length; i++)
		{
			Expression index = Expression.Constant(i);
			Type paramType = paramsInfo[i].ParameterType;
			Expression paramAccessorExp = Expression.ArrayIndex(param, index);
			Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
			argsExp[i] = paramCastExp;
		}

		// make a NewExpression that calls the ctor with the args we just created
		NewExpression newExp = Expression.New(ctor, argsExp);

		// create a lambda with the new expression as body and our param object[] as arg
		LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

		// compile it
		ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();

		return compiled;
	}

	/// <summary>
	/// Determine if a type is an anonymous type
	/// </summary>
	/// <param name="type">Type</param>
	/// <returns>True if anonymous, false otherwise</returns>
	public static bool IsAnonymousType(this Type type)
	{
		if (type.FullName is null)
		{
			return false;
		}
		bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
		bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType", StringComparison.OrdinalIgnoreCase);
		bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
		return isAnonymousType;
	}

	/// <summary>
	/// Clear all caches, free up memory
	/// </summary>
	public static void ClearCaches()
	{
		typeCache.Clear();
	}
}
