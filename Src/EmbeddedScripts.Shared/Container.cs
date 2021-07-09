using System;
using System.Collections.Generic;

namespace EmbeddedScripts.Shared
{
    public class Container
    {
        public void Register<T>(T instance, string alias)
        {
            if (Instances.ContainsKey(alias))
                throw new ArgumentException("Instance with such alias already exists");

            Types[alias] = typeof(T);
            Instances[alias] = instance;
        }

        public T Resolve<T>(string alias) => 
            (T) Resolve(alias);

        public object Resolve(string alias)
        {
            if (!Instances.ContainsKey(alias))
                throw new ArgumentException("Instance with such name does not exists");

            return Instances[alias];
        }

        public IEnumerable<string> VariableAliases => Types.Keys;
        public Type GetTypeByAlias(string alias) => Types[alias];

        private Dictionary<string, Type> Types { get; } = new();
        private Dictionary<string, object> Instances { get; } = new();
    }
}
