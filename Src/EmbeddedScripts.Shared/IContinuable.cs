using System.Threading.Tasks;

namespace EmbeddedScripts.Shared
{
    public interface IContinuable
    {
        Task ContinueWithAsync(string code);
    }
}