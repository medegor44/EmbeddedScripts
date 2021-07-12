using System;
using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task RunAsync(string code);
        ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc);
    }
}
