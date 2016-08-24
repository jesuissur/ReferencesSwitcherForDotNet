using System.Linq;
using System.Security.Cryptography;
using FileHelpers;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library
{
    [DelimitedRecord("|--|")]
    public class ProjectStatus
    {
        public ProjectStatus()
        {
        }

        public ProjectStatus(Project project) : this(project.Xml.RawXml, project.FullPath)
        {
        }

        public ProjectStatus(string projectXmlContent, string projectFullPath)
        {
            var hasher = new SHA256Managed();
            Hash = hasher.ComputeHash(projectXmlContent.ToMemoryStream());
            FullPath = projectFullPath;
            WasReadonly = true;
        }

        [FieldOrder(2)]
        public string FullPath { get; set; }

        [FieldOrder(3)]
        public byte[] Hash { get; set; }

        [FieldOrder(1)]
        public bool WasReadonly { get; set; }

        public override bool Equals(object obj)
        {
            var other = (ProjectStatus) obj;

            if (other == null) return false;
            return (FullPath == other.FullPath) && Hash.SequenceEqual(other.Hash);
        }
    }
}