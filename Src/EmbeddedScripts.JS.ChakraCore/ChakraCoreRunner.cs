using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IEvaluator, IDisposable
    {
        private readonly JsRuntime _runtime = new();
        private TypeMapper _mapper;
        private JsContext _context;
        private readonly ScriptDispatcher _dispatcher = new();

        public ChakraCoreRunner()
        {
            _dispatcher.Invoke(() =>
            {
                _context = _runtime.CreateContext();
                _context.AddRef();
                _mapper = new(_context);
            });
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            return Task.FromResult(_dispatcher.Invoke(() =>
            {
                //Console.WriteLine($"start evaluate {expression}");
                var val = _context.Evaluate(expression);
                //Console.WriteLine($"end evaluate {expression}");

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
                _context.GlobalObject.AddProperty(alias, _mapper.Map(obj));
            });
            
            return this;
        }

        public void Dispose()
        {
            //Console.WriteLine("in dispose");
            _dispatcher.Dispose();
            //Console.WriteLine("_dispatcher disposed");


            if (_context.IsValid)
                _context.Release();
            
            //Console.WriteLine("context released");
            
            _runtime?.Dispose();
            //Console.WriteLine("_runtime disposed");


            GC.SuppressFinalize(true);
            //Console.WriteLine("in dispose done");
        }
    }
}
