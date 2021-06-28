using EmbeddedScripts.CSharp.Shared.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration
{
    public class CodeGeneratorForCompilation
    {
        public string GenerateCode(string code, Container container)
        {
            var containerTypeFullName = typeof(Container).FullName;
            var fieldsInitialization = ResolvingCodeGenerator.GenerateVariablesInitialization(container, ContainerName);
            var fieldsDeclaration = ResolvingCodeGenerator.GenerateVariablesDeclaration(container);

            var codeForCompilation = $@"
public class {ClassName}
{{
    public {ClassName}({containerTypeFullName} {ContainerName})
    {{
        {fieldsInitialization}
    }}

    public void {MethodName}() 
    {{ 
       {code}
    }} 

    {fieldsDeclaration}
}}";

            return codeForCompilation;
        }

        public string ClassName => "Runner";
        public string MethodName => "Run";
        public string ContainerName => "MyContainer";
        private ResolvingCodeGenerator ResolvingCodeGenerator { get; } = new();
    }
}