using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;
using ReferencesSwitcherForDotNet.Library.Extensions;
using ReferencesSwitcherForDotNet.Library.Internal;

namespace ReferencesSwitcherForDotNet.Library
{
    internal class SwitchMissingProjectReferences
    {
        private readonly IUserInteraction _userInteraction;
        private readonly Configuration _config;
        private Projects _projects;

        public SwitchMissingProjectReferences(IUserInteraction userInteraction, Configuration config)
        {
            _userInteraction = userInteraction;
            _config = config;
        }

        public void Switch(string solutionFullPath)
        {
            using (_projects = new Projects())
            {
                var solution = SolutionFile.Parse(solutionFullPath);
                var solutionProjects = solution.GetExistingProjects(_config).ToList();
                //if (UserGiveHisApprobation(solutionProjects))
                    solutionProjects.ForEach(x => Switch(x, solution, solutionProjects));
            }
        }

        private void Switch(ProjectInSolution solutionProject, SolutionFile solution, List<ProjectInSolution> solutionProjects)
        {
            var project = _projects.LoadProject(solutionProject.AbsolutePath);
            var projectReferences = project.GetItems("ProjectReference");
            foreach (var projectReference in projectReferences)
            {
                if (solutionProjects.All(x => x.ProjectName != projectReference.GetMetadataValue("Name")))
                {
                    // Add reference with configured expected path for library (what if exe?)
                    // Remove proj ref
                }
            }
        }
    }
}