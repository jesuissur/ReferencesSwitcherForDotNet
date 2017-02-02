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
            return GetProjects(solution, config).Where(x => File.Exists(x.AbsolutePath));
        }

        public static IEnumerable<ProjectInSolution> GetProjects(this SolutionFile solution, Configuration config)
        {
            foreach (var solutionProject in solution.ProjectsInOrder.Where(x => config.ProjectNameShouldNotBeSkipped(x.ProjectName)))
                    yield return solutionProject;
        }

    }
}
