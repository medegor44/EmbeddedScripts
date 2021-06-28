using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Shared
{
    public class CSharpCodeRunnerOptions
    {
        public CSharpCodeRunnerOptions Register<T>(T obj, string alias)
        {
            Container.Register(obj, alias);
            return this;
        }

        public static CSharpCodeRunnerOptions Default => new();

        internal Container Container { get; } = new();
    }
}