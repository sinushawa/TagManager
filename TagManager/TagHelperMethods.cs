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
        public static string ConcateneNameFromElements(IEnumerable<string> _branchElements)
        {
            string result = "";
            foreach (string _element in _branchElements)
            {
                
                if (result != "")
                {
                    result += TagGlobals.delimiter;
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
            List<TagNode> allEntities = _root.GetNodeList();
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
        public static IEnumerable<string> GetAllBranchNames()
        {
            TagNode projectEntity = TagGlobals.root.GetNodeList().First(x => x.Name == "Project");
            List<TagNode> nodesList = projectEntity.Children.ToList().GetNodeList();
            return nodesList.Select(x=> x.LongName);
        }
        public static TagNode RetrieveEntityFromTag(string _tag)
        {
            List<TagNode> nodeList = TagGlobals.root.GetNodeList();
            return nodeList.Where(x=> x.LongName== _tag).FirstOrDefault();
        }
        public static TagNode GetLonguestMatchingTag(string _tag)
        {
            List<string> tagElements = EntityNamesFromBranch(_tag);
            List<string> matchingList = new List<string>();
            TagNode matchingEntity = null;
            foreach (string _element in tagElements)
            {
                matchingList.Add(_element);
                string tagToCheck = ConcateneNameFromElements(matchingList);
                TagNode retrievedEntity = RetrieveEntityFromTag(tagToCheck);
                if (retrievedEntity != null)
                {
                    matchingEntity = retrievedEntity;
                }
            }
            return matchingEntity;
        }
        public static EntityVisibility GetEntityVisibility(this TagNode _tagNode)
        {
            List<bool> nodesHidden = _tagNode.Nodes.Select(x => MaxPluginUtilities.GetNodeHidden(x)).ToList();
            List<bool> entityHidden = nodesHidden.Distinct().ToList();
            if (entityHidden.Count < 2)
            {
                return entityHidden[0] == true ? EntityVisibility.Hidden : EntityVisibility.Vsible;
            }
            else
            {
                return EntityVisibility.Mixed;
            }
        }
    }
}
