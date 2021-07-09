namespace EmbeddedScripts.Shared
{
    public class CodeRunnerConfig
    {
        public CodeRunnerConfig Register<T>(T obj, string alias)
        {
            Container.Register(obj, alias);
            return this;
        }

        public static CodeRunnerConfig Default => new();

        internal Container Container { get; } = new();
    }
}