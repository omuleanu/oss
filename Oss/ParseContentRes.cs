using System.Collections.Generic;

namespace Oss
{
    public class ParseContentRes
    {
        public IEnumerable<string> Inherits { get; set; }

        public string Content { get; set; }

        public string RawContent { get; set; }

        public bool IsRaw { get; set; }
    }
}