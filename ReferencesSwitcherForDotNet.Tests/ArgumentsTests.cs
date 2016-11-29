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
        }

        [Test]
        public void WithHelp_Should_DisplayHelp()
        {
            new Arguments(_userInteraction, new[] {"-help"}, new Configuration());

            _userInteraction.Received().DisplayMessage(Arg.Any<string>());
        }

        [Test]
        public void WithIgnorePatterns_Should_AddThoseToConfiguration()
        {
            var config = new Configuration();

            new Arguments(_userInteraction, new[] {"-ignorePatterns=Part1,Part2"}, config);

            config.ProjectNameIgnorePatterns.Should().Contain(new[] {"Part1", "Part2"});
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