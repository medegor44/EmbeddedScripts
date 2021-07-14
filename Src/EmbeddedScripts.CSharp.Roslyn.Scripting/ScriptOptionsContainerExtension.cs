using System.Linq;
using EmbeddedScripts.Shared;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting
{
    public static class ScriptOptionsContainerExtension
    {
        internal static ScriptOptions WithReferencesFromContainer(this ScriptOptions options, Container container) =>
            options.WithReferences(
                container.VariableAliases
                    .Select(container.GetTypeByAlias)
                    .Select(type => type.Assembly)
                    .Concat(new[] {typeof(Container).Assembly})
            );
    }
}