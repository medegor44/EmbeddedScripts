using System;
using System.Linq;
using System.Threading.Tasks;

using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner, IContinuable, IDisposable
    {
        private Container _container = new();
        private JsContext _context;
        private JsRuntime _runtime = new();
        
        private JsContext AddGlobals(JsContext context)
        {
            var mapper = new TypeMapper(context);
            _container.VariableAliases
                .Aggregate(context.GlobalObject, (globalObj, alias) =>
                    globalObj.AddProperty(alias, mapper.Map(_container.Resolve(alias))));

            return context;
        }
        
        public Task RunAsync(string code)
        {
            _context = AddGlobals(_runtime.CreateContext());
            _context.Run(code);
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }

        public Task ContinueWithAsync(string code)
        {
            _context = AddGlobals(_context);
            _context.Run(code);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
