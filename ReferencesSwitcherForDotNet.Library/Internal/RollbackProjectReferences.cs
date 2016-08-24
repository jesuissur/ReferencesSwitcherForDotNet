using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal class RollbackProjectReferences
    {
        private static readonly Regex HiddenProjectReferencesRegex = new Regex(@"<!-- {0}(?<content>.*?Include=""(?<projectName>.*?)"".*?)-->".FormatWith(SolutionReferencesSwitcher.Comment),
                                                                               RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly Configuration _config;

        private Projects _projects;

        public RollbackProjectReferences(Configuration config)
        {
            _config = config;
        }

        public void Rollback(string solutionFullPath)
        {
            using (_projects = new Projects())
            {
                var solution = SolutionFile.Parse(solutionFullPath);
                RollbackSolutionProjects(solution);
            }
        }

        private bool ProjectHasBeenSwitched(string projectPath, out string xml)
        {
            xml = File.ReadAllText(projectPath, Encoding.UTF8);
            return xml.Contains(SolutionReferencesSwitcher.Comment);
        }

        private void RemoveProjectReferences(List<string> projectReferenceNamesToRemove, Project project)
        {
            foreach (var projectReferenceName in projectReferenceNamesToRemove)
            {
                var projectReference = project.Items.FirstOrDefault(x => (x.ItemType == "ProjectReference") &&
                                                                         x.Metadata.Any(m => (m.Name == "Name") && (m.EvaluatedValue == projectReferenceName)));
                if (projectReference != null)
                    project.RemoveItem(projectReference);
            }
            project.Save();
        }

        private void RollbackProject(string projectPath, string xml)
        {
            var project = _projects.LoadProject(projectPath);
            var projectReferenceNamesToRemove = RollbackReferences(xml, ref project);
            RemoveProjectReferences(projectReferenceNamesToRemove, project);
            EnsureProjectIsSetbackToReadOnlyIfItWas(project);
        }

        private void EnsureProjectIsSetbackToReadOnlyIfItWas(Project project)
        {
            var repository = new Repository(_config);
            if (repository.ProjectWasReadOnlyAndContentHasNotChange(project))
            {
                repository.RemoveReadOnlyStatusForProject(project);
                FileSystem.SetFileAsReadOnly(project.FullPath);
            }
        }

        private List<string> RollbackReferences(string xml, ref Project project)
        {
            var projectReferenceNamesToRemove = new List<string>();

            foreach (Match match in HiddenProjectReferencesRegex.Matches(xml))
                projectReferenceNamesToRemove.Add(match.Groups["projectName"].Value);
            xml = Regex.Replace(xml, HiddenProjectReferencesRegex.ToString(), "${content}", HiddenProjectReferencesRegex.Options);
            _projects.UpdateProject(ref project, xml);
            return projectReferenceNamesToRemove;
        }

        private void RollbackSolutionProjects(SolutionFile solution)
        {
            foreach (var solutionProject in solution.ProjectsInOrder.Where(x => File.Exists(x.AbsolutePath)))
            {
                string xml;
                if (ProjectHasBeenSwitched(solutionProject.AbsolutePath, out xml))
                    RollbackProject(solutionProject.AbsolutePath, xml);
            }
        }
    }
}