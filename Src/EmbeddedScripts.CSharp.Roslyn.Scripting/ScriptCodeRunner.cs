using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Scripting.CodeGeneration;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting
{
    public class ScriptCodeRunner : ICodeRunner, IEvaluator
    {
        private readonly Globals _globals = new();
        private ScriptOptions _roslynOptions = ScriptOptions.Default;
        private ScriptState _scriptState;
        
        public async Task<T> EvaluateAsync<T>(string expression)
        {
            _scriptState ??= await CSharpScript.RunAsync<T>("", BuildEngineOptions(),
                _globals);
            
            try
            {
                var state = await _scriptState.ContinueWithAsync<T>(GenerateScriptCode(expression), BuildEngineOptions());
                _scriptState = state;
                
                return state.ReturnValue;
            }
            catch (CompilationErrorException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }

        }
        
        public async Task<ICodeRunner> RunAsync(string code)
        {
            await EvaluateAsync<object>(code);
            
            return this;
        }
        
        public ScriptCodeRunner AddEngineOptions(Func<ScriptOptions, ScriptOptions> optionsFunc)
        {
            _roslynOptions = optionsFunc(_roslynOptions);
            
            return this;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _globals.Container.Register(obj, alias);
            
            return this;
        }

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, _globals.Container);

        private ScriptOptions BuildEngineOptions() =>
            _roslynOptions.AddReferencesFromContainer(_globals.Container);
    }
}
