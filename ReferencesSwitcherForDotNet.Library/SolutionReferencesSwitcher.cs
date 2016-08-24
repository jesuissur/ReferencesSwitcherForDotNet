using ReferencesSwitcherForDotNet.Library.Internal;

namespace ReferencesSwitcherForDotNet.Library
{
    public class SolutionReferencesSwitcher
    {
        internal const string Comment = "ReferencesSwitcherForDotNet needs this to rollback changes";
        private readonly Configuration _config;
        private readonly IUserInteraction _userInteraction;

        public SolutionReferencesSwitcher() : this(new ConsoleUserInteraction(), new Configuration())
        {
        }

        public SolutionReferencesSwitcher(Configuration config) : this(new ConsoleUserInteraction(), config)
        {
        }

        public SolutionReferencesSwitcher(IUserInteraction userInteraction, Configuration config)
        {
            _userInteraction = userInteraction;
            _config = config;
        }

        public void Rollback(string solutionFullPath)
        {
            new RollbackProjectReferences(_config).Rollback(solutionFullPath);
        }

        public void Switch(string solutionFullPath)
        {
            new SwitchProjectReferences(_userInteraction, _config).Switch(solutionFullPath);
        }
    }
}