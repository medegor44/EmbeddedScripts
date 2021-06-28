using System;
using System.Linq;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Shared.CodeGeneration
{
    public class ResolvingCodeGenerator
    {
        public string GenerateVariablesDeclaration(Container container) =>
            GenerateCode(container, (alias, type) =>
                $"{type.FullName} {alias};");

        public string GenerateVariablesInitialization(Container container, string containerName) =>
            GenerateCode(container, (alias, type) => 
                $"{alias} = {containerName}.Resolve<{type.FullName}>(\"{alias}\");");

        private string GenerateCode(Container container, Func<string, Type, string> generator) => 
            container
                .VariableAliases
                .Select(alias => generator(alias, container.GetTypeByAlias(alias)))
                .Aggregate("", (cur, nxt) => cur + nxt + Environment.NewLine);
    }
}
