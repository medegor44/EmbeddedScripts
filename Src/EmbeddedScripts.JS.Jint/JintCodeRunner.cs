using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Jint;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        public JintCodeRunner(string code)
            : this(code, _ => CodeRunnerOptions.Default)
        {
        }

        public JintCodeRunner(string code, Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            Code = code;
            RunnerOptions = opts(CodeRunnerOptions.Default);
        }

        public ICodeRunner WithOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            RunnerOptions = opts(CodeRunnerOptions.Default);
            return this;
        }

        public ICodeRunner AddOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            RunnerOptions = opts(RunnerOptions);
            return this;
        }

        public async Task RunAsync() =>
            await Task.Run(() => 
                new Engine()
                    .SetValuesFromContainer(RunnerOptions.Container)
                    .Execute(Code));

        private string Code { get; }
        private CodeRunnerOptions RunnerOptions { get; set; }
    }
}
