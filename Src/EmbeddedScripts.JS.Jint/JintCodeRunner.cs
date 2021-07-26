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

        public Task RunAsync(string code)
        {
            try
            {
                new Engine(_jintOptions)
                    .SetValuesFromContainer(_container)
                    .Execute(code);
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
            
            return Task.CompletedTask;
        }
    }
}
