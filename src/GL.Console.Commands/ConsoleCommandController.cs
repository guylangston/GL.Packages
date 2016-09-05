using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GL.Console.Commands
{

    public enum CommandResults
    {
        Ok = 0,
        NoResult = 1,
        Error = -1,
        Exception = -2,
        NotFound = -3,
        UnExpectedError = -4,
        ParamError,
        Cancelled
    }



    /// <summary>
    /// This is a helper class to allow multiple commands with multiple parameters to be quickly built for a Console app.
    /// </summary>
    public class ConsoleCommandController 
    {
        private readonly string applicationName;
        private readonly string applicationVersion;
        private readonly string applicationCommandLine;
        private readonly List<ConsoleCommandBase> commands;
        private string[] args;
        private CommandResults result;
        public string BannerText = $"Copyright (C) 2014-{DateTime.Now.Year}. All Rights Reserved. ";
        protected internal bool SurpressConsole { get; set; }


        /// <summary>
        /// Strong Construction
        /// </summary>
        public ConsoleCommandController(string applicationName, string applicationVersion, string applicationCommandLine, string[] executeArgs)
        {
            this.args = executeArgs;
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            this.applicationCommandLine = applicationCommandLine;
            commands = new List<ConsoleCommandBase>();
            Log = new ConsoleLogger(this);
        }

        public ConsoleLogger Log { get; }
        public ConsoleCommandBase CurrentlyExecutingCommand { get; private set; }

        public string[] GetCommandLineArguments() => args.Skip(1).ToArray();

        /// <summary>
        /// Find ard in the form '-arg:value'
        /// </summary>
        /// <param name="name">Should be in the form -NAME:  (prefix -, suffix :)</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual string FindArg(string name, string defaultValue = null)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Remove(0, name.Length);
                }
            }
            return defaultValue;
        }


        /// <summary>
        /// Find ard in the form '-arg:value'
        /// </summary>
        /// <param name="name">Should be in the form -NAME:  (prefix -, suffix :)</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual string FindArgQualified(string name, string defaultValue = null)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"+name+":", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Remove(0, name.Length+2);
                }
            }
            return defaultValue;
        }

        public virtual string FindArgRequired(string name)
        {
            var res = FindArg(name);
            if (res == null)
            {
                throw new Exception(string.Format("Expected argument {0} missing", name));
            }
            return res;
        }



        public string FindArg(int number, string defaultArg)
        {
            if (args.Length <= 1)
                return defaultArg;

            if (number > (args.Length - 1))
                return defaultArg;

            var arg = args[number + 1];
            if (String.IsNullOrEmpty(arg))
                return defaultArg;

            return arg;
        }

        public virtual T FindArgThen<T>(string name, string defaultValue, Func<string, T> converter, bool skipNull = true)
        {
            var arg = FindArg(name, defaultValue);
            if (arg == null && skipNull) return default(T);
            return converter(arg);

        }

        /// <summary>
        /// Find the argument (not including the command name)
        /// </summary>
        /// <param name="index">cmd.exe PLAY file; here file is index 0</param>
        /// <returns></returns>
        public string FindArg(int index)
        {
            if (index >= (args.Length - 1)) return null;

            return args[index + 1];
        }

        public void Add(ConsoleCommandBase cmd)
        {
            commands.Add(cmd);
        }

        public CommandResults Execute()
        {
            var colourAtStart = System.Console.ForegroundColor;
            using (var timer = new CodeTimer())
            {
                ExecuteInternal();
                switch (result)
                {
                    case CommandResults.Ok:
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    default:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Display("Exiting with ReturnCode: {0}({1}) in {2}", result, (int)result, Elipse(timer.Duration.ToString(), 80));   
            }

            System.Console.ForegroundColor = colourAtStart;

            return result;
        }

        private static string Elipse(string text, int max, string elipse = "...")
        {
            if (text == null) return null;
            text = text.Replace('\n', '|').Replace('\r', '|').Replace('\f', '|');
            if (text.Length <= max) return text;
            return text.Substring(0, max - elipse.Length) + elipse;

        }

        private void ExecuteInternal()
        {
            var colourAtStart = System.Console.ForegroundColor;
            ConsoleCommandBase match = null;
            try
            {
                result = CommandResults.Ok;
                match = GetMatchingCommand();

                if (result != CommandResults.Ok)
                {
                    return;
                }
                if (match == null)
                {
                    // No Command Found
                    DisplayBanner();
                    Display("No commands Found");
                    Display(string.Empty);
                    DisplayHelp();
                    result = CommandResults.NotFound;
                    return;
                }
                else if (args.Contains("-?") || args.Contains("/?"))
                {
                    DisplayBanner();
                    Display("Help for command '{0}' -- {1}", match.Name, match.Description);
                    DisplayHelp(match, true);

                    return;
                }

                SurpressConsole = match.SurpressConsole;
                match.WaitOnError = GetWaitOnError();

                DisplayBanner();

                Display(string.Empty);

                Display("Running command '{0}' -- {1}; using the following arguments:", match.Name, match.Description);

                // Process and display arguments
                foreach (var arg in match.ConsoleArguments)
                {
                    var argument = arg.Argument as ConsoleArgument;
                    argument.State = this;
                    argument.Owner = match;
                    DisplayLabel("-" + argument.Name, argument.GetValueText() ?? "(null)");
                }
                Display(string.Empty);

                var ok = match.CheckArgs(this, x =>
                {
                    var c = System.Console.ForegroundColor;
                    System.Console.ForegroundColor = ConsoleColor.Red;

                    System.Console.Write("ERROR: ");
                    System.Console.ForegroundColor = ConsoleColor.DarkMagenta;

                    System.Console.Write(x);
                    System.Console.ForegroundColor = c;
                    System.Console.WriteLine();
                });
                if (!ok)
                {
                    DisplayHelp(match, true);
                    result = CommandResults.ParamError;
                    return;
                }

                // Global Arguments
                if (FindArg("-debug:") != null)
                {
                    System.Console.WriteLine("DEBUGGER: Launching Debugger");
                    Debugger.Launch();
                }

                if (FindArg("-check") != null)
                {
                    System.Console.WriteLine("Confirm Settings, [Enter] to Continue; [Ctrl+C] to exit...");
                    System.Console.ReadLine();
                }

                this.CurrentlyExecutingCommand = match;
                result = match.Execute(this);

            }
            catch (Exception ex)
            {
                var d = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.Red;
                SurpressConsole = false;
                Display(ex);
                System.Console.ForegroundColor = d;

                result = CommandResults.UnExpectedError;
            }
            finally
            {
                System.Console.ForegroundColor = colourAtStart;
            }
        }

        private void DisplayBanner()
        {
            Display($"{applicationName} version {applicationVersion}. {GetInstructionSetDescription()} [{(GetElevatedString())}]", 0);
            
            Display(BannerText);
        }

        private static string GetInstructionSetDescription()
        {
#if WINDOWS
            return (Environment.Is64BitProcess ? "x64" : "x86");
#else
            return "??";
#endif
        }

        private string GetElevatedString()
        {
            var r = GetElevated();
            if (r == null) return "?";
            return r.Value ? "Admin" : "Normal";
        }

        private bool? GetElevated()
        {
#if WINDOWS
             try
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();
                var windowsPrincipal = new WindowsPrincipal(windowsIdentity);
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return null;
            }
#else
            return null;
#endif
        }


        private ConsoleCommandBase GetMatchingCommand()
        {
            if (args.Length > 0)
            {
                // Using the first argument check to see if there is a command we can execute
                foreach (var command in commands)
                {
                    if (!command.CanProcess(args[0])) continue;
                    if (command.MinParams > args.Length - 1)
                    {
                        Display("This command {0} requires {1} params, only {2} was given.", command.Name,
                            args.Length, command.MinParams);
                        result = CommandResults.ParamError;
                    }

                    return command;
                }
            }
            return null;
        }

        private bool GetWaitOnError()
        {
            return args.Any(arg => arg.ToLower().Trim().Equals("-pause"));
        }

        private void DisplayHelp()
        {
            DisplayHeader("Available Commands", 0);
            foreach (var command in commands)
            {
                DisplayHelp(command, false);
            }
            Display(string.Empty);
            DisplayHeader("Universal arguments", 0);
            Display("\t-debug:debug\twill attach the .NET debugger");
            Display("\t-check\t will confirm settings before execution");
            Display("\t<command> -? or /?\t will display argument help");
            Display(string.Empty);
            Display("In the format 'C:\\> {0} COMMAND -arg1:123 -arg2:SomeString'", applicationCommandLine);
        }

        private void DisplayHelp(ConsoleCommandBase cmd, bool verbose)
        {
            Display("{0,18}\t{1}", cmd.Name, cmd.Description);
            if (!string.IsNullOrWhiteSpace(cmd.Example))
            {
                Display("\t\t\tExample: {0}", cmd.Example);
            }
            if (verbose)
            {
                if (cmd.ConsoleArguments != null)
                {
                    foreach (var a in cmd.ConsoleArguments)
                    {
                        var arg = a.Argument as ConsoleArgument;
                        Display("\t\t\tArg: {0,10} ({1}) {2,-40} Default: {3}",
                            arg.Name,
                            arg.IsRequired ? "REQUIRED" : "Optional",
                            arg.Description,
                            Elipse(arg.Default ?? "(null)", 50));
                    }
                }
                else if (cmd.ArgDescription != null)
                {
                    foreach (var arg in cmd.ArgDescription)
                    {
                        Display("\t\t\tArgument: {0} ({1}) {2}",
                            arg.Item1,
                            arg.Item2 ? "REQUIRED" : "Optional",
                            arg.Item3);
                    }
                }
                Display(string.Empty);
            }
            
        }

        public void Display(Exception ex)
        {
            DisplayHeader("Error");
            Display(ex.GetType().Name);
            Display(ex.Message);
            Display(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Display(ex.InnerException);
            }
        }

        private void DisplayHeader(string title, int depth = 1)
        {
            if (depth == 0)
            {
                Display("=========================================");
                Display(" " + title);
                Display("-----------------------------------------");
                return;
            }

            if (depth == 1)
            {
                Display(String.Empty);
                Display(" " + title);
                Display("-----------------------------------------");
                return;
            }


            Display("### " + title + " ### ");
        }


        public void Display(string aLine)
        {
            if (SurpressConsole) return;

            System.Console.WriteLine(aLine);
        }

        public void DisplayNonCr(string atext)
        {
            if (SurpressConsole) return;

            System.Console.Write(atext);
        }

        public void DisplayLabel(string label, object value)
        {
            Display("{0,20}: {1}", label, value);
        }

        public void Display(string stringFormat, params object[] arguments)
        {
            Display(string.Format(stringFormat, arguments));
        }


        public bool Confirm(string msg)
        {
            System.Console.WriteLine(msg);
            System.Console.WriteLine("Press 'Y' to agree, any other key cancels");
            return System.Console.ReadKey().Key == ConsoleKey.Y;
        }

        
        private static object locker = new object();
        
        public void BindArgs(object targetForArgs)
        {
            foreach (var arg in CurrentlyExecutingCommand.ConsoleArguments)
            {
                if (arg.Argument.BindStingArgvalue != null)
                {
                    var rawValue = CurrentlyExecutingCommand.ConsoleArguments.GetRawValue(arg.Argument.Name)
                        ?? arg.Argument.Default;
                    
                    arg.Argument.BindStingArgvalue(targetForArgs, rawValue);
                }
            }
        }

       
    }
}