using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library
{
    public class SolutionReferencesSwitcher
    {
        public void Switch(string solutionFullPath)
        {
            var solution = SolutionFile.Parse(solutionFullPath);
            foreach (var solutionProject in solution.ProjectsInOrder)
            {
                if (File.Exists(solutionProject.AbsolutePath))
                {
                    var project = new Project(solutionProject.AbsolutePath);
                    var references = project.Items.Where(x=>x.ItemType == "Reference").ToList();
                    foreach (var referenceItem in references)
                    {
                        var matchedSolutionProject = solution.ProjectsInOrder.FirstOrDefault(x => x.ProjectName == referenceItem.EvaluatedInclude);
                        if (matchedSolutionProject != null)
                        {
                            var metadata = new List<KeyValuePair<string, string>>();
                            metadata.Add(new KeyValuePair<string, string>("Project", matchedSolutionProject.ProjectGuid));
                            metadata.Add(new KeyValuePair<string, string>("Name", matchedSolutionProject.ProjectName));
                            var uri = new Uri(solutionProject.AbsolutePath);
                            var relativePath = uri.MakeRelativeUri(new Uri(matchedSolutionProject.AbsolutePath));
                            project.AddItem("ProjectReference", relativePath.ToString().Replace("/", Path.DirectorySeparatorChar.ToString()), metadata);
                            project.RemoveItem(referenceItem);
                            project.Save();
                        }
                    }
                }
            }
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }
    }
}