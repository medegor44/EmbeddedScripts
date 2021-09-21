using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IEvaluator, IDisposable
    {
        private JsRuntime _runtime;
        private TypeMapper _mapper;
        private JsContext _context;
        private readonly ScriptDispatcher _dispatcher = new();

        private InterlockedStatedFlag _disposed;

        private void VerifyNotDisposed()
        {
            if (_disposed.IsSet())
                throw new ObjectDisposedException(ToString());
        }
        
        public ChakraCoreRunner()
        {
            _dispatcher.Invoke(() =>
            {
                _runtime = new();
                _context = _runtime.CreateContext();
                if (_context.IsValid)
                    _context.AddRef();
                _mapper = new(_context);
            });
        }

        public Task<T> EvaluateAsync<T>(string expression)
        { 
            VerifyNotDisposed();
            
            return Task.FromResult(_dispatcher.Invoke(() =>
            {
                var val = _context.Evaluate(expression);

                return _mapper.Map<T>(val);
            }));
        }

        public Task RunAsync(string code)
        {
            VerifyNotDisposed();
            
            _dispatcher.Invoke(() =>
            {
                _context.Evaluate(code);
            });
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            VerifyNotDisposed();

            _dispatcher.Invoke(() =>
            {
                var val = _mapper.Map(obj);
                
                val.AddRef();
                
                _context.GlobalObject.AddProperty(alias, val);
            });
            
            return this;
        }

        public void Dispose()
        {
            if (!_disposed.Set()) return;
            
            _dispatcher.Invoke(() =>
            {
                if (_context.IsValid)
                    _context.Release();

                _runtime?.Dispose();
            });
            
            _dispatcher.Dispose();
            _mapper.Dispose();
        }
    }
}
