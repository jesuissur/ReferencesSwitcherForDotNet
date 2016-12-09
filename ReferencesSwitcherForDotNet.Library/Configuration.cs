using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReferencesSwitcherForDotNet.Library
{
    public class Configuration
    {
        public string DatabaseFileName { get; set; } = Path.GetTempPath().PathCombine("ReferencesSwitcherForDotNet.txt");
        public List<string> ProjectSkipPatterns { get; } = new List<string>();
        public List<string> ReferenceIgnorePatterns { get; } = new List<string>();
        public bool ShouldAskForReadonlyOverwrite { get; set; } = true;
        public bool ShouldLeaveNoWayBack { get; set; } = false;

        public bool ProjectNameShouldNotBeSkipped(string projectName)
        {
            return !ProjectSkipPatterns.Any(pattern => projectName.ToLower().Contains(pattern.ToLower()));
        }

        public bool ReferenceNameShouldNotBeIgnored(string referenceProjectName)
        {
            return !ReferenceIgnorePatterns.Any(pattern => referenceProjectName.ToLower().Contains(pattern.ToLower()));
        }
    }
}