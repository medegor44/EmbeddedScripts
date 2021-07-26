using System;
using System.IO;
using System.Reflection;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    internal class InstanceCreator
    {
        public InstanceCreator(CSharpCompilation compilation)
        {
            Compilation = compilation;
        }

        public InstanceWrapper CreateInstanceOf(string className, Container container)
        {
            var assembly = LoadAssemblyFromCompilation(Compilation);
            var type = assembly.GetType(className);
            return new InstanceWrapper(Activator.CreateInstance(type, container), type);
        }

        private Assembly LoadAssemblyFromCompilation(CSharpCompilation compilation)
        {
            using var memoryStream = new MemoryStream();
            var result = compilation.Emit(memoryStream);

            if (!result.Success)
            {
                var compilationException = new CompilationErrorException("Compilation failed", result.Diagnostics);
                throw new ScriptSyntaxErrorException("CompilationFailed", compilationException);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(memoryStream.ToArray());
        }

        private CSharpCompilation Compilation { get; }
    }
}