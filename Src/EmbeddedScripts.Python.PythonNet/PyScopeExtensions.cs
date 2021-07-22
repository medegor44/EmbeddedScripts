using EmbeddedScripts.Shared;
using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet
{
    public static class PyScopeExtensions
    {
        public static PyScope AddHostObjectsFromContainer(this PyScope scope, Container container)
        {
            foreach (var name in container.VariableAliases)
            {
                var pyObj = container.Resolve(name).ToPython();
                scope.Set(name, pyObj);
            }

            return scope;
        }
    }
}