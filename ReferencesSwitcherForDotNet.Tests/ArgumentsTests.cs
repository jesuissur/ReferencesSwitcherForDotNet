using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    [TestFixture]
    public class ArgumentsTests
    {
        private readonly IUserInteraction _userInteraction = Substitute.For<IUserInteraction>();

        [Test]
        public void DisplayHelp_Should_DisplayEveryFlags()
        {
            var subject = new Arguments(_userInteraction, new[] {""}, new Configuration());

            subject.DisplayHelp();

            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-switch")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-rollback")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("help")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-s[olutions]")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("ignorePatterns")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-noWayBack")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-acceptReadonlyOverwrite")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-skip")));
            _userInteraction.Received().DisplayMessage(Arg.Is<string>(x => x.Contains("-switchMissingProjRef")));
        }

        [Test]
        public void WithHelp_Should_DisplayHelp()
        {
            new Arguments(_userInteraction, new[] {"-help"}, new Configuration());

            _userInteraction.Received().DisplayMessage(Arg.Any<string>());
        }

        [Test]
        public void AreMissing_Should_BeFalse_When_SwitchOrRollbackIsSetAlongSolutionFile()
        {
            var args = new Arguments(_userInteraction, new[] {"-s=x.sln", "-switch"}, new Configuration());
            args.AreMissing.Should().BeFalse();

            args = new Arguments(_userInteraction, new[] {"-s=x.sln", "-rollback"}, new Configuration());
            args.AreMissing.Should().BeFalse();

            args = new Arguments(_userInteraction, new[] {"-s=x.sln", "-switchMissingProjRef" }, new Configuration());
            args.AreMissing.Should().BeFalse();
        }

        [Test]
        public void AreMissing_Should_BeTrue_When_MissingSolutionFileOrSwitchRollback()
        {
            var args = new Arguments(_userInteraction, new[] {"-rollback", "-switch", "-switchMissingProjRef" }, new Configuration());
            args.AreMissing.Should().BeTrue();

            args = new Arguments(_userInteraction, new[] { "-s=x.sln" }, new Configuration());
            args.AreMissing.Should().BeTrue();
        }

        [Test]
        public void WithIgnorePatterns_Should_AddThoseToConfiguration()
        {
            var config = new Configuration();

            new Arguments(_userInteraction, new[] {"-ignorePatterns=Part1,Part2"}, config);

            config.ReferenceIgnorePatterns.Should().Contain(new[] {"Part1", "Part2"});
        }

        [Test]
        public void WithSkip_Should_AddThoseProjectPatternsToConfiguration()
        {
            var config = new Configuration();

            new Arguments(_userInteraction, new[] {"-skip=ProjectNamePart1,ProjectNamePart2"}, config);

            config.ProjectSkipPatterns.Should().Contain(new[] { "ProjectNamePart1", "ProjectNamePart2" });
        }

        [Test]
        public void WithNoWayBack_Should_SetNoWayBack()
        {
            var config = new Configuration();

            new Arguments(_userInteraction, new[] {"-noWayBack"}, config);

            config.ShouldLeaveNoWayBack.Should().BeTrue();
        }

        [Test]
        public void WithAcceptReadonlyOverwrite_Should_SetShouldAskForReadonlyOverwrite()
        {
            var config = new Configuration();

            config.ShouldAskForReadonlyOverwrite.Should().BeTrue();

            new Arguments(_userInteraction, new[] { "-acceptReadonlyOverwrite" }, config);

            config.ShouldAskForReadonlyOverwrite.Should().BeFalse();
        }

        [Test]
        public void WithRollbackFlag_Should_SetRollback()
        {
            var subject = new Arguments(_userInteraction, new[] {"-rollback"}, new Configuration());

            subject.ShouldRollback.Should().BeTrue();
        }

        [Test]
        public void WithSwitchMissingProjRefFlag_Should_SetSwitchMissingProjectReferences()
        {
            var subject = new Arguments(_userInteraction, new[] { "-switchMissingProjRef" }, new Configuration());

            subject.ShouldSwitchMissingProjectReferences.Should().BeTrue();
        }

        [Test]
        public void WithSolutions_Should_SetSolutionsFullPath()
        {
            var subject = new Arguments(_userInteraction, new[] {@"-solutions=C:\Temp1\Temp1.sln,C:\Temp2\Temp2.sln"}, new Configuration());

            subject.SolutionsFullPath.Should().Contain(new[] {@"C:\Temp1\Temp1.sln", @"C:\Temp2\Temp2.sln"});

            subject = new Arguments(_userInteraction, new[] {@"-s=C:\Temp3\Temp3.sln"}, new Configuration());

            subject.SolutionsFullPath.Should().Contain(@"C:\Temp3\Temp3.sln", "-s or -solutions works the same");
        }

        [Test]
        public void WithSwitchFlag_Should_SetSwitch()
        {
            var subject = new Arguments(_userInteraction, new[] {"-switch"}, new Configuration());

            subject.ShouldSwitch.Should().BeTrue();
        }
    }
}