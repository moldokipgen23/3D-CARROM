using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static readonly object _lock = new object();

    public static void Register<T>(T service) where T : IService
    {
        lock (_lock)
        {
            var key = typeof(T);
            if (_services.ContainsKey(key))
            {
                Debug.LogWarning($"Service of type {key} is already registered. Overwriting.");
            }
            _services[key] = service;
            Debug.Log($"Registered service: {key}");
        }
    }

    public static T Get<T>() where T : IService
    {
        lock (_lock)
        {
            var key = typeof(T);
            if (_services.TryGetValue(key, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"No service of type {key} found.");
            return default;
        }
    }

    public static void Unregister<T>() where T : IService
    {
        lock (_lock)
        {
            var key = typeof(T);
            if (_services.Remove(key))
            {
                Debug.Log($"Unregistered service: {key}");
            }
            else
            {
                Debug.LogWarning($"No service of type {key} to unregister.");
            }
        }
    }

    public static bool HasService<T>() where T : IService
    {
        lock (_lock)
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}