using System;

namespace EmbeddedScripts.Shared.Exceptions
{
    public class ScriptSyntaxErrorException : Exception
    {
        public ScriptSyntaxErrorException(Exception innerException) 
            : this("Syntax error", innerException)
        {}
        
        public ScriptSyntaxErrorException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}