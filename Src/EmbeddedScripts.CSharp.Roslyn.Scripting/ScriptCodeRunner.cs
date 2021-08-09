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
        private readonly Container _container = new();
        private ScriptOptions _roslynOptions = ScriptOptions.Default;
        private ScriptState _scriptState;
        
        public async Task<T> EvaluateAsync<T>(string expression)
        {
            try
            {
                if (_scriptState is null)
                    _scriptState = await CSharpScript.RunAsync(GenerateScriptCode(expression), BuildEngineOptions(),
                        new Globals{ Container = _container });
                else
                    _scriptState = await _scriptState.ContinueWithAsync(GenerateScriptCode(expression));
                return (T) _scriptState.ReturnValue;
            }
            catch (CompilationErrorException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }
            
        }
        
        public async Task<object> EvaluateAsync(string expression)
        {
            await EvaluateAsync<object>(expression);
            
            return _scriptState.ReturnValue;
        }
        
        public async Task<ICodeRunner> RunAsync(string code)
        {
            await EvaluateAsync(code);
            
            return this;
        }
        
        public ScriptCodeRunner AddEngineOptions(Func<ScriptOptions, ScriptOptions> optionsFunc)
        {
            _roslynOptions = optionsFunc(_roslynOptions);
            
            return this;
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
            _roslynOptions.AddReferencesFromContainer(_container);
    }
}
