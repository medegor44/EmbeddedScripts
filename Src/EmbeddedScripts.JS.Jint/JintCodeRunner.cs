using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Jint;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        public JintCodeRunner(string code)
            : this(code, _ => JsCodeRunnerOptions.Default)
        {
        }

        public JintCodeRunner(string code, Func<JsCodeRunnerOptions, JsCodeRunnerOptions> opts)
        {
            Code = code;

            RunnerOptions = opts(JsCodeRunnerOptions.Default);
        }

        public JintCodeRunner WithOptions(Func<JsCodeRunnerOptions, JsCodeRunnerOptions> opts)
        {
            RunnerOptions = opts(JsCodeRunnerOptions.Default);
            return this;
        }

        public JintCodeRunner AddOptions(Func<JsCodeRunnerOptions, JsCodeRunnerOptions> opts)
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
        private JsCodeRunnerOptions RunnerOptions { get; set; }
    }
}
