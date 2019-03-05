using System.Collections.Generic;

namespace Oss
{
    public class Rule
    {
        public string Name { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }

        public bool IsComment { get; set; }

        public bool IsEmpty { get; set; }

        public IEnumerable<string> Inherits { get; set; }

        public bool EmptyHeader { get; set; }
    }
}