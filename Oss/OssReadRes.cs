using System.Collections.Generic;

namespace Oss
{
    public class OssReadRes
    {
        public IDictionary<string, OssItem> Vars { get; set; }

        public IList<OssItem> Items { get; set; }
    }
}