using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GL.Common.Props
{
    public interface IProperty
    {
        string PropName { get; }
        Type PropType { get; }
    }

    public class PropertyInfoAdapter : IProperty
    {
        readonly PropertyInfo prop;

        public PropertyInfoAdapter(PropertyInfo prop)
        {
            this.prop = prop;
        }

        public string PropName => prop.Name;
        public Type PropType => prop.PropertyType;
    }

    public interface IPropertyCollection : IReadOnlyCollection<IProperty>
    {
        string TypeName { get; }
        Type Type { get; }
    }


    public enum AtomicType
    {
        Integer,
        String,
        Bool,
        Number,
        DateTime,

        Complex
    }
}
