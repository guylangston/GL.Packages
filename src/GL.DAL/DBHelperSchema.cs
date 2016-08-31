using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace GL.DAL
{
    public static class DBHelperSchema
    {
        public static bool ExistsTable(string db, string table, string schema = "dbo")
        {
            var sqlText = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='{0}' AND TABLE_NAME='{1}'", schema, table);
            try
            {
                using (var conn = new SqlConnection(db))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = sqlText;
                        using (var reader = command.ExecuteReader())
                        {
                            return reader.HasRows;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw DBHelper2.HandleException(ex, db, sqlText);
            }
        }

        public class SchemaTable
        {
            public string Catelog { get; set; }
            public string Schema { get; set; }
            public string Table { get; set; }
            public string TableType { get; set; }

            public IReadOnlyList<SchemaField> Fields { get; set; }
        }

        public class SchemaField
        {
            public SchemaTable Table { get; set; }

            public string Schema { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsNull { get; set; }
            public int Index { get; set; }

            public object[] RawProps { get; set; }
            public int? NumericPrecision { get; set; }
            public int? NumericPrecisionRadix { get; set; }
            public int? NumericPrecisionScale { get; set; }
            public int? DateTimePrecision { get; set; }
            public int? StringMaxLength { get; set; }
        }

        /// <summary>
        /// No fields
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static IReadOnlyList<SchemaTable> GetTablesOnly(string conn)
        {
            return OldHelper.ExecuteQuery(conn, r =>
            {
                return new SchemaTable()
                {
                    Catelog = r.GetString(0),
                    Schema = r.GetString(1),
                    Table = r.GetString(2),
                    TableType = r.GetString(3)
                };
            },
                "SELECT * FROM INFORMATION_SCHEMA.TABLES ORDER BY  TABLE_NAME");
        }



        public static IReadOnlyList<SchemaTable> GetAllTablesAndFields(string conn)
        {
            var tbls = GetTablesOnly(conn);
            
            OldHelper.ExecuteQuery(conn, r =>BindField(r, tbls),
                "SELECT * FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME, ORDINAL_POSITION");

            return tbls;
        }

        public static IReadOnlyList<SchemaTable> GetTableAndFields(string conn, string table)
        {
            var tbls = GetTablesOnly(conn);

            OldHelper.ExecuteQuery(conn, r => BindField(r, tbls),
                "SELECT * FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME={0} ORDER BY TABLE_NAME, ORDINAL_POSITION", table);

            return tbls;
        }

        public class TableSizeEntry
        {
            public SchemaTable Table { get; set; }

            public string TableName => Table.Table;

            public long Rows { get; set; }
            public string Reserved { get; set; }
            public string Data { get; set; }
            public string IndexSize { get; set; }
        }


        public static List<TableSizeEntry> GetTableSizes(string conn)
        {
            var res = new List<TableSizeEntry>();

            foreach (var table in GetTablesOnly(conn))
            {
                
                res.Add(OldHelper.ExecuteQuerySingle(conn, r =>
                {
                    return new TableSizeEntry()
                    {
                        Table = table,
                        Rows = r.GetUnknown<long>(1),
                        Reserved = r.GetUnknown<string>(2),
                        Data = r.GetString(3), 
                        IndexSize = r.GetString(4)
                    };
                }, "EXEC sp_spaceused {0};", table.Table));
            }

            return res;
        }


        private static SchemaField BindField(IDataReader r, IReadOnlyList<SchemaTable> tbls)
        {
            var f = new SchemaField();
            f.RawProps = new object[r.FieldCount];
            r.GetValues(f.RawProps);

            var t = r.GetString(2); // TABLE_CATELOG
            f.Table = tbls.First(x => x.Table == t);
            if (f.Table.Fields == null)
            {
                f.Table.Fields = new List<SchemaField>();
            }
            (f.Table.Fields as List<SchemaField>).Add(f);

            f.Schema = r.GetString(1); // TABLE_SCHEMA
            f.Name = r.GetString(3);    // COLUMN_NAME
            f.Index = r.GetInt32(4); // ORDINAL_POSITION
            f.Type = r.GetString(7); // DATA_TYPE
            f.StringMaxLength = OldHelper.ReadInt32Null(r, 8);
            f.NumericPrecision = OldHelper.ReadByteNull(r, 10); // NUMERIC_PRECISION
            f.NumericPrecisionRadix = OldHelper.ReadShortNull(r, 11); // NUMERIC_PRECISION_RADIX
            f.NumericPrecisionScale = OldHelper.ReadInt32Null(r, 12); // NUMERIC_PRECISION_SCALE
            f.DateTimePrecision = OldHelper.ReadShortNull(r, 13); // DATETIME_PRECISION

            return f;
        }
    }
}