using System.IO;

namespace ReferencesSwitcherForDotNet.Library
{
    public class Configuration
    {
        public string DatabaseFileName { get; set; } = Path.GetTempPath().PathCombine("ReferencesSwitcherForDotNet.txt");
    }
}