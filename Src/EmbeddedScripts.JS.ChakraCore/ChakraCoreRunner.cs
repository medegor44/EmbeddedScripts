using System;
using System.Linq;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IEvaluator, IDisposable
    {
        private readonly Container _container = new();
        private JsContext _context;
        private readonly JsRuntime _runtime = new();
        
        private JsContext AddGlobals(JsContext context)
        {
            var mapper = new TypeMapper(context);
            _container.VariableAliases
                .Aggregate(context.GlobalObject, (globalObj, alias) =>
                    globalObj.AddProperty(alias, mapper.Map(_container.Resolve(alias))));

            return context;
        }
        
        public Task<T> EvaluateAsync<T>(string expression)
        {
            _context = AddGlobals(_context ?? _runtime.CreateContext());
            var val = _context.Evaluate(expression);

            return Task.FromResult((T) new TypeMapper(_context).Map(val));
        }
        
        public Task<object> EvaluateAsync(string expression) 
            => EvaluateAsync<object>(expression);
        
        public Task<ICodeRunner> RunAsync(string code)
        {
            EvaluateAsync(code);
            
            return Task.FromResult(this as ICodeRunner);
        }
        
        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            
            return this;
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
