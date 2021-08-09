using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task<ICodeRunner> RunAsync(string code);
        Task<object> EvaluateAsync(string expression);
        ICodeRunner Register<T>(T obj, string alias);
    }
}
