using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EmbeddedScripts.CSharp.MonoEvaluator.Hosting
{
    public static class HostFields
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, object>> Fields = 
            new ConcurrentDictionary<int, Dictionary<string, object>>();

        internal static void CreateNewEntry(int id)
        {
            if (!Fields.ContainsKey(id))
                Fields.TryAdd(id, new Dictionary<string, object>());
        }

        public static Dictionary<string, object> Get(int id) => 
            Fields[id];
    }
}