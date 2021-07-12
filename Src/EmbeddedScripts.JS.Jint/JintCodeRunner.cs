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
            JintEngineOptions = optionsFunc(JintEngineOptions);
            return this;
        }

        public async Task RunAsync(string code) =>
            await Task.Run(() => 
                new Engine(JintEngineOptions)
                    .SetValuesFromContainer(RunnerConfig.Container)
                    .Execute(code));

        private CodeRunnerConfig RunnerConfig { get; set; }
        private Options JintEngineOptions { get; set; } = new();
    }
}
