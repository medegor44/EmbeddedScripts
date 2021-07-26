using System.Linq;
using System.Threading.Tasks;

using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class ChakraCoreRunner : ICodeRunner
    {
        private Container _container = new();

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
            using var runtime = new JsRuntime();
            var context = AddGlobals(runtime.CreateContext());
            
            context.Run(code);
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }
    }
}
