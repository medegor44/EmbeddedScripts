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
        private readonly TaskFactory _factory = new(new SequentialTaskScheduler());

        public ChakraCoreRunner()
        {
            _context = _runtime.CreateContext();
            _mapper = new(_context);
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            return _factory.StartNew(() =>
            {
                var val = _context.Evaluate(expression);

                return new TypeMapper(_context).Map<T>(val);
            });
        }
        
        public Task RunAsync(string code)
        {
            return _factory.StartNew(() =>
            {
                _context.Evaluate(code);
            });
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            var task = _factory.StartNew(() =>
            {
                _context.GlobalObject.AddProperty(alias, _mapper.Map(obj));
            });

            try
            {
                // I used task.Wait() to preserve syntax consistency
                task.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions is null)
                    throw;
                
                foreach (var ex in e.InnerExceptions)
                    throw ex;
            }

            return this;
        }

        public void Dispose() =>
            _runtime?.Dispose();
    }
}
