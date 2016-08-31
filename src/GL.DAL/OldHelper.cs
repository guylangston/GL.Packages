using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GL.DAL
{
    
    internal static class OldHelper
    {
        #region Init
        public static IDBLogger Logger { get; set; }
        public static Action<SqlCommand> SetupCommandAction { get; set; }

        public static void ConfirmLogger(Func<IDBLogger> factory)
        {
            if (Logger == null) Logger = factory();
        }

        private static void SetupCommand(SqlCommand command)
        {
            if (SetupCommandAction != null)
            {
                SetupCommandAction(command);
            }
        }

        #endregion Init

        #region Execute
        

        public static T ExecuteScalarCommand<T>(string connectionString, string sqlFormat, params object[] args)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var sqlText = SafeStringFormat(sqlFormat, Escape(args));

            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = con;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (var command = con.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        SetupCommand(command);
                        var id = command.ExecuteScalar();
                        if (logItem != null && Logger != null && Logger.IsEnabled) Logger.Log(logItem);
                        try
                        {
                            if (id == DBNull.Value || id == null) return default(T);
                            return (T) id;
                        }
                        catch (InvalidCastException)
                        {
                            if (id == null) throw;
                            throw new InvalidCastException(
                                $"Expected {typeof (T).FullName} but got {id.GetType().FullName}");
                        }
                    }
                }
            }
            catch (SqlException sql)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = sql;
                    Logger.Log(logItem);
                }
                throw HandleException(sql, connectionString, sqlText);
            }
        }

        public static void ExecuteCommand(string connectionString, string sqlFormat, params object[] args)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var sqlText = SafeStringFormat(sqlFormat, Escape(args));

            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = con;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (var command = con.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        SetupCommand(command);
                        command.ExecuteNonQuery();
                        if (logItem != null && Logger != null && Logger.IsEnabled) Logger.Log(logItem);
                    }
                }
            }
            catch (SqlException sql)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = sql;
                    Logger.Log(logItem);
                }
                throw HandleException(sql, connectionString, sqlText);
            }
        }

        public static void ExecuteTimedCommand(string connectionString, string sqlFormat, int commandTimeout,
            params object[] args)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var sqlText = SafeStringFormat(sqlFormat, Escape(args));

            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = con;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (var command = con.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        command.CommandTimeout = commandTimeout;
                        SetupCommand(command);
                        command.ExecuteNonQuery();
                        if (logItem != null && Logger != null && Logger.IsEnabled) Logger.Log(logItem);
                    }
                }
            }
            catch (SqlException sql)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = sql;
                    Logger.Log(logItem);
                }
                throw HandleException(sql, connectionString, sqlText);
            }
        }

        public static void ExecuteCommand(string connectionString, string sqlText,
            IEnumerable<SqlParameter> sqlParameter)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = con;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (var command = con.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;

                        foreach (var p in sqlParameter)
                        {
                            command.Parameters.Add(p);
                        }
                        SetupCommand(command);
                        command.ExecuteNonQuery();
                        if (logItem != null && Logger != null && Logger.IsEnabled) Logger.Log(logItem);
                    }
                }
            }
            catch (SqlException sql)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = sql;
                    Logger.Log(logItem);
                }
                throw HandleException(sql, connectionString, sqlText);
            }
        }

        public static List<T> ExecuteTimedQuery<T>(string connectionString, int commandTimeout,
            Func<SqlDataReader, T> readRow, string sql, params object[] sqlParams)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var sqlText = SafeStringFormat(sql, Escape(sqlParams));
            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = conn;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        command.CommandTimeout = commandTimeout;
                        SetupCommand(command);
                        using (var reader = command.ExecuteReader())
                        {
                            var result = new List<T>();
                            while (reader.Read())
                            {
                                result.Add(readRow(reader));
                            }
                            if (logItem != null && Logger != null && Logger.IsEnabled)
                            {
                                logItem.Count = result.Count;
                                Logger.Log(logItem);
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = ex;
                    Logger.Log(logItem);
                }
                throw HandleException(ex, connectionString, sqlText);
            }
        }


        public static List<T> ExecuteQuery<T>(string connectionString, Func<IDataReader, T> readRow, string sql,
            params object[] sqlParams)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var sqlText = SafeStringFormat(sql, Escape(sqlParams));
            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = conn;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        SetupCommand(command);

                        using (var reader = command.ExecuteReader())
                        {
                            var result = new List<T>();
                            while (reader.Read())
                            {
                                result.Add(readRow(reader));
                            }
                            if (logItem != null && Logger != null && Logger.IsEnabled)
                            {
                                logItem.Count = result.Count;
                                Logger.Log(logItem);
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = ex;
                    Logger.Log(logItem);
                }
                throw HandleException(ex, connectionString, sqlText);
            }
        }

        public static void ExecuteQueryNoReturn(string connectionString, Action<SqlDataReader> readRow, string sql,
            params object[] sqlParams)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var sqlText = SafeStringFormat(sql, Escape(sqlParams));
            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText,
                    Connection = connectionString
                };
            }
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = conn;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                        if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                        command.CommandText = sqlText;
                        SetupCommand(command);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                readRow(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = ex;
                    Logger.Log(logItem);
                }
                throw HandleException(ex, connectionString, sqlText);
            }
        }


        public static List<T> ExecuteQuery<T>(IDbConnection connection, Func<IDataReader, T> readRow, string sql,
            params object[] sqlParams)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var sqlText = SafeStringFormat(sql, Escape(sqlParams));

            DBLoggerItemADO logItem = null;
            if (Logger != null && Logger.IsEnabled)
            {
                logItem = new DBLoggerItemADO
                {
                    SQL = sqlText
                };
            }

            try
            {
                var requiresClose = false;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    requiresClose = true;
                }
                if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlConnection = connection;
                using (var command = connection.CreateCommand())
                {
                    if (logItem != null && Logger != null && Logger.IsEnabled) logItem.SqlCommand = command;
                    command.CommandText = sqlText;
                    SetupCommand(command);
                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<T>();
                        while (reader.Read())
                        {
                            result.Add(readRow(reader));
                        }
                        if (requiresClose)
                        {
                            connection.Close();
                        }
                        if (logItem != null && Logger != null && Logger.IsEnabled)
                        {
                            logItem.Count = result.Count;
                            Logger.Log(logItem);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                if (logItem != null && Logger != null && Logger.IsEnabled)
                {
                    logItem.Exception = ex;
                    Logger.Log(logItem);
                }
                throw HandleException(ex, connection.Database, sqlText);
            }
        }

      

        public static T ExecuteQuerySingle<T>(string connectionString, Func<IDataReader, T> readRow, string sql,
            params object[] sqlParams)
        {
            var data = ExecuteQuery(connectionString, readRow, sql, sqlParams);
            if (data == null) return default(T);
            return data.FirstOrDefault();
        }

        #endregion Execute


        public static DatabaseException HandleException(Exception sql, string conn, string sqlText)
        {
            var text = string.Format("{0}\n{1}\n{2}", sql.Message, conn, sqlText);

            return new DatabaseException(text, sql)
            {
                Connection = conn,
                SQL = sqlText
            };
        }


        private static void SetupCommand(IDbCommand command)
        {
            // TODO
        }

        public static string SafeStringFormat(string stringFormat, object[] args)
        {
            if (stringFormat == null) return null;
            if (args == null) return stringFormat;
            if (args.Length == 0) return stringFormat;
            return string.Format(stringFormat, args);
        }

        public static string FormatSQL(string sqlTemplate, params object[] args)
        {
            return SafeStringFormat(sqlTemplate, Escape(args));
        }

        public static string Make8601LongFormat(DateTime input)
        {
            return input.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
        }

        public static object[] Escape(object[] args)
        {
            if (args == null) return null;
            if (args.Length == 0) return args;

            var clone = new object[args.Length];
            for (var cc = 0; cc < args.Length; cc++)
            {
                clone[cc] = Escape(args[cc]);
            }
            return clone;
        }

        public static string Escape(object arg)
        {
            if (arg == null) return "NULL";

            arg = AdaptForDB(arg);


            if (arg is int)
            {
                return arg.ToString();
            }
            if (arg is double)
            {
                return arg.ToString();
            }

            if (arg is SqlText)
            {
                // Don't escape
                return arg.ToString();
            }
            if (arg is DateTime || arg is DateTime?)
            {
                var dt = (DateTime) arg;
                if (dt == DateTime.MinValue) return "NULL";
                //return dt.ToString("yyyyMMdd");
                return "'" + Make8601LongFormat((DateTime) arg) + "'";
            }
            if (arg is Enum)
            {
                return ((int) arg).ToString();
            }
            if (arg is bool)
            {
                return (bool) arg ? "1" : "0";
            }
            var toStr = arg.ToString();
            var escaped = toStr.Replace("'", "''");
            return "N'" + escaped + "'";
        }

        public static string EscapeStringNoQuotes(string raw)
        {
            if (raw == null) return null;
            return raw.Replace("'", "''");
        }


        public static InsertStatement INSERT(string table)
        {
            return new InsertStatement().Into(table);
        }

        public static UpdateStatement UPDATE(string table, string where)
        {
            return new UpdateStatement().Table(table, where);
        }

        public static UpdateStatement UPDATE(string table, string whereFieldName, object equalValue)
        {
            return new UpdateStatement().Table(table, $"[{whereFieldName}]={Escape(equalValue)}");
        }


        public static object AdaptForDB(object value)
        {
            if (value == null) return null;

            //var idbase = value as IdBase;
            //if (idbase != null)
            //{
            //    return idbase.Key;
            //}

            //var idcode = value as IdCodeBase;
            //if (idcode != null)
            //{
            //    return idcode.Code;
            //}

            return value;
        }

        public static object AdaptForCode(PropertyInfo prop, object inst, object data)
        {
            if (data == null) return null;
            //if (IdHelper.IsIdType(prop.PropertyType))
            //{
            //    return IdHelper.MakeId(prop.PropertyType, Convert.ToInt64(data));
            //}

            //if (IdHelper.IsIdCodeType(prop.PropertyType))
            //{
            //    return IdHelper.MakeIdCode(prop.PropertyType, data.ToString());
            //}

            return data;
        }

        public static FieldLookup GetColumnNames(IDataReader reader)
        {
            var res = new FieldLookup();
            for (var cc = 0; cc < reader.FieldCount; cc++)
                res.Add(reader.GetName(cc));
            return res;
        }

        public static bool SqlExceptionContains(Exception ex, string frag)
        {
            do
            {
                if (ex is SqlException)
                {
                    if (ex.Message.Contains(frag))
                    {
                        return true;
                    }
                }

                if (ex.InnerException == null)
                {
                    return false;
                }
                ex = ex.InnerException;
            } while (true);
        }

        public static long TableRowCount(string db, string tableName)
        {
            return ExecuteScalarCommand<long>(db, "SELECT Cast(Count(*) as bigint) AS Count FROM " + tableName);
        }

        public static string GetDatabaseName(string getConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(getConnectionString);
            return builder.InitialCatalog;
        }

        public static string EscapeLIKE(string filter)
        {
            filter = filter ?? string.Empty;
            return string.Format("N'%{0}%'", filter.Replace("\'", "\'\'"));
        }

        public static int? ReadInt32Null(IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;
            return reader.GetInt32(i);
        }

        public static DateTime? ReadDateTimeNull(IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;
            return reader.GetDateTime(i);
        }

        public static byte? ReadByteNull(IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;
            return reader.GetByte(i);
        }

        public static short? ReadShortNull(IDataReader reader, int i)
        {
            if (reader.IsDBNull(i)) return null;
            return reader.GetInt16(i);
        }

        public static KeyLookup BuildLookup(string db, string table, string f1, string f2)
        {
            var res = new KeyLookup();
            var sql = "SELECT {0}, {1} FROM {2}";
            ExecuteQueryNoReturn(db, r =>
            {
                var key = r.GetInt32(0);
                var code = r.GetString(1);
                res.CodeToId.Add(code, key);
                res.IdToCode.Add(key, code);
            },
                sql
                , new SqlText(f1), new SqlText(f2), new SqlText(table)
                );
            return res;
        }

        public static SqlText Encode_IN_ListClause<T>(IEnumerable<T> data, Func<T, object> selectProp)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                var v = selectProp(item);
                if (v != null)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(Escape(v));
                }
            }
            return SqlText.Create(sb.ToString());
        }


     
        public static class Utils
        {
            public static void BackupToFile(string dbConn, string dbName, string outFile)
            {
                var sql = string.Format(@"BACKUP DATABASE [{0}] TO  DISK = N'{1}' WITH NOFORMAT, INIT,  
NAME = N'CalvusWeb-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10", dbName, outFile);
                ExecuteCommand(dbConn, sql);
            }


            public static void ExecuteScriptFile(string db, string resourceFile)
            {
                var sql = File.ReadAllText(resourceFile);
                ExecuteCommand(db, sql);
            }


            public static string MakeConnectionString(string name, string machine = "localhost")
            {
                return string.Format("Data Source={1};Initial Catalog={0};Integrated Security=True",
                    name, machine);
            }
        }

    }


    public class FieldLookup : List<string>
    {
        private readonly Dictionary<string, int> indexes = new Dictionary<string, int>();

        public int this[string field]
        {
            get
            {
                if (!HasField(field))
                {
                    throw new Exception($"Field not found: {field}");
                };
                return indexes[field.ToLowerInvariant()];
            }
        }

        public bool HasField(string field)
        {
            return indexes.ContainsKey(field.ToLowerInvariant());
        }

        public new void Add(string field)
        {
            base.Add(field);
            if (!HasField(field)) // covers the case where a dataset has more than one instance of this col name
            {
                indexes.Add(field.ToLowerInvariant(), Count - 1);
            }
        }
    }

    public class KeyLookup
    {
        public KeyLookup()
        {
            IdToCode = new Dictionary<int, string>();
            CodeToId = new Dictionary<string, int>();
        }

        public Dictionary<int, string> IdToCode { get; set; }
        public Dictionary<string, int> CodeToId { get; set; }

        public int GetCodeToId(string code)
        {
            var k = 0;
            if (CodeToId.TryGetValue(code, out k))
            {
                return k;
            }
            throw new Exception($"Code: {code} not found");
        }
    }
}