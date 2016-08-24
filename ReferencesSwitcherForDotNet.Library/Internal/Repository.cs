using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileHelpers;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal class Repository
    {
        private readonly Configuration _config;
        private readonly FileHelperEngine<ProjectStatus> _fileHelper = new FileHelperEngine<ProjectStatus>();
        private readonly List<ProjectStatus> _projects;

        public Repository(Configuration config)
        {
            _config = config;
            _projects = File.Exists(DbFileName) ? _fileHelper.ReadFileAsList(DbFileName) : new List<ProjectStatus>();
        }

        private string DbFileName => _config.DatabaseFileName;

        public bool ContentHasNotChange(Project project)
        {
            var projectStatus = _projects.FirstOrDefault(x => x.FullPath == project.FullPath);
            return (projectStatus != null) && ProjectHasNotChange(project, projectStatus);
        }

        public void RememberReadonlyStatusForProject(Project project)
        {
            _projects.Add(new ProjectStatus(project));
            _fileHelper.WriteFile(DbFileName, _projects);
        }

        public void RemoveReadOnlyStatusForProject(Project project)
        {
            _projects.RemoveWhere(x => x.FullPath == project.FullPath);
            _fileHelper.WriteFile(DbFileName, _projects);
        }

        private static bool ProjectHasNotChange(Project project, ProjectStatus projectStatus)
        {
            return projectStatus.Equals(new ProjectStatus(project));
        }

        public bool ProjectWasReadOnly(Project project)
        {
            return _projects.Any(x => x.FullPath == project.FullPath);
        }
    }
}