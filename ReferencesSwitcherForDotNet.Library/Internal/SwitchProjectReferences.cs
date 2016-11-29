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
        private readonly Repository _repository;
        private readonly IUserInteraction _userInteraction;
        private Projects _projects;

        public SwitchProjectReferences(IUserInteraction userInteraction, Configuration config)
        {
            _userInteraction = userInteraction;
            _config = config;
            _repository = new Repository(_config);
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

        private void AddProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, Project project)
        {
            var metadata = GetMetadataForProjectReference(matchedSolutionProject);
            var relativePath = GetRelativePathForProjectReference(solutionProject, matchedSolutionProject);
            project.AddItem("ProjectReference", relativePath, metadata);
        }

        private bool AtLeastOneProjectIsReadOnly(List<ProjectInSolution> solutionProjects)
        {
            return solutionProjects.Any(x => FileSystem.IsReadOnly(x.AbsolutePath));
        }

        private void EnsureProjectIsNotReadOnly(Project project)
        {
            if (FileSystem.SetFileAsWritable(project.FullPath))
                _repository.RememberReadonlyStatusForProject(project);
        }

        private IEnumerable<ProjectInSolution> GetExistingProjects(SolutionFile solution)
        {
            foreach (var solutionProject in solution.ProjectsInOrder)
                if (File.Exists(solutionProject.AbsolutePath))
                    yield return solutionProject;
        }

        private List<KeyValuePair<string, string>> GetMetadataForProjectReference(ProjectInSolution matchedSolutionProject)
        {
            return new List<KeyValuePair<string, string>>
                   {
                       new KeyValuePair<string, string>("Project", matchedSolutionProject.ProjectGuid),
                       new KeyValuePair<string, string>("Name", matchedSolutionProject.ProjectName)
                   };
        }

        private IEnumerable<ProjectItem> GetReferences(Project project)
        {
            return project.Items.Where(x => (x.ItemType == "Reference") &&
                                            _config.ProjectNameShouldNotBeIgnored(x.GetEvaluatedIncludeForProjectShortName())).ToList();
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
            foreach (var referenceItem in GetReferences(project))
            {
                var matchedSolutionProject = solution.ProjectsInOrder.FirstOrDefault(x => x.ProjectName == referenceItem.GetEvaluatedIncludeForProjectShortName());
                if (matchedSolutionProject != null)
                    SwitchReferenceWithProjectReference(matchedSolutionProject, solutionProject, ref project, referenceItem);
            }
        }

        private void SwitchReferenceWithProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, ref Project project, ProjectItem referenceItem)
        {
            EnsureProjectIsNotReadOnly(project);
            AddProjectReference(matchedSolutionProject, solutionProject, project);
            project.Save();
            if (_config.ShouldLeaveNoWayBack)
                project.RemoveItem(referenceItem);
            else
                HideReference(ref project, referenceItem);
        }

        private bool UserGiveHisApprobation(List<ProjectInSolution> solutionProjects)
        {
            return UserWantsToOverrideReadonlyFiles(solutionProjects) && UserWantsToBurnBridges();
        }

        private bool UserWantsToBurnBridges()
        {
            if (_config.ShouldLeaveNoWayBack)
                if (!_userInteraction.AskQuestion("Are you sure you want to remove the possibility to rollback your changes?"))
                {
                    _userInteraction.DisplayMessage("The operation has stopped because you want to be able to rollback your changes. Try again with the right configuration.");
                    return false;
                }
            return true;
        }

        private bool UserWantsToOverrideReadonlyFiles(List<ProjectInSolution> solutionProjects)
        {
            if (_config.ShouldAskForReadonlyOverwrite && AtLeastOneProjectIsReadOnly(solutionProjects))
                if (!_userInteraction.AskQuestion("At least one project file is read only.  Do you accept to remove the readonly attribute on those files?"))
                {
                    _userInteraction.DisplayMessage("The operation has stopped. Remove the readonly attribute before trying again.");
                    return false;
                }
            return true;
        }
    }
}