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
        public static string ConcateneNameFromElements(List<List<string>> _branchElements, string _delimiter)
        {
            string result = "";
            List<string> elems =_branchElements.SelectMany(x=> x).ToList();
            foreach (string _element in elems)
            {
                
                if (result != "")
                {
                    result += _delimiter;
                }
                result += _element;
            }
            return result;
        }
        public static int GetEntityDepth(this TagNode _entity)
        {
            int i = 0;
            while (_entity.Parent != null)
            {
                i++;
                _entity = _entity.Parent;
            }
            return i;
        }
        public static List<TagNode> FindLeavesEntities(TagNode _root)
        {
            SortableObservableCollection<TagNode> allEntities = _root.GetNodeList();
            List<TagNode> _leavesEntites = allEntities.Where(x => x.Parent == null).ToList();
            return _leavesEntites;
        }
        public static int CompareBranches(List<TagNode> _toCompare, List<TagNode> _possibility)
        {
            int _shortestBranchCount = _toCompare.Count < _possibility.Count ? _possibility.Count : _toCompare.Count;
            int _identical = 0;
            for (int i = 0; i < _shortestBranchCount; i++)
            {
                if (_toCompare[i] == _possibility[i])
                {
                    _identical = i;
                }
            }
            return _identical;
        }
        public static TagNode FindMatchingEntitiesSequence(TagNode _root, TagNode _entity)
        {
            List<TagNode> _branchToCompare = _entity.GetNodeBranch();
            List<TagNode> _leavesEntities = FindLeavesEntities(_root);
            List<List<TagNode>> _entitiesSequences = new List<List<TagNode>>();
            List<int> _matchingLength = new List<int>();
            foreach (TagNode _entityLeaf in _leavesEntities)
            {
                List<TagNode> _branchSolution = _entityLeaf.GetNodeBranch();
                _matchingLength.Add(CompareBranches(_branchToCompare, _branchSolution));
                _entitiesSequences.Add(_branchSolution);
            }
            int _longuest = _matchingLength.IndexOf(_matchingLength.Max());
            return _entitiesSequences[_longuest][_matchingLength.Max()];
        }
    }
}
