using System.Collections.Generic;
using System.Text;
using Arguments;

namespace ReferencesSwitcherForDotNet.Library
{
    public class Arguments
    {
        private readonly IUserInteraction _userInteraction;

        public Arguments(IUserInteraction userInteraction, string[] args, Configuration config)
        {
            _userInteraction = userInteraction;
            new ArgumentProcessor(args).UsingParameterSeparator('=')
                                       .AddArgument("help", "?").WithAction(x => DisplayHelp())
                                       .AddArgument("s", "solutions").WithAction(x => SolutionsFullPath.AddRange(x.Split(",")))
                                       .AddArgument("ips", "ignorePatterns").WithAction(x => config.ReferenceIgnorePatterns.AddRange(x.Split(",")))
                                       .AddArgument("skip").WithAction(x => config.ProjectSkipPatterns.AddRange(x.Split(",")))
                                       .AddArgument("switch").WithAction(x => ShouldSwitch = true)
                                       .AddArgument("rollback").WithAction(x => ShouldRollback = true)
                                       .AddArgument("noWayBack").WithAction(x => config.ShouldLeaveNoWayBack = true)
                                       .AddArgument("acceptReadonlyOverwrite").WithAction(x => config.ShouldAskForReadonlyOverwrite = false)
                                       .Process();
        }

        public bool AreMissing => SolutionsFullPath.IsNullOrEmpty() || (!ShouldSwitch && !ShouldRollback);

        public bool ShouldRollback { get; set; }

        public bool ShouldSwitch { get; set; }

        public List<string> SolutionsFullPath { get; } = new List<string>();

        public void DisplayHelp()
        {
            var help = new StringBuilder();
            help.AppendLineFormat("#####################################");
            help.AppendLineFormat("Arguments for the References Switcher");
            help.AppendLineFormat("#####################################");
            help.AppendLineFormat("");
            help.AppendLineFormat("-?|help\t\tDisplay this helpful documentation");
            help.AppendLineFormat(@"-s[olutions]=""C:\FullPath\To\Solution.sln[,C:\FullPath\To\AnotherSolution.sln,...]""");
            help.AppendLineFormat("-switch");
            help.AppendLineFormat("-rollback");
            help.AppendLineFormat("-ips|ignorePatterns=PartOfProjectNameToIgnore1,AnotherPart,...\t Do not switch references whose name match one of the patterns");
            help.AppendLineFormat("-noWayBack\tThe switch operation is not going to support the rollback operation.  There is no way back :)");
            help.AppendLineFormat("-acceptReadonlyOverwrite\t Readonly Project files will be overriden without asking");
            _userInteraction.DisplayMessage(help.ToString());
        }
    }
}