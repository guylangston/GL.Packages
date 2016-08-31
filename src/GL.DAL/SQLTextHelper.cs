using System;
using System.Collections.Generic;
using System.Text;

namespace GL.FluentDAL
{
    public static class SQLTextHelper
    {
        /// <summary>
        /// Overload
        /// </summary>
        public static string ToStringConcat<T>(IEnumerable<T> data)
        {
            return ToStringConcat(data, x => x == null ? "" : x.ToString(), ", ");
        }

        /// <summary>
        /// Overload
        /// </summary>
        public static string ToStringConcat<T>(IEnumerable<T> data, Func<T, object> toString)
        {
            return ToStringConcat(data, toString, ", ");
        }


        /// <summary>
        /// Render a simple list. Nulls are skipped
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Source</param>
        /// <param name="toString">if null, then object.ToString() is used</param>
        /// <param name="seperator">if null, no separator is used</param>
        /// <returns>Concatenated string</returns>
        public static string ToStringConcat<T>(IEnumerable<T> data, Func<T, object> toString, string seperator, bool skipNull = true, int wrap = -1, string wrapPrefix = "")
        {
            if (data == null) return null;

            var sb = new StringBuilder();
            var line = new StringBuilder();
            bool first = true;
            foreach (T item in data)
            {
                if (skipNull && item == null)
                {
                    continue;
                }
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (seperator != null) line.Append(seperator);
                }

                if (toString != null)
                {
                    object val = toString(item);
                    if (val != null)
                    {
                        line.Append(val.ToString());
                    }
                }
                else
                {
                    line.Append(item.ToString());
                }
                if (wrap < 0)
                {
                    sb.Append(line);
                    line.Clear();
                }
                else
                {
                    if (line.Length > wrap)
                    {
                        sb.Append(wrapPrefix);
                        sb.Append(line);
                        sb.AppendLine();
                        line.Clear();
                    }
                }
            }



            sb.Append(line);

            return sb.ToString();
        }
    }
}