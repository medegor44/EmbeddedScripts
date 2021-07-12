using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Jint;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        public JintCodeRunner()
            : this(_ => CodeRunnerConfig.Default)
        {
        }

        public JintCodeRunner(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(CodeRunnerConfig.Default);
        }

        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
            return this;
        }

        public JintCodeRunner AddEngineOptions(Func<Options, Options> optionsFunc)
        {
            JintOptions = optionsFunc(JintOptions);
            return this;
        }

        public async Task RunAsync(string code) =>
            await Task.Run(() => 
                new Engine(JintOptions)
                    .SetValuesFromContainer(RunnerConfig.Container)
                    .Execute(code));

        private CodeRunnerConfig RunnerConfig { get; set; }
        private Options JintOptions { get; set; } = new();
    }
}
