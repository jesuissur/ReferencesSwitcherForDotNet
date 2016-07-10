using Microsoft.Build.Construction;

namespace ReferencesSwitcherForDotNet.Library
{
    public class SolutionReferencesSwitcher
    {
        public void Switch(string solutionFullPath)
        {
            var solution = SolutionFile.Parse(solutionFullPath);
            solution.ProjectsInOrder
        }
    }
}