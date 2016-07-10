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
        [Test]
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            var subject = new SolutionReferencesSwitcher();

            subject.Switch(GetSolutionFileFullPath());

            var project2 = GetProject2();
            project2.Should().NotBeNull();
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
