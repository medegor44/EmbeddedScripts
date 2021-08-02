using System;

namespace EmbeddedScripts.Shared.Exceptions
{
    public class ScriptEngineErrorException : Exception
    {
        public ScriptEngineErrorException(Exception innerException) 
            : this("Engine error", innerException)
        {}
        
        public ScriptEngineErrorException(string message, Exception innerException = null) 
            : base(message, innerException)
        {
        }
    }
}