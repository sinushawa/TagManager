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
        public static List<uint> GetBranchObjects(TagNode _entity)
        {
            List<TagNode> _entities = _entity.GetNodeList();
            List <uint> sel = _entities.SelectMany(x => x.Nodes).ToList<uint>();
            return sel;
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
            List<TagNode> nodeList = TagGlobals.root.GetNodeList().Where(x=> !TagGlobals.baseNames.Contains(x.Name)).ToList();
            return nodeList.Where(x=> x.LongName== _tag).FirstOrDefault();
        }
        public static List<TagNode> RetrieveEntitiesContainsTag(string _tag)
        {
            TagNode projectEntity = TagGlobals.root.GetNodeList().First(x => x.Name == "Project");
            List<TagNode> nodesList = projectEntity.Children.ToList().GetNodeList();
            List<TagNode> result = nodesList.Where(x => x.Name.Contains(_tag)).ToList();
            return result;
        }
        public static TagNode GetLonguestMatchingTag(string _tag, bool _appendMissingTags, bool? _nameable)
        {
            List<string> tagElements = EntityNamesFromBranch(_tag);
            Queue<string> queuedElements = new Queue<string>(tagElements);
            List<string> matchingList = new List<string>();
            TagNode matchingEntity = null;
            bool unexisting = false;
            while (queuedElements.Count > 0 && unexisting==false)
            {
                matchingList.Add(queuedElements.Peek());
                string tagToCheck = ConcateneNameFromElements(matchingList);
                TagNode retrievedEntity = RetrieveEntityFromTag(tagToCheck);
                if (retrievedEntity != null)
                {
                    matchingEntity = retrievedEntity;
                    queuedElements.Dequeue();
                }
                else
                {
                    unexisting = true;
                }
            }
            if (matchingEntity == null)
            {
                matchingEntity = TagGlobals.root.GetNodeList().First(x => x.Name == "Project");
            }
            if (_appendMissingTags)
            {
                while (queuedElements.Count > 0)
                {
                    TagNode _newlyCreated;
                    if (_nameable != null)
                    {
                        bool _nameableNN = _nameable ?? default(bool);
                        _newlyCreated = new TagNode(queuedElements.Dequeue(), _nameableNN);
                    }
                    else
                    {
                        _newlyCreated = new TagNode(queuedElements.Dequeue(), matchingEntity.IsNameable);
                    }
                    matchingEntity.Children.Add(_newlyCreated);
                    matchingEntity.Children.Sort(x => x.Name);
                    matchingEntity = _newlyCreated;
                }
            }
            
            TagGlobals.tagCenter.fastPan.SortSource();

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
        public static void MergeEntities(TagNode _toMerge, TagNode _target)
        {
            if (_toMerge != _target && _toMerge.Name == _target.Name)
            {
                _target.Nodes.AddRange(_toMerge.Nodes);
                if (_target.IsNameable)
                {
                    foreach (uint _nodeHandle in _toMerge.Nodes)
                    {
                        Autodesk.Max.IINode _object = MaxPluginUtilities.GetNodeByHandle(_nodeHandle);
                        _object.RenameNode(_target.LongName);
                    }
                }
                List<List<TagNode>> _matchToMerge = new List<List<TagNode>>();
                foreach (TagNode _potentialMerge in _toMerge.Children)
                {
                    TagNode _potentialTarget = _target.Children.Where(x => x.Name == _potentialMerge.Name).FirstOrDefault();
                    if (_potentialTarget != null)
                    {
                        _matchToMerge.Add(new List<TagNode>() { _potentialMerge, _potentialTarget });
                        
                    }
                }
                // needed to be able to delete nodes children without disturbing the loop
                foreach (List<TagNode> _match in _matchToMerge)
                {
                    MergeEntities(_match[0], _match[1]);
                }
            }
            if (_toMerge.Children.Count > 0)
            {
                _target.Children.AddRange(_toMerge.Children);
            }
            _toMerge.Parent.Children.Remove(_toMerge);
        }
    }
}
