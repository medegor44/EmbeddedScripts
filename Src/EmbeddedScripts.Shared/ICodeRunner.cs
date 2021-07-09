using System;
using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task RunAsync();
        public ICodeRunner WithConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc);
        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc);
    }
}
