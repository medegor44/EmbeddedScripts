using System;
using System.Reflection;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    internal class InstanceWrapper
    {
        public InstanceWrapper(object instance, Type type)
        {
            Instance = instance;
            InstanceType = type;
        }

        public void InvokeMethod(string methodName)
        {
            try
            {
                InstanceType.InvokeMember(
                    methodName,
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    Instance,
                    new object[] { }
                );
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        private Type InstanceType { get; }
        private object Instance { get; }
    }
}