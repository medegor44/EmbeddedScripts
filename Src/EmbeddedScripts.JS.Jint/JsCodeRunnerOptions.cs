using EmbeddedScripts.Shared;

namespace EmbeddedScripts.JS.Jint
{
    public class JsCodeRunnerOptions
    {
        public JsCodeRunnerOptions Register<T>(T obj, string alias)
        {
            Container.Register(obj, alias);
            return this;
        }

        public static JsCodeRunnerOptions Default => new();

        internal Container Container { get; } = new();
    }
}