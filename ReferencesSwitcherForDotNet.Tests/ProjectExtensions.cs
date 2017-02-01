using System.Linq;
using Microsoft.Build.Evaluation;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    internal static class ProjectExtensions
    {
        public static ProjectItem GetProjectReference(this Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => x.ItemType == "ProjectReference" &&
                                                     x.DirectMetadata.Any(m=> m.Name == "Name" && m.EvaluatedValue == projectName));
        }

        public static ProjectItem GetFileReference(this Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => (x.ItemType == "Reference") &&
                                                     (x.GetEvaluatedIncludeForProjectShortName() == projectName));
        }
    }
}