using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Evaluation;
using Microsoft.VisualBasic.FileIO;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    /// <summary>
    ///     Isolates testing concepts from each unit test
    /// </summary>
    internal class UnitOfWork : IDisposable
    {
        private const string DirectoryWithFilesForTesting = "FilesForTesting";
        private readonly string _path;
        private readonly ProjectCollection _projects = new ProjectCollection();

        public UnitOfWork()
        {
            _path = CurrentDir.PathCombine(Guid.NewGuid().ToString());
            Directory.CreateDirectory(_path);
            FileSystem.CopyDirectory(CurrentDir.PathCombine(DirectoryWithFilesForTesting), _path);
        }

        public string SolutionFileFullPath
        {
            get { return _path.PathCombine("SolutionFile.sln"); }
        }

        private static string CurrentDir
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public Configuration Configuration  => new Configuration() {DatabaseFileName = _path.PathCombine("dbFileName.txt")} ;

        public void Dispose()
        {
            if (_path != CurrentDir)
            {
                RemoveReadOnlyAttributes();
                Directory.Delete(_path, true);
            }
            _projects.UnloadAllProjects();
            _projects.Dispose();
        }

        public Project GetProject1()
        {
            return GetProject("Project1");
        }

        public Project GetProject2()
        {
            return GetProject("Project2");
        }

        public Project GetProject3()
        {
            return GetProject("Project3");
        }

        public string GetXmlForProject3()
        {
            var project = GetProject3();
            var xml = project.Xml.RawXml;
            _projects.UnloadProject(project);
            return xml;
        }

        public void SetProject2AsReadOnly()
        {
            File.SetAttributes(GetProjectPath("Project2"), FileAttributes.ReadOnly);
        }

        private Project GetProject(string projectName)
        {
            return _projects.LoadProject(GetProjectPath(projectName));
        }

        private string GetProjectPath(string projectName)
        {
            return _path.PathCombine(projectName, $"{projectName}.csproj");
        }

        private void RemoveReadOnlyAttributes()
        {
            for (int i = 1; i <= 3; i++)
                File.SetAttributes(GetProjectPath($"Project{i}"), FileAttributes.Normal);
        }

        public bool Project2IsReadOnly()
        {
            return (File.GetAttributes(GetProjectPath("Project2")) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        }
    }
}