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

        public Repository(Configuration config)
        {
            _config = config;
        }

        private string DbFileName => _config.DatabaseFileName;

        public void RememberReadonlyStatusForProject(Project project)
        {
            var projectStatusBefore = new ProjectStatus(project);
            _fileHelper.WriteFile(DbFileName, new[] {projectStatusBefore});
        }

        public bool ProjectWasReadOnlyAndContentHasNotChange(Project project)
        {
            if (!File.Exists(DbFileName)) return false;

            var projectStatus = _fileHelper.ReadFileAsList(DbFileName).FirstOrDefault(x => x.FullPath == project.FullPath);
            return (projectStatus != null) && projectStatus.Hash.SequenceEqual(new ProjectStatus(project).Hash);
        }

        public void RemoveReadOnlyStatusForProject(Project project)
        {
            var projects = _fileHelper.ReadFileAsList(DbFileName);
            projects.RemoveWhere(x => x.FullPath == project.FullPath);
            _fileHelper.WriteFile(DbFileName, projects);
        }
    }
}