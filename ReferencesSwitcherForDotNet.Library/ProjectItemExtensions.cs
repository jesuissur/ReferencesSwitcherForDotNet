using System.Linq;
using Microsoft.Build.Evaluation;

namespace ReferencesSwitcherForDotNet.Library
{
    public static class ProjectItemExtensions
    {
        public static string GetEvaluatedIncludeForProjectShortName(this ProjectItem item)
        {
            // We want to ignore the references with full name (ex.: Namespace.Project, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL)
            return item.EvaluatedInclude.Split(",").First();
        }
    }
}