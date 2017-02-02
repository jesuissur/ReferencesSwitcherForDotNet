using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using ReferencesSwitcherForDotNet.Library.Extensions;
using ReferencesSwitcherForDotNet.Library.Internal;

namespace ReferencesSwitcherForDotNet.Library
{
    internal class SwitchMissingProjectReferences
    {
        private readonly Configuration _config;
        private readonly IUserInteraction _userInteraction;
        private Projects _projects;
        private string _defaultFileReferencesDirectory;

        public SwitchMissingProjectReferences(IUserInteraction userInteraction, Configuration config)
        {
            _userInteraction = userInteraction;
            _config = config;
            _defaultFileReferencesDirectory = GetDefaultFileReferencesDirectory();
        }

        public void Switch(string solutionFullPath)
        {
            using (_projects = new Projects())
            {
                var solution = SolutionFile.Parse(solutionFullPath);
                var existingSolutionProjects = solution.GetExistingProjects(_config).ToList();
                existingSolutionProjects.ForEach(x => Switch(x, solution.GetProjects(_config).ToList()));
            }
        }

        private void AddFileReference(string projectName, Project project)
        {
            var metadata = new List<KeyValuePair<string, string>>
                           {
                               new KeyValuePair<string, string>("HintPath", GetRelativeHintPath(projectName, project)),
                               new KeyValuePair<string, string>("SpecificVersion", false.ToString())
                           };
            project.AddItem("Reference", projectName, metadata);
        }

        private string GetDefaultFileReferencesDirectory()
        {
            var dir = _config.FileReferencesDefaultDirectory;
            while (dir.IsNullOrWhiteSpace())
                dir = _userInteraction.AskQuestion("What is the directory where file references are going to be?");
            return dir;
        }

        private string GetRelativeHintPath(string projectName, Project project)
        {
            var defaultFileReferencesDirectory = RelativePath.For(project.FullPath, _defaultFileReferencesDirectory);
            return Path.Combine(defaultFileReferencesDirectory, projectName.ConcatWith(".dll"));
        }

        private void Switch(ProjectInSolution solutionProject, IList<ProjectInSolution> solutionProjects)
        {
            var project = _projects.LoadProject(solutionProject.AbsolutePath);
            var projectReferences = project.GetItems("ProjectReference");
            foreach (var projectReference in projectReferences.ToList())
            {
                var projectName = projectReference.GetMetadataValue("Name");
                if (solutionProjects.All(x => x.ProjectName != projectName))
                {
                    AddFileReference(projectName, project);
                    project.RemoveItem(projectReference);
                }
            }
            project.Save();
        }
    }
}