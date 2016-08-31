using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GL.FluentDAL
{

    public interface IValueWriter
    {
        void Add<T>(string field, T value);
    }

    public class TypedDataStatement<TTarget>
    {
        public TypedDataStatement(TTarget target, IValueWriter valueWriter)
        {
            this.target = target;
            this.valueWriter = valueWriter;
        }

        private readonly TTarget target;
        private readonly IValueWriter valueWriter;

        public TypedDataStatement<TTarget> Map<TRes>(Expression<Func<TTarget, TRes>>  lambda)
        {
            var l = lambda.Compile();
            valueWriter.Add(lambda.ToString(), l(target));
            return this;
        }
    }

    public class DataStatement : IValueWriter
    {
        protected class Item
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public Type Type { get; set; }
        }

        protected List<Item> values = new List<Item>();


        public void Add<T>(string field, T value)
        {
            values.Add(new Item()
                           {
                               Name = field,
                               Value = value,
                               Type = typeof(T)
                           });
        }

        protected object AdaptValue(object value)
        {
            return DBHelper2.AdaptForDB(value);

        }
    }
}