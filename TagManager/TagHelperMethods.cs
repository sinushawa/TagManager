using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagManager
{
    public static class TagHelperMethods
    {
        public static List<string> EntityNamesFromBranch(string _branchName)
        {
            List<string> _result = _branchName.Split(new char[] { '_' }).ToList();
            return _result;
        }
        public static DDNode FindMatchingEntitiesSequence(DDNode _root, string _branchName)
        {
            FastPan
        }
    }
}
