using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Build.Evaluation;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    [TestFixture]
    public class SolutionReferencesSwitcherTest
    {

        [OneTimeSetUp]
        public static void InitializeTestFixture()
        {
            AssertProject2HasReferenceToProject1();
        }

        [SetUp]
        public void InitializeTest()
        {
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }

        [Test]
        public void Rollback_Should_PutBackPreviousReferences()
        {
            using (var workingDir = new WorkingDirectory())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(workingDir.SolutionFileFullPath);

                subject.Rollback(workingDir.SolutionFileFullPath);

                var project2 = workingDir.GetProject2();
                var reference = GetReference(project2, "Project1");
                var projectReference = GetProjectReference(project2, "Project1");
                reference.Should().NotBeNull();
                projectReference.Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var workingDir = new WorkingDirectory())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(workingDir.SolutionFileFullPath);

                var project1 = workingDir.GetProject1();
                var project2 = workingDir.GetProject2();
                var projectReference = GetProjectReference(project2, "Project1");
                projectReference.Should().NotBeNull();
                projectReference.GetMetadataValue("Project").Should().Be(project1.Properties.First(x => x.Name == "ProjectGuid").EvaluatedValue);
                projectReference.GetMetadataValue("Name").Should().Be(project1.Properties.First(x => x.Name == "ProjectName").EvaluatedValue);
            }
        }

        [Test]
        public void Switch_Should_RemoveReferenceToProject_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var workingDir = new WorkingDirectory())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(workingDir.SolutionFileFullPath);

                var project2 = workingDir.GetProject2();
                Console.WriteLine(project2.Xml.RawXml);
                var reference = GetReference(project2, "Project1");
                reference.Should().BeNull();
                Console.WriteLine(project2.Xml.RawXml);
            }
        }

        private static void AssertProject2HasReferenceToProject1()
        {
            var workingDir = WorkingDirectory.ForTestingFiles();
            var project2 = workingDir.GetProject2();
            project2.Items.Should().Contain(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
            ProjectCollection.GlobalProjectCollection.UnloadProject(project2);
        }

        private static ProjectItem GetProjectReference(Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => x.ItemType == "ProjectReference" && 
                                                     x.EvaluatedInclude == string.Format(@"..\{0}\{0}.csproj", projectName));
        }
        private static ProjectItem GetReference(Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => x.ItemType == "Reference" && 
                                                     x.EvaluatedInclude == projectName);
        }
        /// <summary>
        /// Copy the testing files under a temp folder to isolate those files for each unit test.
        /// </summary>
        private class WorkingDirectory : IDisposable
        {
            private const string DirectoryWithFilesForTesting = "FilesForTesting";
            private readonly string _path;

            public WorkingDirectory()
                : this(Guid.NewGuid().ToString())
            {
            }

            private WorkingDirectory(string uniqueDirName)
            {
                if (uniqueDirName.IsNullOrWhiteSpace())
                    _path = CurrentDir.PathCombine(DirectoryWithFilesForTesting);
                else
                {
                    _path = CurrentDir.PathCombine(uniqueDirName);
                    Directory.CreateDirectory(_path);
                    FileSystem.CopyDirectory(CurrentDir.PathCombine(DirectoryWithFilesForTesting), _path);
                }
        }

            public string SolutionFileFullPath
            {
                get { return _path.PathCombine("SolutionFile.sln"); }
            }

            private static string CurrentDir
            {
                get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
            }

            public static WorkingDirectory ForTestingFiles()
            {
                return new WorkingDirectory(null);
            }

            public void Dispose()
            {
                if (_path != CurrentDir)
                    Directory.Delete(_path, true);
            }

            public Project GetProject1()
            {
                return GetProject("Project1");
            }

            public Project GetProject2()
            {
                return GetProject("Project2");
            }
            private Project GetProject(string projectName)
            {
                return new Project(_path.PathCombine(projectName, "{0}.csproj".FormatWith(projectName)));
            }
        }
    }
}