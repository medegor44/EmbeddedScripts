using System;
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
            return Task.FromResult(_dispatcher.Invoke(() =>
            {
                var val = _context.Evaluate(expression);

                return _mapper.Map<T>(val);
            }));
        }

        public Task RunAsync(string code)
        {
            _dispatcher.Invoke(() =>
            {
                _context.Evaluate(code);
            });
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
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
