using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal class SwitchProjectReferences
    {
        private readonly Configuration _config;
        private readonly IUserInteraction _userInteraction;
        private Projects _projects;

        public SwitchProjectReferences(IUserInteraction userInteraction, Configuration config)
        {
            _userInteraction = userInteraction;
            _config = config;
        }

        public void Switch(string solutionFullPath)
        {
            using (_projects = new Projects())
            {
                var solution = SolutionFile.Parse(solutionFullPath);
                var solutionProjects = GetExistingProjects(solution).ToList();
                if (UserGiveHisApprobation(solutionProjects))
                    solutionProjects.ForEach(x => SwitchReferencesForProjectReferences(x, solution));
            }
        }

        private bool UserGiveHisApprobation(List<ProjectInSolution> solutionProjects)
        {
            if (AtLeastOneProjectIsReadOnly(solutionProjects))
                if (!_userInteraction.AskQuestion("At least one project file is read only.  Do you accept to remove the readonly attribute on those files?"))
                {
                    _userInteraction.DisplayMessage("The operation has stopped. Remove the readonly attribute before trying again.");
                    return false;
                }
            return true;
        }

        private bool AtLeastOneProjectIsReadOnly(List<ProjectInSolution> solutionProjects)
        {
            return solutionProjects.Any(x => File.GetAttributes(x.AbsolutePath) == FileAttributes.ReadOnly);
        }

        private IEnumerable<ProjectInSolution> GetExistingProjects(SolutionFile solution)
        {
            foreach (var solutionProject in solution.ProjectsInOrder)
                if (File.Exists(solutionProject.AbsolutePath))
                    yield return solutionProject;
        }

        private void AddProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, Project project)
        {
            var metadata = GetMetadataForProjectReference(matchedSolutionProject);
            var relativePath = GetRelativePathForProjectReference(solutionProject, matchedSolutionProject);
            project.AddItem("ProjectReference", relativePath, metadata);
        }

        private List<KeyValuePair<string, string>> GetMetadataForProjectReference(ProjectInSolution matchedSolutionProject)
        {
            return new List<KeyValuePair<string, string>>
                   {
                       new KeyValuePair<string, string>("Project", matchedSolutionProject.ProjectGuid),
                       new KeyValuePair<string, string>("Name", matchedSolutionProject.ProjectName)
                   };
        }

        private string GetRelativePathForProjectReference(ProjectInSolution solutionProject, ProjectInSolution matchedSolutionProject)
        {
            var uri = new Uri(solutionProject.AbsolutePath);
            var uriRelativePath = uri.MakeRelativeUri(new Uri(matchedSolutionProject.AbsolutePath));
            return uriRelativePath.ToString().Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        private void HideReference(ref Project project, ProjectItem referenceItem)
        {
            var xml = project.Xml.RawXml;
            var hideProjectReferenceRegex = new Regex(@"(?<content><Reference Include=""{0}"".*?</Reference>)".FormatWith(referenceItem.EvaluatedInclude),
                                                      RegexOptions.IgnoreCase | RegexOptions.Singleline);
            xml = hideProjectReferenceRegex.Replace(xml, @"<!-- {0}${{content}}-->".FormatWith(SolutionReferencesSwitcher.Comment));
            _projects.UpdateProject(ref project, xml);
        }

        private void SwitchReferencesForProjectReferences(ProjectInSolution solutionProject, SolutionFile solution)
        {
            var project = _projects.LoadProject(solutionProject.AbsolutePath);
            foreach (var referenceItem in project.Items.Where(x => x.ItemType == "Reference").ToList())
            {
                var matchedSolutionProject = solution.ProjectsInOrder.FirstOrDefault(x => x.ProjectName == referenceItem.EvaluatedInclude);
                if (matchedSolutionProject != null)
                    SwitchReferenceWithProjectReference(matchedSolutionProject, solutionProject, ref project, referenceItem);
            }
        }

        private void SwitchReferenceWithProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, ref Project project, ProjectItem referenceItem)
        {
            EnsureProjectIsNotReadOnly(project);
            AddProjectReference(matchedSolutionProject, solutionProject, project);
            project.Save();
            HideReference(ref project, referenceItem);
        }

        private void EnsureProjectIsNotReadOnly(Project project)
        {
            if (FileSystem.SetFileAsWritable(project.FullPath))
                new Repository(_config).RememberReadonlyStatusForProject(project);
        }
    }
}