using System;

namespace EmbeddedScripts.JS.Common.Tests
{
    public class DummyException : Exception
    {
        public DummyException(string message) : base(message)
        {
        }
    }
}