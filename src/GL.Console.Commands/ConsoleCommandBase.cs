using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GL.Console.Commands
{
    /// <summary>
    /// A abstract helper class to manage a Console Application
    /// </summary>
    public abstract class ConsoleCommandBase
    {
        private ConsoleCommandController controller;

        protected internal bool SurpressConsole { get; set; }

        /// <summary>
        /// The user will be aske to press a key to adknowledge ANY exception
        /// </summary>
        public bool WaitOnError { get; set; }
        
        /// <summary>
        /// Strong Construction
        /// </summary>
        /// <param name="name">Example: PING</param>
        /// <param name="description">Example: PING a machine on the internet</param>
        /// <param name="example">Example: PING www.nba.com</param>
        protected ConsoleCommandBase(ConsoleCommandController controller, string name, string description, string example)
        {
            this.controller = controller;
            this.Name = name;
            this.Description = description;
            this.Example = example;

            ArgDescription = new List<Tuple<string, bool, string>>();
            ConsoleArguments = new ArgumentListFunc(x => controller.FindArgQualified(x));
            WaitOnError = true;
        }

        /// <summary>
        /// Unique Command Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Human-readable Command Description (this will be shown in help text)
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// A human-readable example of what a fully command may look like
        /// </summary>
        public string Example { get; }

        public List<Tuple<string, bool, string>> ArgDescription { get; set; }
        public ArgumentList ConsoleArguments { get; set; } 
        public int MinParams { get; set; }

        [Obsolete("Use RegisterArgument")]
        public ConsoleCommandBase AddArgument(string name, bool required, string description)
        {
            ArgDescription.Add(new Tuple<string, bool, string>(name, required, description));
            return this;
        }

        public ConsoleArgument RegisterArgument(ConsoleArgument arg)
        {
            ArgDescription.Add(new Tuple<string, bool, string>(arg.Name, arg.IsRequired, arg.Description));
            arg.Owner = this;

            ConsoleArguments.Register(arg);
            return arg;
        }

        public ConsoleArgument<T> RegisterArgument<T>(ConsoleArgument<T> arg)
        {
            ArgDescription.Add(new Tuple<string, bool, string>(arg.Name, arg.IsRequired, arg.Description));
            arg.Owner = this;

            ConsoleArguments.Register(arg);
            return arg;
        }

        public ConsoleArgument<TReturn> RegisterArgument<TClass, TReturn>(
            TClass target, 
            Expression<Func<TClass, TReturn>> exp, 
            Action<TClass, string> bind, 
            bool required = true, 
            string desc = null)
        {
            var expStr = exp.ToString().Remove(0, "x => x.".Length);
            var comp = exp.Compile();
            var args = new ConsoleArgument<TReturn>()
            {
                Name =  expStr,
                IsRequired = required,
                Description = desc,
                Default = Serialise(comp(target)),
                BindStingArgvalue = (o,s) => bind((TClass)o,s)
            };
            return RegisterArgument(args);
        }

        private string Serialise(object comp)
        {
            if (comp == null) return null;

            if (comp is DateTime) return ((DateTime) comp).ToString("yyyy-MM-dd");

            return comp.ToString();
        }

        /// <summary>
        /// Can this command process this command name
        /// </summary>
        /// <param name="argCommandName"></param>
        /// <returns></returns>
        public virtual bool CanProcess(string argCommandName)
        {
            return string.Equals(Name, argCommandName, StringComparison.OrdinalIgnoreCase);
        }

        public virtual bool CheckArgs(ConsoleCommandController controller, Action<string> displayError)
        {
            return true;
        }

        /// <summary>
        /// Execute the command one the parameters have been matched and validated
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public abstract CommandResults Execute(ConsoleCommandController controller);

        public override string ToString()
        {
            return Name;
        }

      
    }
}