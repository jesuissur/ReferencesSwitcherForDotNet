using System;
using System.IO;
using System.Text;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal class Projects : IDisposable
    {

        private readonly ProjectCollection _projects = new ProjectCollection();

        public void UpdateProject(ref Project project, string xml)
        {
            xml = xml.Replace(@"encoding=""utf-16""", @"encoding=""utf-8""");
            File.WriteAllText(project.FullPath, xml, Encoding.UTF8);
            project = ReloadProject(project);
        }

        private Project ReloadProject(Project project)
        {
            _projects.UnloadAllProjects();
            return LoadProject(project.FullPath);
        }

        public Project LoadProject(string projectFullPath)
        {
            return _projects.LoadProject(projectFullPath);
        }

        public void Dispose()
        {
            _projects.UnloadAllProjects();
            _projects.Dispose();
        }
    }
}