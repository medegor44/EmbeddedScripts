﻿using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Scripting.CodeGeneration;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting
{
    public class ScriptCodeRunner : ICodeRunner
    {
        private Container _container = new();
        private ScriptOptions _roslynOptions = ScriptOptions.Default;

        public ScriptCodeRunner AddEngineOptions(Func<ScriptOptions, ScriptOptions> optionsFunc)
        {
            _roslynOptions = optionsFunc(_roslynOptions);
            return this;
        }
        
        public async Task RunAsync(string code)
        {
            try
            {
                await CSharpScript.RunAsync(
                    GenerateScriptCode(code),
                    BuildEngineOptions(),
                    new Globals {Container = _container});
            }
            catch (CompilationErrorException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, _container);

        private ScriptOptions BuildEngineOptions() =>
            _roslynOptions.WithReferencesFromContainer(_container);
    }
}
