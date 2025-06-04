// Assets/Scripts/Infrastructure/DIContainer.cs
using System;
using System.Collections.Generic;

public static class DIContainer
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        var type = typeof(T);
        services[type] = service;
    }

    public static T Resolve<T>()
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }
        throw new Exception($"Service of type {type} not registered");
    }
}
