using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GL.FluentDAL
{
    public class QueryContext : BaseContext
    {
        public string Sql { get; set; }
        public List<object> Args { get; set; }
        public Dictionary<string, object> NamedArgs { get; set; }       // TODO: Not implemented yet

        public QueryContext Param<TObj>(string name, TObj val)
        {
            if (NamedArgs == null) NamedArgs = new Dictionary<string, object>();
            if (Args== null) Args = new List<object>();
            Args.Add(val);
            NamedArgs.Add(name, val);
            return this;
        }

        public ManualQueryBindContext BindWithOutReturn(Action<IDataReader> bindWith)
        {
            return new ManualQueryBindContext()
            {
                BindFunc = bindWith,
                Query = this
            };
        }

        public QueryBindContext<TResult> BindByIndex<TResult>(Func<IDataReader, QueryBindContext<TResult>, TResult> func)
        {
            return new QueryBindContext<TResult>()
            {
                BindFunc = func,
                Query = this
            };
        }

        public QueryBindContext<TResult> BindByName<TResult>(Func<IDataReader, FieldLookup, QueryBindContext<TResult>, TResult> func)
        {
            FieldLookup lookup = null;
            return new QueryBindContext<TResult>()
            {
                BindFunc = (r, c) =>
                {
                    if (lookup == null)
                    {
                        lookup = DBHelper.GetColumnNames(r);
                    }
                    return func(r, lookup, c);
                },
                Query = this
            };
        }

        public QueryBindContext<TResult> Bind<TResult>()
        {
            return new QueryBindContext<TResult>()
            {
                Query = this
            };
        }
    }

    // This is passed into the mapper functions to allow for stateful looksups (caching, master data, etc)
    public class QueryBindContext
    {
        public QueryContext Query { get; set; }
    }

    public class ManualQueryBindContext : QueryBindContext
    {
        public Action<IDataReader> BindFunc { get; set; }

        public void Execute()
        {
            DBHelper.ExecuteQueryNoReturn(Query.Context.ConnectionString, BindFunc, Query.Sql, Query.Args?.ToArray());
        }
    }

    public class QueryBindContext<TResult> : QueryBindContext
    {
        public Func<IDataReader, QueryBindContext<TResult>, TResult> BindFunc { get; set; }

        public ImmutableList<TResult> ToList()
        {
            return DBHelper.ExecuteQuery<TResult>(Query.Context.ConnectionString, r=>BindFunc(r, this), Query.Sql, Query.Args).ToImmutableList();
        }

        public List<TResult> ToMutableList()
        {
            return DBHelper.ExecuteQuery<TResult>(Query.Context.ConnectionString, r => BindFunc(r, this), Query.Sql, Query.Args).ToList();
        }

        public TResult FirstOrDefault()
        {
            return DBHelper.ExecuteQuerySingle<TResult>(Query.Context.ConnectionString, r => BindFunc(r, this), Query.Sql, Query.Args);
        }

        public Task<List<TResult>> ToMutableListAsync()
        {
            return Task.Run( () => ToMutableList());
        }
    }
}