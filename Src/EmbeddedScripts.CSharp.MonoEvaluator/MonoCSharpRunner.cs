using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.MonoEvaluator.CodeGeneration;
using EmbeddedScripts.CSharp.MonoEvaluator.Hosting;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Mono.CSharp;

namespace EmbeddedScripts.CSharp.MonoEvaluator
{
    public class MonoCSharpRunner : ICodeRunner, IEvaluator
    {
        private static int _nextRunnerId;
        private readonly StringBuilder _errorLogBuilder = new();
        private readonly Evaluator _evaluator;
        private readonly MonoSpecificCodeGenerator _codeGenerator = new();
        private readonly int _runnerId;

        /// <summary>
        /// Evaluator doesn't throw any syntax exceptions. Instead, it just logged errors into StreamReportPrinter. So we using
        /// StringWriter and after each run check if any error is logged or not. If error is logged method throws an exception
        /// with logged message. 
        /// </summary>
        /// <exception cref="ScriptSyntaxErrorException"></exception>
        private void ThrowIfErrorLogged()
        {
            var message = _errorLogBuilder.ToString();

            if (string.IsNullOrEmpty(message))
                return;

            _errorLogBuilder.Clear();

            throw new ScriptSyntaxErrorException(message);
        }

        public MonoCSharpRunner()
        {
            var defaultSettings = new CompilerSettings();
            _evaluator = new Evaluator(new CompilerContext(
                defaultSettings, new StreamReportPrinter(new StringWriter(_errorLogBuilder))));

            _runnerId = Interlocked.Increment(ref _nextRunnerId);

            HostFields.CreateNewEntry(_runnerId);
            var hostFieldsType = typeof(HostFields);
            _evaluator.ReferenceAssembly(hostFieldsType.Assembly);

            RunAsync(_codeGenerator.GenerateUsingLine(hostFieldsType));
        }

        public Task RunAsync(string code)
        {
            _evaluator.Run(code);

            ThrowIfErrorLogged();

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T instance, string alias)
        {
            var instanceType = typeof(T);
            
            HostFields.Get(_runnerId).Add(alias, instance);

            _evaluator.TryReferenceAssembly(instanceType.Assembly);
            
            var resolveLine =_codeGenerator.GenerateResolveLine(alias, instanceType, _runnerId);
            RunAsync(resolveLine);

            return this;
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            var evaluationComplete = _evaluator.Evaluate(expression, out var result, out var resultSet);

            ThrowIfErrorLogged();
            
            if (evaluationComplete != null)
                throw new ScriptSyntaxErrorException("Given expression is partial");
            if (!resultSet)
                throw new ScriptEngineErrorException("Given expression is statement");

            return Task.FromResult((T)result);
        }
    }
}