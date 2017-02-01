using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace ReferencesSwitcherForDotNet.Library.Extensions
{
    public static class ProjectSolutionExtensions
    {

        public static IEnumerable<ProjectInSolution> GetExistingProjects(this SolutionFile solution, Configuration config)
        {
            foreach (var solutionProject in solution.ProjectsInOrder.Where(x => config.ProjectNameShouldNotBeSkipped(x.ProjectName)))
                if (File.Exists(solutionProject.AbsolutePath))
                    yield return solutionProject;
        }

    }
}
