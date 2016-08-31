using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace GL.DAL
{
    public class InsertStatement : DataStatement
    {
        public InsertStatement()
        {
            DebugMode = true;
            SelectScopeIdentity = true;
        }

        private string table;

        public InsertStatement Into(string table)
        {
            this.table = table;
            return this;
        }

        public new InsertStatement Add<T>(string field, T value)
        {
            base.Add(field, value);
            return this;
        }

        public bool DebugMode { get; set; }
        public bool SelectScopeIdentity { get; set; }

        public InsertStatement WithNoScopeIdentity()
        {
            SelectScopeIdentity = false;
            return this;
        }

        public override string ToString()
        {
            var res = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2}); {3}",
                                 table,
                                 SQLTextHelper.ToStringConcat(values, x => "[" + x.Name + "]", ",", false, 80),
                                 SQLTextHelper.ToStringConcat(values, x => DBHelper2.Escape(x.Value), ",", false, 80),
                                 SelectScopeIdentity ? "SELECT CAST(SCOPE_IDENTITY() AS INT)" : null
                                 );
            if (DebugMode)
            {
                var sb = new StringBuilder();
                sb.Append(res);
                sb.AppendLine();
                sb.Append("/*");
                sb.Append(SQLTextHelper.ToStringConcat(values, x => 
                    string.Format("{0}={1}", x.Name, DBHelper2.Escape(x.Value)).Replace("/*", "").Replace("*/", ""), 
                    ", ", false, 80));
                sb.Append("*/");
                return sb.ToString();
            }
            return res;

        }

        public int Execute(string db)
        {
            return OldHelper.ExecuteQuerySingle(db, r => r.GetInt32(0), ToString());
        }

        public void ExecuteNoScopeId(string db)
        {
            OldHelper.ExecuteCommand(db, ToString());
        }

        public void SetupCommand(SqlCommand com)
        {

            com.CommandText = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2}); SELECT SCOPE_IDENTITY()",
                                            table,
                                            SQLTextHelper.ToStringConcat(values, x => "[" + x.Name + "]", ","),
                                            SQLTextHelper.ToStringConcat(values, x => "@" + x.Name, ",")
                );

            foreach (var item in values)
            {

                var p = new SqlParameter(item.Name, base.AdaptValue(item.Value) ?? DBNull.Value);
                if (item.Type == typeof(byte[]))
                {
                    p.DbType = DbType.Binary;
                }
                com.Parameters.Add(p);
            }


        }

    }
}