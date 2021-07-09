namespace EmbeddedScripts.Shared
{
    public class CodeRunnerOptions
    {
        public CodeRunnerOptions Register<T>(T obj, string alias)
        {
            Container.Register(obj, alias);
            return this;
        }

        public static CodeRunnerOptions Default => new();

        internal Container Container { get; } = new();
    }
}