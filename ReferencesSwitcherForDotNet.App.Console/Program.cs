using System;
using System.Text;
using Arguments;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.App.Console
{
    internal class Program
    {
        private static void DisplayHelp()
        {
            Arguments.DisplayHelp();
            System.Console.ReadLine();
            Environment.Exit(-1);
        }

        private static void Main(string[] args)
        {
            try
            {
                var config = new Configuration();
                var arguments = new Arguments(args, config);

                if (arguments.AreMissing)
                    DisplayHelp();

                var switcher = new SolutionReferencesSwitcher(config);
                if (arguments.ShouldSwitch)
                    switcher.Switch(arguments.SolutionFullPath);
                else
                    switcher.Rollback(arguments.SolutionFullPath);
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Unexpected error just happened.  We're sorry.  Here are the details: {e}");
            }
        }

        private class Arguments
        {
            public Arguments(string[] args, Configuration config)
            {
                new ArgumentProcessor(args).UsingParameterSeparator('=')
                                           .AddArgument("help", "?").WithAction(x => DisplayHelp())
                                           .AddArgument("s", "solution").WithAction(x => SolutionFullPath = x)
                                           .AddArgument("ips", "ignorePatterns").WithAction(x => config.ProjectNameIgnorePatterns.AddRange(x.Split("|")))
                                           .AddArgument("switch").WithAction(x => ShouldSwitch = true)
                                           .AddArgument("rollback").WithAction(x => ShouldRollback = true)
                                           .Process();
            }

            public bool AreMissing => SolutionFullPath.IsNullOrWhiteSpace() || (!ShouldSwitch && !ShouldRollback);

            public bool ShouldRollback { get; set; }

            public bool ShouldSwitch { get; set; }

            public string SolutionFullPath { get; set; }

            internal static void DisplayHelp()
            {
                var help = new StringBuilder();
                help.AppendLineFormat("#####################################");
                help.AppendLineFormat("Arguments for the References Switcher");
                help.AppendLineFormat("#####################################");
                help.AppendLineFormat("");
                help.AppendLineFormat(@"-?|help");
                help.AppendLineFormat(@"-s[olution]=""C:\FullPath\To\Solution.sln""");
                help.AppendLineFormat(@"-switch");
                help.AppendLineFormat(@"-rollback");
                help.AppendLineFormat(@"-ips|ignorePatterns=PartOfProjectNameToIgnore1|AnotherPart|...");
                System.Console.WriteLine(help);
            }
        }
    }
}