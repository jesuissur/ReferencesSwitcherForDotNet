using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReferencesSwitcherForDotNet.Library
{
    public class Configuration
    {
        public string DatabaseFileName { get; set; } = Path.GetTempPath().PathCombine("ReferencesSwitcherForDotNet.txt");
        public List<string> ProjectNameIgnorePatterns { get; } = new List<string>();
        public bool ShouldAskForReadonlyOverwrite { get; set; } = true;
        public bool ShouldLeaveNoWayBack { get; set; } = false;

        public bool ProjectNameShouldNotBeIgnored(string projectName)
        {
            return !ProjectNameIgnorePatterns.Any(pattern => projectName.ToLower().Contains(pattern.ToLower()));
        }
    }
}