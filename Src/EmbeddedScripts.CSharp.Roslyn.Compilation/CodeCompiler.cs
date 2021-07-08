using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmbeddedScripts.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    internal class CodeCompiler
    {
        public CodeCompiler(string code, Container container)
        {
            Container = container;
            Compilation = Compile(code);
        }

        private CSharpCompilation Compile(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();
            var references = GetMetadataReferences(Container);
            var opts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            return CSharpCompilation
                .Create(assemblyName)
                .WithReferences(references)
                .WithOptions(opts)
                .AddSyntaxTrees(syntaxTree);
        }

        private IEnumerable<MetadataReference> GetMetadataReferences(Container container)
        {
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

            return GetMetadataReferencesFromContainer(container)
                .Concat(new []
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "netstandard.dll")),
                    MetadataReference.CreateFromFile(typeof(Container).Assembly.Location)
                });
        }

        private IEnumerable<MetadataReference> GetMetadataReferencesFromContainer(Container container) =>
            Container.VariableAliases
                .Select(container.GetTypeByAlias)
                .Select(type => type.Assembly.Location)
                .Select(location => MetadataReference.CreateFromFile(location));

        public CSharpCompilation Compilation { get; }

        private Container Container { get; }
    }
}
