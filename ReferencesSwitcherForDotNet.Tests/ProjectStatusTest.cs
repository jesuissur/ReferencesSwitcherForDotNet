using FluentAssertions;
using NUnit.Framework;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    [TestFixture]
    public class ProjectStatusTest
    {
        [Test]
        public void Equals_Should_Be_When_ProjectAreIdentical()
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var project = unitOfWork.GetProject1();

                new ProjectStatus(project).Should().Be(new ProjectStatus(project));
            }
        }
    }
}