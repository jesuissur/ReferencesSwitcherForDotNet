using System.Security.Cryptography;
using FileHelpers;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    [DelimitedRecord("|--|")]
    internal class ProjectStatus
    {
        public ProjectStatus()
        {
        }

        public ProjectStatus(Project project)
        {
            var hasher = new SHA256Managed();
            Hash = hasher.ComputeHash(project.Xml.RawXml.ToMemoryStream());
            FullPath = project.FullPath;
            WasReadonly = true;
        }

        public bool WasReadonly { get; set; }

        public string FullPath { get; set; }

        public byte[] Hash { get; set; }
    }
}