using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface IEvaluator
    {
        Task<T> EvaluateAsync<T>(string expression);
    }
}