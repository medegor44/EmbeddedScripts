using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Esprima;
using Jint;
using Jint.Runtime;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        private Container _container = new();
        private Options _jintOptions = new();
        private Engine _engine;

        public JintCodeRunner AddEngineOptions(Func<Options, Options> optionsFunc)
        {
            _jintOptions = optionsFunc(_jintOptions);
            return this;
        }
        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }

        public Task<ICodeRunner> RunAsync(string code)
        {
            _engine ??= new Engine(_jintOptions);
            
            try
            {
                _engine.SetValuesFromContainer(_container);
                _engine.Execute(code);
            }
            catch (JavaScriptException e)
            {
                throw new ScriptRuntimeErrorException(e);
            }
            catch (ParserException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }
            catch (Exception e) when (e is 
                MemoryLimitExceededException or 
                ExecutionCanceledException or
                RecursionDepthOverflowException or 
                JintException or 
                StatementsCountOverflowException)
            {
                throw new ScriptEngineErrorException(e);
            }

            return Task.FromResult(this as ICodeRunner);
        }
    }
}
