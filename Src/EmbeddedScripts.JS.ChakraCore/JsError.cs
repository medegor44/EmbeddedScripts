using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    internal class JsError
    {
        private readonly JsValue _value;
        
        internal JsError(JsContext context, string errorName, string message)
        {
            _value = new JsValue(context);
            _value.AddProperty("name", new JsValue(context, JavaScriptValue.FromString(errorName)));
            _value.AddProperty("message", new JsValue(context, JavaScriptValue.FromString(message)));
        }
        
        public static implicit operator JavaScriptValue(JsError error) => 
            error._value;
    }
}