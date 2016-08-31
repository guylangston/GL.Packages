using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace GL.CMS
{
    public class DateRange : IEnumerable<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;
            if (start > end)
            {
                End = start;
                Start = end;
            }
            else
            {
                End = end;
                Start = start;
            }
        }

        public DateRange(DateTime start, int offset) : this(start, start.AddDays(offset)) { }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        /// <summary>
        /// Doubly-inclusive (Start, End) both inside = 1 day
        /// </summary>
        public int Count => (int)((End - Start).TotalDays) + 1;

        public static readonly DateRange All = null;

        public bool Contains(DateTime d) => d >= Start && d <= End;

        public bool Contains(DateRange r) => Contains(r.Start) && Contains(r.End);

        public IEnumerator<DateTime> GetEnumerator()
        {
            var s = Start;
            while (s <= End)
            {
                yield return s;
                s = s.AddDays(1);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static DateRange Combine(params DateRange[] dates)
        {
            if (!dates.Any() || dates.All(x => x == null)) return null;
            return Combine(dates.Where(x => x != null));
        }

        public static DateRange Combine(IEnumerable<DateRange> ranges)
        {
            var r = ranges.Where(x => x != null).Select(x => x.Start).Union(ranges.Where(x => x != null).Select(x => x.End)).ToArray();
            if (r == null || !r.Any()) return null;
            return new DateRange(r.Min(), r.Max());
        }

        public static DateRange Combine(IEnumerable<DateTime> r)
        {
            if (!r.Any()) return null;
            return new DateRange(r.Min(), r.Max());
        }

        public static DateRange Intersect(DateRange a, DateRange b)
        {
            if (a == All && b == All) return All;

            if (a == DateRange.All) return b;
            if (b == DateRange.All) return a;
            return Combine(a.Intersect(b).ToArray());
        }


        public override string ToString()
        {
            return String.Format("[{0}..{1}={2}d]", Start, End, Count);
        }

        

        public static DateRange Parse(string txt, CultureInfo currentCulture, DateTime relZero)
        {
            if (string.IsNullOrWhiteSpace(txt)) return All;

            txt = txt.Replace("_", "..");
            var format = "yyyy-MM-dd";
            var format2 = "dd/MM/yyyy";

            try
            {
                return ParseInner(txt, currentCulture, relZero, format);
            }
            catch (Exception)
            {
                return ParseInner(txt, currentCulture, relZero, format2);
            }

        }

        private static DateRange ParseInner(string txt, CultureInfo currentCulture, DateTime relZero, string format)
        {
            if (string.IsNullOrWhiteSpace(txt)) return All;

            // Example "[01/01/2015..03/01/2015=3d]"
            if (txt.StartsWith("0.."))
            {
                txt = relZero.ToString(format) + txt.Remove(0, 1);
            }

            var t = txt.Trim('[', ']').Split('.', '=');
            var s = DateTime.ParseExact(t[0], format, currentCulture);

            int rel = 0;
            if (int.TryParse(t[2], out rel))
            {
                return new DateRange(s, s.AddDays(rel));
            }


            var e = DateTime.ParseExact(t[2], format, currentCulture);
            return new DateRange(s, e);
        }
    }
}