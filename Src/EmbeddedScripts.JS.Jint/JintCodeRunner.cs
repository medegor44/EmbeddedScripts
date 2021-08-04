﻿using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Esprima;
using Jint;
using Jint.Runtime;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner, IContinuable
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

        private void ExecuteWithExceptionHandling(Engine engine, string code)
        {
            try
            {
                engine.Execute(code);
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
        }

        public Task RunAsync(string code)
        {
            _engine = new Engine(_jintOptions)
                .SetValuesFromContainer(_container);
            
            ExecuteWithExceptionHandling(_engine, code);
            
            return Task.CompletedTask;
        }

        public Task ContinueWithAsync(string code)
        {
            _engine.SetValuesFromContainer(_container);
            
            ExecuteWithExceptionHandling(_engine, code);
            
            return Task.CompletedTask;
        }
    }
}
