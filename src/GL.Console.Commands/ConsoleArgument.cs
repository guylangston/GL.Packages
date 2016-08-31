using System;
using System.Collections.Generic;

namespace GL.Console.Commands
{
    public class ConsoleArgument : Argument
    {
        // Implementation
        protected internal ConsoleCommandBase Owner { get; set; }
        protected internal ConsoleCommandController State { get; set; }

        public string GetValueText()
        {
            if (State == null) throw new InvalidOperationException("Too early, no state yet.");
            var val = State.FindArgQualified(Name, Default);
            
            if (string.IsNullOrWhiteSpace(val))
            {
                return Default;
            }
            return val;
        }

        public string ValueText
        {
            get
            {
                var v = GetValueText();
                if (IsRequired && string.IsNullOrWhiteSpace(v)) throw new Exception("Required Value: "+Name);
                
                return v;
            }
        }

        public bool HasValue => State.FindArgQualified(Name) != null;
    }

    public class ConsoleArgument<T> : ConsoleArgument
    {
        public Func<string, T> Parser { get; set; }

        public T Value
        {
            get
            {
                if (Parser == null) throw new Exception("Parser for T not defined");
                return Parser(ValueText);
            }
        }
    }


   
}