using System;
using System.Threading;

namespace GL.Console.Commands
{
    internal class CodeTimer : IDisposable
    {
        private DateTime? start;
        private DateTime? end;
        private readonly Action<CodeTimer> report;
        private int itemCount = 0;

        public CodeTimer()
        {
            Start();
        }

        public CodeTimer(Action<CodeTimer> report)
        {
            Start();
            this.report = report;
        }

        public static CodeTimer TimeBlockThen(Action<CodeTimer> report) => new CodeTimer(report);

        public void Start()
        {
            start = DateTime.Now;
        }

        public void Increment() => Interlocked.Increment(ref itemCount);
        public void Increment(int step) => Interlocked.Add(ref itemCount, step);

        public void Stop()
        {
            end = DateTime.Now;
            if (report != null) report(this);
        }

        public DateTime? Started => start;
        public int ItemCount => itemCount;
        public TimeSpan Duration => (end ?? DateTime.Now) - start.Value;


        public override string ToString()
        {
            if (itemCount > 0)
            {
                return $"{itemCount} in {Duration} at {Duration.TotalSeconds / (double)itemCount} items/sec";
            }
            return $"{Duration.TotalSeconds:0.0000} sec";
        }

        public void Dispose()
        {
            if (end == null)
            {
                Stop();
            }
        }
    }
}