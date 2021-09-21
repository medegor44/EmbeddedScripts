using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class JsValue
    {
        protected JavaScriptValue _innerValue;
        protected JsContext _context;

        internal JsValue(JsContext context)
        {
            _context = context;
            using (_context.Scope)
                _innerValue = JavaScriptValue.CreateObject();
        }

        internal JsValue(JsContext context, JavaScriptValue value)
        {
            _context = context;
            _innerValue = value;
        }

        public void AddRef()
        {
            using (_context.Scope)
                _innerValue.AddRef();
        }

        public void Release()
        {
            using (_context.Scope)
                _innerValue.Release();
        }

        public JsValue AddProperty(string name, JsValue value)
        {
            using (_context.Scope)
                _innerValue.SetProperty(JavaScriptPropertyId.FromString(name), value, true);

            return this;
        }

        public JsValue GetProperty(string name)
        {
            using (_context.Scope)
                return new JsValue(_context, _innerValue.GetProperty(JavaScriptPropertyId.FromString(name)));
        }

        public override string ToString() =>
            _innerValue.ConvertToString().ToString();

        public static implicit operator JavaScriptValue(JsValue value) =>
            value._innerValue;
    }
}