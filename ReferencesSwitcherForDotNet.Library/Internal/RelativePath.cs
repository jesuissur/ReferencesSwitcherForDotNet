using System;
using System.IO;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal static class RelativePath
    {
        public static string For(string mainAbsolutePath, string secondaryAbsolutePath)
        {
            var uri = new Uri(mainAbsolutePath);
            secondaryAbsolutePath = EnsurePathEndsWithDirSeparator(secondaryAbsolutePath);
            var uriRelativePath = uri.MakeRelativeUri(new Uri(secondaryAbsolutePath));
            return uriRelativePath.ToString().Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        private static string EnsurePathEndsWithDirSeparator(string secondaryAbsolutePath)
        {
            // When the path does not ends with a /, the relative URI goes back a bit too much
            if (!secondaryAbsolutePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                secondaryAbsolutePath = secondaryAbsolutePath.ConcatWith(Path.DirectorySeparatorChar.ToString());
            return secondaryAbsolutePath;
        }
    }
}