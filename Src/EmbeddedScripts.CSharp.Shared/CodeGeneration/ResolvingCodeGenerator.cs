using System;
using System.Linq;
using System.Text;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Shared.CodeGeneration
{
    public class ResolvingCodeGenerator
    {
        internal string GenerateVariablesDeclaration(Container container) =>
            GenerateCode(container, (alias, type) =>
                $"{GenerateTypeName(type)} {alias};");

        internal string GenerateVariablesInitialization(Container container, string containerName) =>
            GenerateCode(container, (alias, type) => 
                $"{alias} = {containerName}.Resolve<{GenerateTypeName(type)}>(\"{alias}\");");

        private string GenerateCode(Container container, Func<string, Type, string> generator) => 
            container
                .VariableAliases
                .Select(alias => generator(alias, container.GetTypeByAlias(alias)))
                .Aggregate("", (cur, nxt) => cur + nxt + Environment.NewLine);

        public string GenerateTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;
            
            var genericArguments = type.GetGenericArguments().Select(GenerateTypeName);
            
            var typeName = type.FullName;
            var delimiterPosition = typeName.IndexOf("`");
            var sb = new StringBuilder(typeName.Remove(delimiterPosition))
                .Append("<")
                .Append(string.Join(",", genericArguments))
                .Append(">");

            return sb.ToString();
        }
    }
}
