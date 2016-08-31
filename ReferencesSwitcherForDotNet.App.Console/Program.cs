using System;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.App.Console
{
    internal class Program
    {
        private static void DisplayHelp(Arguments arguments)
        {
            arguments.DisplayHelp();
            System.Console.ReadLine();
            Environment.Exit(-1);
        }

        private static void Main(string[] args)
        {
            try
            {
                var config = new Configuration();
                var arguments = new Arguments(new ConsoleUserInteraction(), args, config);

                if (arguments.AreMissing)
                    DisplayHelp(arguments);

                var switcher = new SolutionReferencesSwitcher(config);
                if (arguments.ShouldSwitch)
                    arguments.SolutionsFullPath.ForEach(x=>switcher.Switch(x));
                else
                    arguments.SolutionsFullPath.ForEach(x=>switcher.Rollback(x));
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Unexpected error just happened.  We're sorry.  Here are the details: {e}");
            }
        }
    }
}