using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IEvaluator, IDisposable
    {
        private readonly JsRuntime _runtime = new();
        private readonly TypeMapper _mapper;
        private readonly JsContext _context;

        private readonly object _lockObj = new();

        public ChakraCoreRunner()
        {
            _context = _runtime.CreateContext();
            _mapper = new(_context);
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            lock (_lockObj)
            {
                Console.WriteLine($"-- before evaluate {expression}");

                var val = _context.Evaluate(expression);
                
                Console.WriteLine($"-- after evaluate {expression}");

                return Task.FromResult( new TypeMapper(_context).Map<T>(val));
            }
        }

        public Task RunAsync(string code)
        {
            lock (_lockObj)
                _context.Evaluate(code);
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            lock (_lockObj)
                _context.GlobalObject.AddProperty(alias, _mapper.Map(obj));

            return this;
        }

        public void Dispose() =>
            _runtime?.Dispose();
    }
}
