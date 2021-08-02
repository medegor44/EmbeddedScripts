using EmbeddedScripts.Shared;
using Jint;
using System.Linq;

namespace EmbeddedScripts.JS.Jint
{
    internal static class EngineExtension
    {
        internal static Engine SetValuesFromContainer(this Engine engine, Container container) =>
            container
                .VariableAliases
                .Aggregate(engine, (currentEngine, alias) =>
                    currentEngine.SetValueFromContainer(container, alias));

        private static Engine SetValueFromContainer(this Engine engine, Container container, string alias) =>
            engine.SetValue(alias, container.Resolve(alias));
    }
}