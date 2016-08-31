using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GL.Console.Commands
{
 

    public enum ArgType
    {
        String,
        Integer,
        Decimal,
        Date,
        DateTime,
        Bool,

        FileName
    }

    /// <summary>
    /// The goal here is to make a universal, simple, string-based, self-documenting argument system 
    /// Compatible with web, console, webservices, json, etc.
    /// </summary>
    public class Argument
    {
        // Meta Data
        public string Name { get; set; }
        public ArgType Type { get; set; }
        public bool IsRequired { get; set; }

        public string Default { get; set; }
        public string Example { get; set; }
        public string Description { get; set; }

        // return null for OK
        public Func<string, string> ValidateString { get; set; }
        public Action<object, string> BindStingArgvalue { get; set; }
    }

    public class Argument<T> : Argument
    {
        public Func<string, T> Parser { get; set; }
        public Func<string, T, string> Validate { get; set; }
    }

    public class IntegerArgument : Argument<int>
    {
        public IntegerArgument()
        {
            Parser = int.Parse;
        }
    }

    public class DateTimeArgument : Argument<DateTime>
    {
        public DateTimeArgument()
        {
            Parser = DateTime.Parse;
        }
    }

    public class ArgumentInstance
    {
        public ArgumentInstance(Argument argument, string rawValue)
        {
            Argument = argument;
            RawValue = rawValue;
        }

        public Argument Argument { get; }

        // Core props
        public string Name => Argument.Name;
        public ArgType Type => Argument.Type;
        public bool IsRequired => Argument.IsRequired;

        // Instance
        public string RawValue { get; set; }

        public bool HasValue => !string.IsNullOrWhiteSpace(RawValue);

        // Parsed, Validated
        public virtual string ValueText
        {
            get
            {
                var v = RawValue;
                if (IsRequired && !HasValue) throw new Exception("Required Value: " + Name);
                if (Argument.ValidateString != null)
                {
                    var error = Argument.ValidateString(v);
                    if (error != null)
                    {
                        throw new Exception("Failed Validation: " + error);
                    }
                }
                return v;
            }
        }


    }

    public class ArgumentInstance<T> : ArgumentInstance
    {
        public ArgumentInstance(Argument<T> argument, string rawValue) : base(argument, rawValue)
        {
            Argument = argument;
        }

        new public Argument<T> Argument { get; }

        public T Value
        {
            get
            {
                if (Argument.Parser == null) throw new Exception("Parser for T not defined");
                try
                {
                    var p = Argument.Parser(ValueText);
                    if (Argument.Validate != null)
                    {
                        var error = Argument.Validate(ValueText, p);
                        if (error != null)
                        {
                            throw new Exception("Failed Validation: " + error);
                        }
                    }
                    return p;
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Unable to Parse '{ValueText}'", ex);
                }
            }
        }
    }

    public abstract class ArgumentList : List<ArgumentInstance>
    {
        public abstract string GetRawValue(string name);

        public ArgumentInstance Register(Argument a)
        {
            var i = new ArgumentInstance(a, null);
            Add(i);
            i.RawValue = GetRawValue(a.Name); // allows reflection/complex logic
            return i;
        }

        public ArgumentInstance<T> Register<T>(Argument<T> a)
        {
            var i = new ArgumentInstance<T>(a, null);
            Add(i);
            i.RawValue = GetRawValue(a.Name); // allows reflection/complex logic
            return i;
        }

        public void PrintHelp(TextWriter outp)
        {
            PrintValues(outp);
        }

        public void PrintValues(TextWriter outp)
        {
            foreach (var arg in this)
            {
                var r = arg.IsRequired ? "(*)" : "   ";
                outp.WriteLine($"{arg.Name,20}{r}[{arg.Type,8}]={arg.RawValue,-20} Default: {arg.Argument.Default,-20} | {arg.Argument.Description}");
            }
        }
    }

    public class ArgumentListFromDict : ArgumentList
    {
        private readonly Dictionary<string, string> rawValues;

        public ArgumentListFromDict(Dictionary<string, string> rawValues)
        {
            this.rawValues = rawValues;
        }

        public override string GetRawValue(string name)
        {
            string s = null;
            rawValues.TryGetValue(name, out s);
            return s;
        }


    }

    public class ArgumentListFunc : ArgumentList
    {
        private readonly Func<string, string> getRawValueFunc;

        public ArgumentListFunc(Func<string, string> getRawValueFunc)
        {
            this.getRawValueFunc = getRawValueFunc;
        }

        public override string GetRawValue(string name)
        {
            return getRawValueFunc(name);
        }
    }
}
