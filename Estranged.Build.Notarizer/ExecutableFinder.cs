using System;
using System.Collections.Generic;
using System.IO;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableFinder
    {
        public IEnumerable<FileInfo> FindExecutables(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (file.Extension == ".dylib")
                {
                    yield return file;
                }

                if (file.Extension == string.Empty)
                {
                    if (ContainsBinary(file))
                    {
                        yield return file;
                    }
                }
            }
        }

        private bool ContainsBinary(FileInfo file)
        {
            using (var fs = file.OpenRead())
            using (var br = new BinaryReader(fs))
            {
                while (fs.Position < fs.Length)
                {
                    try
                    {
                        br.ReadChar();
                    }
                    catch (ArgumentException)
                    {
                        // ReadChar breaks if the input is
                        // out of the UTF-8 range - we're
                        // counting on this, as our executable
                        // will be binary (and trigger the exception)
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
