using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface IEvaluator
    {
        Task<object> EvaluateAsync(string expression);
        Task<T> EvaluateAsync<T>(string expression);
    }
}