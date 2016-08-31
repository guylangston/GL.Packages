using System;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace GL.FluentDAL
{
    public static class DBHelper2
    {

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


        public static DatabaseException HandleException(Exception sql, string conn, string sqlText)
        {
            var text = string.Format("{0}\n{1}\n{2}", sql.Message, conn, sqlText);

            return new DatabaseException(text, sql)
            {
                Connection = conn,
                SQL = sqlText
            };
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
                var dt = (DateTime)arg;
                if (dt == DateTime.MinValue) return "NULL";
                //return dt.ToString("yyyyMMdd");
                return "'" + Make8601LongFormat((DateTime)arg) + "'";
            }
            if (arg is Enum)
            {
                return ((int)arg).ToString();
            }
            if (arg is bool)
            {
                return (bool)arg ? "1" : "0";
            }
            var toStr = arg.ToString();
            var escaped = toStr.Replace("'", "''");
            return "N'" + escaped + "'";
        }

        public static string Make8601LongFormat(DateTime input)
        {
            return input.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
        }

        public static string EscapeStringNoQuotes(string raw)
        {
            if (raw == null) return null;
            return raw.Replace("'", "''");
        }


    }
}