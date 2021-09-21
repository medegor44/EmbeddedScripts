using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class TypeMapper : IDisposable
    {
        private readonly JsContext _context;
        
        // stores registered delegates to prevent their garbage collection
        private readonly List<JavaScriptNativeFunction> _jsNativeFunctions = new();
        private readonly object _listSynchronizer = new();

        public TypeMapper(JsContext context)
        {
            _context = context;
        }
        
        private JavaScriptValue MapClrPrimitivesToJs(object value)
        {
            if (value is null)
                return JavaScriptValue.Null;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.Int32:
                    return JavaScriptValue.FromInt32(Convert.ToInt32(value));
                
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return JavaScriptValue.FromDouble(Convert.ToDouble(value));

                case TypeCode.Boolean:
                    return JavaScriptValue.FromBoolean((bool) value);

                case TypeCode.String:
                    return JavaScriptValue.FromString((string) value);
                
                default:
                    throw new ArgumentException("Type is not supported");
            }
        }
        
        private object ToNumber(JavaScriptValue value)
        {
            var num = value.ToDouble();

            if (Math.Abs(num - Math.Round(num)) < double.Epsilon)
                return (int) num;
            
            return num;
        }

        private object MapJsPrimitivesToClr(JavaScriptValue value) =>
            value.ValueType switch
            {
                JavaScriptValueType.String => value.ToString(),
                JavaScriptValueType.Boolean => value.ToBoolean(),
                JavaScriptValueType.Number => ToNumber(value),
                JavaScriptValueType.Null => null,
                JavaScriptValueType.Undefined => null,
                _ => throw new ArgumentException("Type is not supported")
            };

        private JavaScriptValue MapDelegate(Delegate func)
        {
            var cb = new JavaScriptNativeFunction((_, _, args, _, _) =>
            {
                try
                {
                    if (func.Method.GetParameters().Length == args.Length - 1)
                    {
                        var res = func.DynamicInvoke(args.Skip(1).Select(MapJsPrimitivesToClr).ToArray());

                        return Map(res);
                    }

                    JavaScriptContext.SetException(
                        JavaScriptValue.CreateError(JavaScriptValue.FromString("Inappropriate args list")));
                    return JavaScriptValue.Undefined;
                }
                catch (TargetInvocationException e) when (e.InnerException != null)
                {
                    _context.CallbackException = e.InnerException;

                    JavaScriptContext.SetException(
                        new JsError(_context, ErrorCodes.HostError, e.InnerException.Message));
                }
                catch (ArgumentException e)
                {
                    JavaScriptContext.SetException(
                        JavaScriptValue.CreateError(JavaScriptValue.FromString(e.Message)));
                }

                return JavaScriptValue.Undefined;
            });

            lock (_listSynchronizer)
                _jsNativeFunctions.Add(cb);

            var f = JavaScriptValue.CreateFunction(cb, IntPtr.Zero);

            return f;
        }

        private object Map(JsValue value)
        {           
            object t;
            using (_context.Scope)
                t = MapJsPrimitivesToClr(value);

            return t;
        }
        
        public T Map<T>(JsValue value) => 
            (T)Map(value);
        
        public JsValue Map(object value)
        {
            using (_context.Scope)
            {
                if (value is Delegate func)
                    return new(_context, MapDelegate(func));

                return new(_context, MapClrPrimitivesToJs(value));
            }
        }

        public void Dispose()
        {
            _jsNativeFunctions.Clear();
        }
    }
}