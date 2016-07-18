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
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var workingDir = new WorkingDirectory())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(workingDir.GetSolutionFileFullPath());

                var project1 = workingDir.GetProject1();
                var project2 = workingDir.GetProject2();
                var projectReference = project2.Items.FirstOrDefault(x => x.ItemType == "ProjectReference");
                projectReference.Should().NotBeNull();
                projectReference.EvaluatedInclude.Should().Be(@"..\Project1\Project1.csproj");
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

                subject.Switch(workingDir.GetSolutionFileFullPath());

                var project2 = workingDir.GetProject2();
                var reference = project2.Items.FirstOrDefault(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
                reference.Should().BeNull();
            }
        }

        private static void AssertProject2HasReferenceToProject1()
        {
            var workingDir = WorkingDirectory.ForTestingFiles();
            var project2 = workingDir.GetProject2();
            project2.Items.Should().Contain(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
            ProjectCollection.GlobalProjectCollection.UnloadProject(project2);
        }

        private class WorkingDirectory : IDisposable
        {
            private readonly string _path;

            public WorkingDirectory()
                : this(Guid.NewGuid().ToString())
            {
            }

            private WorkingDirectory(string uniqueDirName)
            {
                if (uniqueDirName.IsNullOrWhiteSpace())
                    _path = CurrentDir;
                else
                {
                    _path = CurrentDir.PathCombine(uniqueDirName);
                    Directory.CreateDirectory(_path);
                    FileSystem.CopyDirectory(CurrentDir.PathCombine("FilesForTesting"), _path);
                }
        }

            private static string CurrentDir
            {
                get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
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

            public string GetSolutionFileFullPath()
            {
                return _path.PathCombine("SolutionFile.sln");
            }
            private Project GetProject(string projectName)
            {
                return new Project(_path.PathCombine(projectName, "{0}.csproj".FormatWith(projectName)));
            }

            public static WorkingDirectory ForTestingFiles()
            {
                return new WorkingDirectory(null);
            }
        }
    }
}