using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface ICodeRunner
    {
        Task<ICodeRunner> RunAsync(string code);
        
        ICodeRunner Register<T>(T obj, string alias);
    }
}
