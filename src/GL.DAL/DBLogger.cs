using System;
using System.Data;

namespace GL.DAL
{
    public interface IDBLogger
    {
        bool IsEnabled { get; set; }
        void Log(object context, string connection, string sql);
        void Log(DBLoggerItem item);
    }

    public class DBLoggerItem
    {
        public DBLoggerItem()
        {
            Count = 0;
        }

        public object Context { get; set; }
        public string Connection { get; set; }
        public string SQL { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("Context: {0}, Connection: {1}, SQL: {2}, Count: {3}", Context, Connection, SQL, Count);
        }
    }

    public class DBLoggerItemADO : DBLoggerItem
    {
        public IDbConnection SqlConnection { get; set; }
        public IDbCommand SqlCommand { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, Exception: {1}", base.ToString(), Exception);
        }
    }

    public class DBLoggerConsole : IDBLogger
    {
        public DBLoggerConsole()
        {
            IsEnabled = true;
            seq = 0;
        }

        public bool IsEnabled { get; set; }
        private int seq;

        public void Log(object context, string connection, string sql)
        {
            if (!IsEnabled) return;

            var lSql = sql;
            if (lSql != null)
            {
                if (lSql.Contains("\n"))
                {
                    lSql = "\n~\t" + lSql.Replace("\n", "\n~\t");    
                }
            }
            Console.WriteLine("[~SQL~:{4}] {0} on {1} => ({2}){3}", context, connection, null, lSql, seq++);
        }

        public void Log(DBLoggerItem item)
        {
            if (!IsEnabled) return;
            if (item == null) return;

            var lSql = item.SQL;
            if (lSql != null)
            {
                if (lSql.Contains("\n"))
                {
                    lSql = "\n~\t" + lSql.Replace("\n", "\n~\t");
                }
            }
            Console.WriteLine("[~SQL~:{4}] {0} on {1} => ({2}){3}", item.Context, item.Connection, item.Count, lSql, seq++);
        }
    }

}