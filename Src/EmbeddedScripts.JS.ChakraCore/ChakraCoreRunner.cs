using System;
using System.Linq;
using System.Threading.Tasks;

using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IDisposable
    {
        private Container _container = new();
        private JsContext _context;
        private JsRuntime _runtime = new();
        
        public Task<object> EvaluateAsync(string expression)
        {
            throw new NotImplementedException();
        }
        
        private JsContext AddGlobals(JsContext context)
        {
            var mapper = new TypeMapper(context);
            _container.VariableAliases
                .Aggregate(context.GlobalObject, (globalObj, alias) =>
                    globalObj.AddProperty(alias, mapper.Map(_container.Resolve(alias))));

            return context;
        }
        
        public Task<ICodeRunner> RunAsync(string code)
        {
            _context = AddGlobals(_context ?? _runtime.CreateContext());
            _context.Run(code);
            
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
