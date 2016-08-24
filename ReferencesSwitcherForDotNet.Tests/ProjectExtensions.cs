using System.Linq;
using Microsoft.Build.Evaluation;
using ReferencesSwitcherForDotNet.Library;

namespace ReferencesSwitcherForDotNet.Tests
{
    internal static class ProjectExtensions
    {
        public static ProjectItem GetProjectReference(this Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => (x.ItemType == "ProjectReference") &&
                                                     (x.GetEvaluatedIncludeForProjectShortName() == string.Format(@"..\{0}\{0}.csproj", projectName)));
        }

        public static ProjectItem GetReference(this Project project, string projectName)
        {
            return project.Items.FirstOrDefault(x => (x.ItemType == "Reference") &&
                                                     (x.GetEvaluatedIncludeForProjectShortName() == projectName));
        }
    }
}