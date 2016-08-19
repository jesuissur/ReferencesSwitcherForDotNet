using ReferencesSwitcherForDotNet.Library.Internal;

namespace ReferencesSwitcherForDotNet.Library
{
    public class SolutionReferencesSwitcher
    {
        internal const string Comment = "ReferencesSwitcherForDotNet needs this to rollback changes";

        public void Rollback(string solutionFullPath)
        {
            new RollbackProjectReferences().Rollback(solutionFullPath);
        }

        public void Switch(string solutionFullPath)
        {
            new SwitchProjectReferences().Switch(solutionFullPath);
        }
    }
}