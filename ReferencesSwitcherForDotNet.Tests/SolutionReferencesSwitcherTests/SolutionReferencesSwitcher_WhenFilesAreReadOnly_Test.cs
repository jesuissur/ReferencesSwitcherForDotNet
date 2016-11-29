using System.IO;
using FluentAssertions;
using Microsoft.Build.Evaluation;
using NSubstitute;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests.SolutionReferencesSwitcherTests
{
    [TestFixture]
    public class SolutionReferencesSwitcher_WhenFilesAreReadOnly_Test
    {
        [Test]
        public void Rollback_Should_NotSetBackReadOnly_When_OtherChangesHasBeenDone()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                userInteraction.AskQuestion(Arg.Is<string>(x => x.ContainsAll("read", "only"))).Returns(true);
                unitOfWork.SetProject2AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);
                ChangeSomethingInProjectFile(unitOfWork.GetProject2());
                subject.Rollback(unitOfWork.SolutionFileFullPath);

                unitOfWork.Project2IsReadOnly().Should().BeFalse();
            }
        }

        [Test]
        public void Rollback_Should_SetBackReadOnly_When_NoOtherChangesHasBeenDone()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                userInteraction.AskQuestion(Arg.Any<string>()).Returns(true);
                unitOfWork.SetProject2AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);
                subject.Rollback(unitOfWork.SolutionFileFullPath);

                unitOfWork.Project2IsReadOnly().Should().BeTrue();
            }
        }

        [Test]
        public void Rollback_Should_SetBackReadOnly_When_NoOtherChangesHasBeenDoneOnMultipleProjects()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                userInteraction.AskQuestion(Arg.Any<string>()).Returns(true);
                unitOfWork.SetProject2AsReadOnly();
                unitOfWork.SetProject3AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);
                subject.Rollback(unitOfWork.SolutionFileFullPath);

                unitOfWork.Project2IsReadOnly().Should().BeTrue();
                unitOfWork.Project3IsReadOnly().Should().BeTrue();
            }
        }

        [Test]
        public void Switch_Should_DoNothing_When_UserRefuseToRemoveReadOnly()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                userInteraction.AskQuestion(Arg.Is<string>(x => x.ContainsAll("read", "only"))).Returns(false);
                unitOfWork.SetProject2AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.ContainsAll("read", "only", "stop")));
            }
        }

        [Test]
        public void Switch_Should_DoSwitch_When_UserAcceptToRemoveReadOnly()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                userInteraction.AskQuestion(Arg.Is<string>(x => x.ContainsAll("read", "only"))).Returns(true);
                unitOfWork.SetProject2AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                unitOfWork.Project2IsReadOnly().Should().BeFalse();
                VerifySwitchHasBeenDone(unitOfWork);
            }
        }

        [Test]
        public void Switch_Should_DoSwitchWithoutAsking_When_AutoAcceptOptionIsSet()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var userInteraction = Substitute.For<IUserInteraction>();
                var subject = new SolutionReferencesSwitcher(userInteraction, unitOfWork.Configuration);

                unitOfWork.Configuration.ShouldAskForReadonlyOverwrite = false;
                unitOfWork.SetProject2AsReadOnly();

                subject.Switch(unitOfWork.SolutionFileFullPath);

                userInteraction.DidNotReceive().AskQuestion(Arg.Is<string>(x => x.ContainsAll("read", "only")));
                unitOfWork.Project2IsReadOnly().Should().BeFalse();
                VerifySwitchHasBeenDone(unitOfWork);
            }
        }

        private static void VerifySwitchHasBeenDone(UnitOfWork unitOfWork)
        {
            var project2 = unitOfWork.GetProject2();
            var reference = project2.GetReference("Project1");
            reference.Should().BeNull();
        }

        private void ChangeSomethingInProjectFile(Project project)
        {
            var contenu = File.ReadAllText(project.FullPath);
            File.WriteAllText(project.FullPath, contenu.Replace("<WarningLevel>4</WarningLevel>", "<WarningLevel>3</WarningLevel>"));
        }
    }
}