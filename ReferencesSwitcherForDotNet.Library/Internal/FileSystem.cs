﻿using System.IO;

namespace ReferencesSwitcherForDotNet.Library.Internal
{
    internal static class FileSystem
    {
        public static bool SetFileAsWritable(string fileName)
        {
            var attributes = File.GetAttributes(fileName);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(fileName, attributes & ~FileAttributes.ReadOnly);
                return true;
            }
            return false;
        }

        public static void SetFileAsReadOnly(string fileName)
        {
            var attributes = File.GetAttributes(fileName);
            File.SetAttributes(fileName, attributes | FileAttributes.ReadOnly);
        }
    }
}