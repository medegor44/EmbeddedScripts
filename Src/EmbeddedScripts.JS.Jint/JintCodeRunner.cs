using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Jint;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        public JintCodeRunner(string code)
            : this(code, _ => CodeRunnerConfig.Default)
        {
        }

        public JintCodeRunner(string code, Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            Code = code;
            RunnerConfig = configFunc(CodeRunnerConfig.Default);
        }

        public ICodeRunner WithConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(CodeRunnerConfig.Default);
            return this;
        }

        public JintCodeRunner WithEngineOptions(Func<Options, Options> optionsFunc)
        {
            JintEngineOptions = optionsFunc(new Options());
            return this;
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

        public async Task RunAsync() =>
            await Task.Run(() => 
                new Engine(JintEngineOptions)
                    .SetValuesFromContainer(RunnerConfig.Container)
                    .Execute(Code));

        private string Code { get; }
        private CodeRunnerConfig RunnerConfig { get; set; }
        private Options JintEngineOptions { get; set; } = new();
    }
}
