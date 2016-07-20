using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library
{
    public class SolutionReferencesSwitcher
    {
        private const string Comment = "ReferencesSwitcherForDotNet needs this to rollback changes";

        public void Rollback(string solutionFullPath)
        {
            new RollbackProjectReferences().Rollback(solutionFullPath);
        }

        public void Switch(string solutionFullPath)
        {
            var solution = SolutionFile.Parse(solutionFullPath);
            foreach (var solutionProject in solution.ProjectsInOrder)
            {
                if (File.Exists(solutionProject.AbsolutePath))
                    SwitchReferencesForProjectReferences(solutionProject, solution);
            }
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }

        private static void AddProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, Project project)
        {
            var metadata = GetMetadataForProjectReference(matchedSolutionProject);
            var relativePath = GetRelativePathForProjectReference(solutionProject, matchedSolutionProject);
            project.AddItem("ProjectReference", relativePath, metadata);
        }

        private static List<KeyValuePair<string, string>> GetMetadataForProjectReference(ProjectInSolution matchedSolutionProject)
        {
            return new List<KeyValuePair<string, string>>
                   {
                       new KeyValuePair<string, string>("Project", matchedSolutionProject.ProjectGuid),
                       new KeyValuePair<string, string>("Name", matchedSolutionProject.ProjectName)
                   };
        }

        private static string GetRelativePathForProjectReference(ProjectInSolution solutionProject, ProjectInSolution matchedSolutionProject)
        {
            var uri = new Uri(solutionProject.AbsolutePath);
            var uriRelativePath = uri.MakeRelativeUri(new Uri(matchedSolutionProject.AbsolutePath));
            return uriRelativePath.ToString().Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        private static void HideReference(ref Project project, ProjectItem referenceItem)
        {
            var xml = project.Xml.RawXml;
            var hideProjectReferenceRegex = new Regex(@"(?<content><Reference Include=""{0}"".*?</Reference>)".FormatWith(referenceItem.EvaluatedInclude),
                                                      RegexOptions.IgnoreCase | RegexOptions.Singleline);
            xml = hideProjectReferenceRegex.Replace(xml, @"<!-- {0}${{content}}-->".FormatWith(Comment));
            UpdateProject(ref project, xml);
        }

        private static void SwitchReferencesForProjectReferences(ProjectInSolution solutionProject, SolutionFile solution)
        {
            var project = new Project(solutionProject.AbsolutePath);
            foreach (var referenceItem in project.Items.Where(x => x.ItemType == "Reference").ToList())
            {
                var matchedSolutionProject = solution.ProjectsInOrder.FirstOrDefault(x => x.ProjectName == referenceItem.EvaluatedInclude);
                if (matchedSolutionProject != null)
                    SwitchReferenceWithProjectReference(matchedSolutionProject, solutionProject, project, referenceItem);
            }
        }

        private static void SwitchReferenceWithProjectReference(ProjectInSolution matchedSolutionProject, ProjectInSolution solutionProject, Project project, ProjectItem referenceItem)
        {
            AddProjectReference(matchedSolutionProject, solutionProject, project);
            HideReference(ref project, referenceItem);
            project.Save();
        }

        private static void UpdateProject(ref Project project, string xml)
        {
            xml = xml.Replace(@"encoding=""utf-16""", @"encoding=""utf-8""");
            File.WriteAllText(project.FullPath, xml, Encoding.UTF8);
            ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            ProjectCollection.GlobalProjectCollection.UnloadProject(project.Xml);
            project = new Project(project.FullPath);
        }

        private class RollbackProjectReferences
        {
            private static readonly Regex HiddenProjectReferencesRegex = new Regex(@"<!-- {0}(?<content>.* Include=""(?<projectName>.*)"".*</Reference>).*</Reference>".FormatWith(Comment),
                                                                                   RegexOptions.IgnoreCase | RegexOptions.Singleline);

            public void Rollback(string solutionFullPath)
            {
                var solution = SolutionFile.Parse(solutionFullPath);
                RollbackSolutionProjects(solution);
                ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            }

            private static void RemoveProjectReferences(List<string> projectReferenceNamesToRemove, Project project)
            {
                foreach (var projectReferenceName in projectReferenceNamesToRemove)
                {
                    var projectReference = project.Items.FirstOrDefault(x => x.ItemType == "ProjectReference" &&
                                                                             x.Metadata.Any(m => m.Name == "Name" && m.EvaluatedValue == projectReferenceName));
                    if (projectReference != null)
                        project.RemoveItem(projectReference);
                }
                project.Save();
            }

            private static void RollbackProject(string projectPath, string xml)
            {
                var project = new Project(projectPath);
                var projectReferenceNamesToRemove = RollbackReferences(xml, ref project);
                RemoveProjectReferences(projectReferenceNamesToRemove, project);
            }

            private static List<string> RollbackReferences(string xml, ref Project project)
            {
                var projectReferenceNamesToRemove = new List<string>();

                foreach (Match match in HiddenProjectReferencesRegex.Matches(xml))
                {
                    xml = HiddenProjectReferencesRegex.Replace(xml, "${content}");
                    projectReferenceNamesToRemove.Add(match.Groups["projectName"].Value);
                }
                UpdateProject(ref project, xml);
                return projectReferenceNamesToRemove;
            }

            private bool ProjectHasBeenSwitched(string projectPath, out string xml)
            {
                xml = File.ReadAllText(projectPath, Encoding.UTF8);
                return xml.Contains(Comment);
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
}