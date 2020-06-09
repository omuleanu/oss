using System;
using System.Collections.Generic;
using System.Linq;

namespace Oss
{
    public class VarStorg
    {
        private readonly SortedDictionary<string, OssVar> vars = new SortedDictionary<string, OssVar>(new ReverseComparer<string>(StringComparer.InvariantCulture));

        private VarStorg bvars;
        
        public IEnumerable<string> Keys => bvars != null ? vars.Keys.Concat(bvars.Keys) : vars.Keys;

        public bool ContainsKey(string name)
        {
            var res = vars.ContainsKey(name);
            
            if (!res && bvars != null)
            {
                res = bvars.ContainsKey(name);
            }

            return res;
        }

        public OssVar Get(string name)
        {
            if (vars.ContainsKey(name))
            {
                var v = vars[name];
                v.Used = true;
                return v;
            }

            if (bvars != null)
            {
                return bvars.Get(name);
            }

            throw new Exception($"can't find var {name}");
        }

        public void Add(string varname, OssVar osv)
        {
            vars.Add(varname, osv);
        }

        public void SetBase(VarStorg storg)
        {
            if (bvars == null)
            {
                bvars = storg;
            }
            else
            {
                bvars.SetBase(storg);
            }
        }

        public OssVar this[string name] => Get(name);

        public IEnumerable<string> GetUnusedNames()
        {
            return from kv in vars where !kv.Value.Used select kv.Key;
        }
    }
}