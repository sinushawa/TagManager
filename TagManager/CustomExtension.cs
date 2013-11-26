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
        public static SortableObservableCollection<TagNode> GetNodeList(this TagNode dnod)
        {
            var ret = new SortableObservableCollection<TagNode>();
            ret.Add(dnod);
            ret.AddRange(dnod.Children);
            return ret;
        }
        public static SortableObservableCollection<TagNode> GetNodeList(this SortableObservableCollection<TagNode> dnodColl)
        {
            var ret = new SortableObservableCollection<TagNode>();
            foreach (TagNode _dnode in dnodColl)
            {
                ret.Add(_dnode);
                ret.AddRange(_dnode.Children.GetNodeList());
            }
            return ret;
        }
        public static List<TagNode> GetNodeBranch(this TagNode _node)
        {
            List<TagNode> _hierarchy = new List<TagNode>();
            _hierarchy.Add(_node);
            while (_node.Parent != null)
            {
                TagNode _parentTag = _node.Parent as TagNode;
                _hierarchy.Add(_parentTag);
                _node = _parentTag;
            }
            _hierarchy.Reverse();
            return _hierarchy;
        }
        public static IEnumerable<T> IntersectNonEmpty<T>(this IEnumerable<IEnumerable<T>> lists)
        {
            var nonEmptyLists = lists.Where(l => l.Any());
            return nonEmptyLists.Aggregate((l1, l2) => l1.Intersect(l2));
        }
    }
}
