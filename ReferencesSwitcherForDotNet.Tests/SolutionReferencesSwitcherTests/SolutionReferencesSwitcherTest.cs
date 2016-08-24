using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests.SolutionReferencesSwitcherTests
{
    [TestFixture]
    public class SolutionReferencesSwitcherTest
    {
        [Test]
        public void Rollback_Should_ChangeBackProjectFileToPreviousContentWithoutAnyChange()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

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
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var projectWithMultipleProjectsToSwitch = unitOfWork.GetProject3();
                projectWithMultipleProjectsToSwitch.GetProjectReference("Project1").Should().BeNull();
                projectWithMultipleProjectsToSwitch.GetProjectReference("Project2").Should().BeNull();
                projectWithMultipleProjectsToSwitch.GetReference("Project1").Should().NotBeNull();
                projectWithMultipleProjectsToSwitch.GetReference("Project2").Should().NotBeNull();
            }
        }

        [Test]
        public void Rollback_Should_LeaveExistingReferencesAndProjectReferences()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                project2.GetReference("Project4").Should().NotBeNull();
                project2.GetProjectReference("Project9").Should().NotBeNull();
            }
        }

        [Test]
        public void Rollback_Should_PutBackPreviousReferences()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);
                subject.Rollback(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                var reference = project2.GetReference("Project1");
                var projectReference = project2.GetProjectReference("Project1");
                reference.Should().NotBeNull();
                projectReference.Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_AddProjectReference_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project1 = unitOfWork.GetProject1();
                var project2 = unitOfWork.GetProject2();
                var projectReference = project2.GetProjectReference("Project1");
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
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var projectWithMultipleProjectsToSwitch = unitOfWork.GetProject3();
                projectWithMultipleProjectsToSwitch.GetProjectReference("Project1").Should().NotBeNull();
                projectWithMultipleProjectsToSwitch.GetProjectReference("Project2").Should().NotBeNull();
                projectWithMultipleProjectsToSwitch.GetReference("Project1").Should().BeNull();
                projectWithMultipleProjectsToSwitch.GetReference("Project2").Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_IgnoreReference_When_ProjectIgnorePatternsAreSet()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                unitOfWork.Configuration.ProjectNameIgnorePatterns.Add("ject1");

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject3();
                project2.GetReference("Project1").Should().NotBeNull("because the project1 should have been ignored");
                project2.GetReference("Project2").Should().BeNull("because the project2 should NOT have been ignored");
            }
        }

        [Test]
        public void Switch_Should_RemoveReferenceToProject_When_ReferenceExistsInSolutionForThisProjectOutput()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject2();
                var reference = project2.GetReference("Project1");
                reference.Should().BeNull();
            }
        }
    }
}