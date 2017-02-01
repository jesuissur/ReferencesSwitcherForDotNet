using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal class QuestionAboutReadOnlyFiles
    {
        private readonly IUserInteraction _userInteraction;
        private readonly Configuration _config;
        private readonly List<ProjectInSolution> _solutionProjects;

        public QuestionAboutReadOnlyFiles(IUserInteraction userInteraction, Configuration config, List<ProjectInSolution> solutionProjects)
        {
            _userInteraction = userInteraction;
            _config = config;
            _solutionProjects = solutionProjects;
        }

        public bool GetAnswer()
        {
            if (_config.ShouldAskForReadonlyOverwrite && AtLeastOneProjectIsReadOnly(_solutionProjects))
                if (!_userInteraction.AskQuestion("At least one project file is read only.  Do you accept to remove the readonly attribute on those files?"))
                {
                    _userInteraction.DisplayMessage("The operation has stopped. Remove the readonly attribute before trying again.");
                    return false;
                }
            return true;
        }

        private bool AtLeastOneProjectIsReadOnly(List<ProjectInSolution> solutionProjects)
        {
            return solutionProjects.Any(x => FileSystem.IsReadOnly(x.AbsolutePath));
        }

    }
}