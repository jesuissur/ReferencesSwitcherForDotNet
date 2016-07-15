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
        private static string CurrentDir
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
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

                AssertProject2HasReferenceToProject1(workingDir);

                subject.Switch(GetSolutionFileFullPath(workingDir));

                var project2 = GetProject2(workingDir);
                var projectReference = project2.Items.FirstOrDefault(x => x.ItemType == "ProjectReference");
                projectReference.Should().NotBeNull();
                projectReference.EvaluatedInclude.Should().Be(@"..\Project1\Project1.csproj");
                var project1 = GetProject1(workingDir);
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

                AssertProject2HasReferenceToProject1(workingDir);

                subject.Switch(GetSolutionFileFullPath(workingDir));

                var project2 = GetProject2(workingDir);
                var reference = project2.Items.FirstOrDefault(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
                reference.Should().BeNull();
            }
        }

        private static Project GetProject(string projectName, WorkingDirectory workingDir)
        {
            return new Project(workingDir.Path.PathCombine(projectName, "{0}.csproj".FormatWith(projectName)));
        }

        private void AssertProject2HasReferenceToProject1(WorkingDirectory workingDir)
        {
            var project2 = GetProject2(workingDir);
            project2.Items.Should().Contain(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
            ProjectCollection.GlobalProjectCollection.UnloadProject(project2);
        }

        private Project GetProject1(WorkingDirectory workingDir)
        {
            return GetProject("Project1", workingDir);
        }

        private Project GetProject2(WorkingDirectory workingDir)
        {
            return GetProject("Project2", workingDir);
        }

        private string GetSolutionFileFullPath(WorkingDirectory workingDir)
        {
            return workingDir.Path.PathCombine("SolutionFile.sln");
        }

        private class WorkingDirectory : IDisposable
        {
            public WorkingDirectory()
            {
                Path = CurrentDir.PathCombine(Guid.NewGuid().ToString());
                Directory.CreateDirectory(Path);
                FileSystem.CopyDirectory(CurrentDir.PathCombine("FilesForTesting"), Path);
            }

            public string Path { get; private set; }

            public void Dispose()
            {
                Directory.Delete(Path, true);
            }
        }
    }
}