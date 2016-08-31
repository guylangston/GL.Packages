using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace GL.DAL
{
    public static class DBHelperExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal? GetDecimalSafe(this IDataReader r, int ordinal)
            => r.IsDBNull(ordinal) ? null : (decimal?) r.GetDecimal(ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal GetDecimalSafeAsMin(this IDataReader r, int ordinal)
            => r.IsDBNull(ordinal) ? decimal.MinValue : r.GetDecimal(ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringSafe(this IDataReader r, int ordinal)
            => r.IsDBNull(ordinal) ? null : r.GetString(ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetInt32Safe(this IDataReader r, int ordinal)
            => r.IsDBNull(ordinal) ? null : (int?) r.GetInt32(ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? GetDateTimeSafe(this IDataReader r, int ordinal)
            => r.IsDBNull(ordinal) ? null : (DateTime?) r.GetDateTime(ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetEnum<T>(this IDataReader r, int ordinal) where T : struct
            => (T) Enum.ToObject(typeof (T), r.GetInt32(ordinal));


        public static T GetUnknown<T>(this IDataReader r, int ordinal)
        {
            var obj = r.GetValue(ordinal);
            try
            {
                return (T) Convert.ChangeType(obj, typeof (T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"Unable to convert {obj} of {obj?.GetType().FullName} to {typeof (T).FullName}", ex);
            }
        }
    }
}