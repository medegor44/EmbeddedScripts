using EmbeddedScripts.Shared;
using Jint;
using System.Linq;

namespace EmbeddedScripts.JS.Jint
{
    public static class EngineExtension
    {
        public static Engine SetVariablesFromContainer(this Engine engine, Container container) =>
            container
                .VariableAliases
                .Aggregate(engine, (currentEngine, alias) =>
                    currentEngine.SetValue(alias, container.Resolve(alias)));
    }
}