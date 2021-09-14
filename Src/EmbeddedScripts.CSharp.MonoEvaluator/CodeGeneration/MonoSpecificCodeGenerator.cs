using System;
using EmbeddedScripts.CSharp.Shared.CodeGeneration;

namespace EmbeddedScripts.CSharp.MonoEvaluator.CodeGeneration
{
    public class MonoSpecificCodeGenerator
    {
        public string GenerateResolveLine(string instanceAlias, Type instanceType, int runnerId)
        {
            var generatedTypeName = new ResolvingCodeGenerator().GenerateTypeName(instanceType);
            return $"var {instanceAlias} = ({generatedTypeName})HostFields.Get({runnerId})[\"{instanceAlias}\"]";
        }

        public string GenerateUsingLine(Type type) => 
            $"using {type.Namespace};";
    }
}