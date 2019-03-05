using System.Collections.Generic;
using System.IO;

namespace Oss
{
    public class ParseRes
    {
        public int Errors { get; set; }

        public IEnumerable<FileSystemWatcher> Watchers { get; set; }
    }
}