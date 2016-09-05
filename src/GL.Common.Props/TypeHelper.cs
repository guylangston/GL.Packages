using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GL.Common.Props
{
    public static class TypeHelper
    {

        public static IProperty GetProperty(this IReadOnlyCollection<IProperty> col, string propName)
        {
            return col?.FirstOrDefault(x => DefaultPropertyComparer(x.PropName, propName));
        }

        public static Func<string, string, bool> DefaultPropertyComparer { get; set; } = (a, b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        public static Type DefaultNumberType { get; set; } = typeof(decimal);

        public static Type Lookup(string typeString)
        {
            // short-cuts
            if (DefaultPropertyComparer(typeString, "string"))
            {
                return typeof(string);
            }
            if (DefaultPropertyComparer(typeString, "int"))
            {
                return typeof(int);
            }
            if (DefaultPropertyComparer(typeString, "bool"))
            {
                return typeof(bool);
            }
            if (DefaultPropertyComparer(typeString, "number"))
            {
                return DefaultNumberType;
            }
            if (DefaultPropertyComparer(typeString, "datetime"))
            {
                return typeof(DateTime);
            }

            return Type.GetType(typeString, false, true);
        }

        public static AtomicType GetAtomicType(this Type type)
        {
            // short-cuts
            if (type == typeof(string))
            {
                return AtomicType.String;
            }
            if (type == typeof(int))
            {
                return AtomicType.Integer;
            }
            if (type == typeof(bool))
            {
                return AtomicType.Bool;
            }
            if (type == typeof(decimal))
            {
                return AtomicType.Number;
            }
            if (type == typeof(DateTime))
            {
                return AtomicType.DateTime;
            }

            return AtomicType.Complex;
        }
    }

    public interface ICodeRenderer
    {
        void Render(TextWriter outp, IPropertyCollection props);
    }
}