﻿using System;
using System.Linq;
using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class TypeMapper
    {
        private JsContext _context;

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
                _ => throw new ArgumentException("Unsupported type")
            };

        private JavaScriptValue MapDelegate(Delegate func) => 
            JavaScriptValue.CreateFunction((_, _, args, _, _) =>
                {
                    var val = JavaScriptValue.Undefined;
                    try
                    {
                        var returnValue =
                            func.DynamicInvoke(args.Skip(1).Select(MapJsPrimitivesToClr).ToArray());
                        val = Map(returnValue);
                    }
                    catch (Exception e)
                    {
                        JavaScriptContext
                            .SetException(
                                JavaScriptValue.CreateError(JavaScriptValue.FromString(e.Message)));
                    }

                    return val;
                },
                IntPtr.Zero);
        
        public JsValue Map(object value)
        {
            using (_context.Scope)
            {
                if (value is Delegate func)
                    return new(_context, MapDelegate(func));
                return new(_context, MapClrPrimitivesToJs(value));
            }
        }
    }
}