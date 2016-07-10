using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    [TestFixture]
    public class SolutionReferencesSwitcherTest
    {
        [SetUp]
        public void InitializeTest()
        {
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }

        [Test]
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            var subject = new SolutionReferencesSwitcher();

            AssertProject2HasReferenceToProject1();

            subject.Switch(GetSolutionFileFullPath());

            var project2 = GetProject2();
            var projectReference = project2.Items.FirstOrDefault(x => x.ItemType == "ProjectReference");
            projectReference.Should().NotBeNull();
            projectReference.EvaluatedInclude.Should().Be(@"..\Project1\Project1.csproj");
        }
        
        private void AssertProject2HasReferenceToProject1()
        {
            var project2 = GetProject2();
            project2.Items.Should().Contain(x => x.ItemType == "Reference" && x.EvaluatedInclude == "Project1");
            ProjectCollection.GlobalProjectCollection.UnloadProject(project2);
        }

        private Project GetProject2()
        {
            return new Project(CurrentDir.PathCombine("FilesForTesting", "Project2", "Project2.csproj"));
        }

        private string GetSolutionFileFullPath()
        {
            return CurrentDir.PathCombine("FilesForTesting", "SolutionFile.sln");
        }

        private static string CurrentDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
