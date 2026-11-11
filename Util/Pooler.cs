using System;
using System.Collections.Generic;
using System.Reflection;

namespace Monocle {
    public class Pooler {
        internal Dictionary<Type, Stack<Entity>> Pools { get; private set; }

        public Pooler() {
            Pools = new Dictionary<Type, Stack<Entity>>();

            foreach (Type type in Assembly.GetEntryAssembly()!.GetTypes()) {
                if (type.GetCustomAttributes(typeof(Pooled), false).Length > 0) {
                    if (!typeof(Entity).IsAssignableFrom(type))
                        throw new Exception("Type '" + type.Name + "' cannot be Pooled because it doesn't derive from Entity");
                    else if (type.GetConstructor(Type.EmptyTypes) == null)
                        throw new Exception("Type '" + type.Name + "' cannot be Pooled because it doesn't have a parameterless constructor");
                    else
                        Pools.Add(type, new Stack<Entity>());
                }
            }
        }

        public T Create<T>() where T : Entity, new() {
            if (!Pools.TryGetValue(typeof(T), out Stack<Entity>? pool) || pool.Count == 0)
                return new T();

            return (T) pool.Pop();
        }

        internal void EntityRemoved(Entity entity) {
            if (Pools.TryGetValue(entity.GetType(), out Stack<Entity>? pool))
                pool.Push(entity);
        }

        public void Log() {
            if (Pools.Count == 0)
                Engine.Commands.Log("No Entity types are marked as Pooled!");

            foreach (KeyValuePair<Type, Stack<Entity>> kv in Pools) {
                string output = kv.Key.Name + " : " + kv.Value.Count;
                Engine.Commands.Log(output);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Pooled : Attribute {

    }
}
