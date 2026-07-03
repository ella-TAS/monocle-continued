using System;
using System.Collections.Generic;

namespace Monocle {
    /// <summary>
    /// Provides object pooling functionality for Entity instances to reduce garbage collection pressure and improve performance.
    /// Maintains type-specific stacks of Entity objects that can be reused instead of creating new instances.
    /// </summary>
    public static class Cache {
        /// <summary>
        /// Dictionary mapping Entity types to stacks of cached instances for object pooling.
        /// </summary>
        public static Dictionary<Type, Stack<Entity>> cache;

        /// <summary>
        /// Initializes the cache for a specific Entity type if not already present.
        /// Creates the main cache dictionary and type-specific stack as needed.
        /// </summary>
        /// <typeparam name="T">The Entity type to initialize caching for. Must have a parameterless constructor.</typeparam>
        private static void Init<T>() where T : Entity, new() {
            if (cache == null)
                cache = new Dictionary<Type, Stack<Entity>>();
            if (!cache.ContainsKey(typeof(T)))
                cache.Add(typeof(T), new Stack<Entity>());
        }

        /// <summary>
        /// Stores an Entity instance in the cache for later reuse.
        /// The instance should be reset to its default state before storing.
        /// </summary>
        /// <typeparam name="T">The Entity type to store. Must have a parameterless constructor.</typeparam>
        /// <param name="instance">The Entity instance to cache for reuse.</param>
        public static void Store<T>(T instance) where T : Entity, new() {
            Init<T>();
            cache[typeof(T)].Push(instance);
        }

        /// <summary>
        /// Creates or retrieves an Entity instance from the cache.
        /// Returns a cached instance if available, otherwise creates a new one.
        /// </summary>
        /// <typeparam name="T">The Entity type to create. Must have a parameterless constructor.</typeparam>
        /// <returns>An Entity instance, either from cache or newly created.</returns>
        public static T Create<T>() where T : Entity, new() {
            Init<T>();
            if (cache[typeof(T)].Count > 0)
                return cache[typeof(T)].Pop() as T;
            else
                return new T();
        }

        /// <summary>
        /// Clears all cached instances for a specific Entity type.
        /// </summary>
        /// <typeparam name="T">The Entity type to clear from cache. Must have a parameterless constructor.</typeparam>
        public static void Clear<T>() where T : Entity, new() {
            if (cache != null && cache.ContainsKey(typeof(T)))
                cache[typeof(T)].Clear();
        }

        /// <summary>
        /// Clears all cached instances for all Entity types.
        /// Useful for memory cleanup between scenes or game states.
        /// </summary>
        public static void ClearAll() {
            if (cache != null)
                foreach (var kv in cache)
                    kv.Value.Clear();
        }
    }
}
