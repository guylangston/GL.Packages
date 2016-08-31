using System;
using System.Linq;

namespace GL.DAL
{
    public class DatabaseContextDefault : IDatabaseContext
    {
        public DatabaseContextDefault(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public QueryContext Query(string sql, params object[] args)
        {
            return new QueryContext()
            {
                Context = this,
                Sql = sql,
                Args = args?.ToList()
            };
        }

        public InsertContext Insert(string table)
        {
            return new InsertContext()
            {
                Statement = new InsertStatement().Into(table) 
            };
        }

        public UpdateContext Update(string table, string where)
        {
            return new UpdateContext()
            {
                Statement = new UpdateStatement().Table(table, where)
            };
        }
    }
}