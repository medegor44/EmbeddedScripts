using System.Linq;
using System.Reflection;
using Mono.CSharp;

namespace EmbeddedScripts.CSharp.MonoEvaluator
{
    internal static class EvaluatorExtension
    {
        internal static bool TryReferenceAssembly(this Evaluator evaluator, Assembly assembly)
        {
            var importer = (ReflectionImporter) evaluator.GetType()
                .GetField("importer", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(evaluator);
            
            var referencedAssembliesNames = importer?.Assemblies.Select(a => a.FullName);
            var containsAssembly = referencedAssembliesNames?.Contains(assembly.FullName) ?? false;

            if (containsAssembly)
                return false;
            
            evaluator.ReferenceAssembly(assembly);

            return true;
        }
    }
}