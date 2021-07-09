using System;
using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task RunAsync();
        public ICodeRunner WithOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts);
        public ICodeRunner AddOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts);
    }
}
