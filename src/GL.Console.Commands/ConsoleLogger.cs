using System;

namespace GL.Console.Commands
{
    public class ConsoleLogger
    {
        ConsoleCommandController parent;

        public ConsoleLogger(ConsoleCommandController parent)
        {
            this.parent = parent;
        }

        public void Chunk(string verb, string desc, Action action)
        {
            System.Console.Write($"[{verb.PadRight(8)}] {desc,-40}...");
            action();
            System.Console.WriteLine(" Done.");
        }
    }
}