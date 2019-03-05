using System.Collections.Generic;

namespace Oss
{
    public class OssParseResult
    {
        public int Errors { get; set; }

        public IEnumerable<string> InsertedFiles { get; set; }

        public string ParseRes { get; set;}
    }
}