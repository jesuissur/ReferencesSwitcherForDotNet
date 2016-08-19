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
        [Test]
        public void Rollback_Should_ChangeBackProjectFileToPreviousContentWithoutAnyChange()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                var projectXml = unitOfWork.GetXmlForProject3();
                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var projectXmlAfterRollback = unitOfWork.GetXmlForProject3();
                projectXmlAfterRollback.Should().Be(projectXml);
            }
        }

        [Test]
        public void Rollback_Should_HandleMultipleProjects()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var projectWithMultipleProjectsToSwitch = unitOfWork.GetProject3();
                GetProjectReference(projectWithMultipleProjectsToSwitch, "Project1").Should().BeNull();
                GetProjectReference(projectWithMultipleProjectsToSwitch, "Project2").Should().BeNull();
                GetReference(projectWithMultipleProjectsToSwitch, "Project1").Should().NotBeNull();
                GetReference(projectWithMultipleProjectsToSwitch, "Project2").Should().NotBeNull();
            }
        }

        [Test]
        public void Rollback_Should_LeaveExistingReferencesAndProjectReferences()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                GetReference(project2, "Project4").Should().NotBeNull();
                GetProjectReference(project2, "Project9").Should().NotBeNull();
            }
        }

        [Test]
        public void Rollback_Should_PutBackPreviousReferences()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                var reference = GetReference(project2, "Project1");
                var projectReference = GetProjectReference(project2, "Project1");
                reference.Should().NotBeNull();
                projectReference.Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project1 = unitOfWork.GetProject1();
                var project2 = unitOfWork.GetProject2();
                var projectReference = GetProjectReference(project2, "Project1");
                projectReference.Should().NotBeNull();
                projectReference.GetMetadataValue("Project").Should().Be(project1.Properties.First(x => x.Name == "ProjectGuid").EvaluatedValue);
                projectReference.GetMetadataValue("Name").Should().Be(project1.Properties.First(x => x.Name == "ProjectName").EvaluatedValue);
            }
        }

        [Test]
        public void Switch_Should_HandleMultipleProjects()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var projectWithMultipleProjectsToSwitch = unitOfWork.GetProject3();
                Console.WriteLine(projectWithMultipleProjectsToSwitch.Xml.RawXml);
                GetProjectReference(projectWithMultipleProjectsToSwitch, "Project1").Should().NotBeNull();
                GetProjectReference(projectWithMultipleProjectsToSwitch, "Project2").Should().NotBeNull();
                GetReference(projectWithMultipleProjectsToSwitch, "Project1").Should().BeNull();
                GetReference(projectWithMultipleProjectsToSwitch, "Project2").Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_RemoveReferenceToProject_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                Console.WriteLine(project2.Xml.RawXml);
                var reference = GetReference(project2, "Project1");
                reference.Should().BeNull();
                Console.WriteLine(project2.Xml.RawXml);
            }
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
        ///     Isolates testing concepts from each unit test
        /// </summary>
        private class UnitOfWork : IDisposable
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

            public void Dispose()
            {
                if (_path != CurrentDir)
                    Directory.Delete(_path, true);
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

            private Project GetProject(string projectName)
            {
                return _projects.LoadProject(_path.PathCombine(projectName, "{0}.csproj".FormatWith(projectName)));
            }
        }
    }
}