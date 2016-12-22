using System.Linq;
using FluentAssertions;
using NSubstitute;
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
        public void Rollback_Should_DoNothing_When_NoWayBackWasAsked()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                unitOfWork.Configuration.ShouldLeaveNoWayBack = true;
                userInteraction.AskQuestion(Arg.Any<string>()).Returns(true);

                subject.Switch(unitOfWork.SolutionFileFullPath);
                subject.Rollback(unitOfWork.SolutionFileFullPath);

                VerifyRollbackHasNotBeenDone(unitOfWork);
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
                projectWithMultipleProjectsToSwitch.GetFileReference("Project1").Should().NotBeNull();
                projectWithMultipleProjectsToSwitch.GetFileReference("Project2").Should().NotBeNull();
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
                project2.GetFileReference("Project4").Should().NotBeNull();
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
                var reference = project2.GetFileReference("Project1");
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
        public void Switch_Should_AskForConfirmation_When_NoWayBackIsAsked()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                unitOfWork.Configuration.ShouldLeaveNoWayBack = true;

                subject.Switch(unitOfWork.SolutionFileFullPath);

                userInteraction.Received().AskQuestion(Arg.Is<string>(x => x.ContainsAll("rollback", "possib")));
            }
        }

        [Test]
        public void Switch_Should_RemoveFileReferences_When_NoWayBackIsAsked()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                unitOfWork.Configuration.ShouldLeaveNoWayBack = true;
                userInteraction.AskQuestion(Arg.Any<string>()).Returns(true);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project = unitOfWork.GetProject3();
                project.GetProjectReference("Project1").Should().NotBeNull();
                project.GetProjectReference("Project2").Should().NotBeNull();
                project.GetFileReference("Project1").Should().BeNull();
                project.GetFileReference("Project2").Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_DoNothing_When_NoWayBackIsAskedButRefused()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                unitOfWork.Configuration.ShouldLeaveNoWayBack = true;
                userInteraction.AskQuestion(Arg.Is<string>(x => x.ToLower().ContainsAll("rollback", "possib"))).Returns(false);

                subject.Switch(unitOfWork.SolutionFileFullPath);

                VerifySwitchHasNotBeenDone(unitOfWork);
                userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.ToLower().ContainsAll("try", "again")));
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
                projectWithMultipleProjectsToSwitch.GetFileReference("Project1").Should().BeNull();
                projectWithMultipleProjectsToSwitch.GetFileReference("Project2").Should().BeNull();
            }
        }

        [Test]
        public void Switch_Should_IgnoreSkipProjects()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                unitOfWork.Configuration.ProjectSkipPatterns.Add("skipPedProJ");

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var skippedProject = unitOfWork.GetSkippedProject();
                skippedProject.GetFileReference("Project1").Should().NotBeNull("This project has not been switched");
            }
        }

        [Test]
        public void Switch_Should_IgnoreReference_When_ProjectIgnorePatternsAreSet()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var subject = new SolutionReferencesSwitcher(unitOfWork.Configuration);

                unitOfWork.Configuration.ReferenceIgnorePatterns.Add("ject1");

                subject.Switch(unitOfWork.SolutionFileFullPath);

                var project2 = unitOfWork.GetProject3();
                project2.GetFileReference("Project1").Should().NotBeNull("because the project1 should have been ignored");
                project2.GetFileReference("Project2").Should().BeNull("because the project2 should NOT have been ignored");
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
                var reference = project2.GetFileReference("Project1");
                reference.Should().BeNull();
            }
        }

        private static void VerifyRollbackHasNotBeenDone(UnitOfWork unitOfWork)
        {
            var project2 = unitOfWork.GetProject2();
            var reference = project2.GetProjectReference("Project1");
            reference.Should().NotBeNull();
        }

        private static void VerifySwitchHasNotBeenDone(UnitOfWork unitOfWork)
        {
            var project2 = unitOfWork.GetProject2();
            var reference = project2.GetFileReference("Project1");
            reference.Should().NotBeNull();
        }
    }
}