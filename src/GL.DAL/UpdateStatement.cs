using System;
using System.Data;
using System.Data.SqlClient;


namespace GL.FluentDAL
{
    public class UpdateStatement : DataStatement
    {
        private string table;
        private string where;


        public UpdateStatement Table(string table, string where)
        {
            this.where = where;
            this.table = table;
            return this;
        }



        public new UpdateStatement Add<T>(string field, T value)
        {
            base.Add(field, value);
            return this;
        }

        public override string ToString()
        {
            bool fat = values.Count > 10;
            var template = "UPDATE [{0}] SET {1} WHERE {2};";
            if (fat) template = "UPDATE [{0}]{3}SET {1} WHERE {2};";
            return string.Format(template,
                                 table,
                                 SQLTextHelper.ToStringConcat(values, x => string.Format("[{0}]={1}", x.Name, DBHelper2.Escape(x.Value)), ",", false, 80),
                                 where,
                                 Environment.NewLine
                );

        }

        public void SetupCommand(SqlCommand com)
        {

            com.CommandText = string.Format("UPDATE [{0}] SET {1} WHERE {2};",
                                            table,
                                            SQLTextHelper.ToStringConcat(values, x => string.Format("[{0}]=@{0}", x.Name), ","),
                                            where
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


        public int Execute(string db)
        {
            return DBHelper.ExecuteQuerySingle(db, r => r.GetInt32(0), ToString() + " SELECT @@ROWCOUNT");
        }
    }
}