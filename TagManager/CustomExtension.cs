using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TagManager
{
    public static class CustomExtension
    {
        public static List<TagNode> GetNodeList(this TagNode dnod)
        {
            var ret = new List<TagNode>();
            ret.Add(dnod);
            ret.AddRange(dnod.Children.ToList().GetNodeList());
            return ret;
        }
        public static List<TagNode> GetNodeList(this List<TagNode> dnodColl)
        {
            var ret = new List<TagNode>();
            foreach (TagNode _dnode in dnodColl)
            {
                ret.Add(_dnode);
                ret.AddRange(_dnode.Children.ToList().GetNodeList());
            }
            return ret;
        }
        public static IEnumerable<T> IntersectNonEmpty<T>(this IEnumerable<IEnumerable<T>> lists)
        {
            var nonEmptyLists = lists.Where(l => l.Any());
            return nonEmptyLists.Aggregate((l1, l2) => l1.Intersect(l2));
        }
        public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings)
        {
            return groupings.ToDictionary(group => group.Key, group => group.ToList());
        }
        
    }
}
