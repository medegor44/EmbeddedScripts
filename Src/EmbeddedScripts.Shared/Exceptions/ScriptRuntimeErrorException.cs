using System;

namespace EmbeddedScripts.Shared.Exceptions
{
    public class ScriptRuntimeErrorException : Exception
    {
        public ScriptRuntimeErrorException(Exception innerException) 
            : this("Runtime error", innerException)
        {}
        
        public ScriptRuntimeErrorException(string message, Exception innerException = null) 
            : base(message, innerException)
        {
        }
    }
}