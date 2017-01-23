using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace TagManager
{
    public static class TagMethods
    {
        public static void ApplyEntities(List<TagNode> _entities, List<uint> _objects)
        {
            
            foreach (TagNode _entity in _entities)
            {
                // Check to make sure there is no double.
                _entity.Nodes.AddRange(_objects, true);
            }
            
        }
        public static void SelectEntities(List<TagNode> _entities)
        {
            TagGlobals.internalSelectionSwitch = true;
            List<uint> objectsToSelect = _entities.SelectMany(x => x.Nodes).ToList();
            MaxPluginUtilities.SetSelection(objectsToSelect);
        }
        public static SortableObservableCollection<TagNode> GetEntitiesContainingObjects(List<uint> _objects)
        {
            SortableObservableCollection<TagNode> nodes = new SortableObservableCollection<TagNode>();
            nodes.AddRange(TagGlobals.root.GetNodeList().Where(x => x.Nodes.Intersect(_objects).ToList().Count>0));
            return nodes;
        }
        public static void RemoveObjects(List<TagNode> _entities, List<uint> _objects)
        {
            foreach (TagNode _entity in _entities)
            {
                _entity.Nodes.RemoveRange(_objects);
                if (TagGlobals.autoRename && _entity.IsNameable)
                {
                    RenameUsingStructure(_objects);
                }
            }
        }
        public static void DeleteEntities(List<TagNode> _entities)
        {
            List<uint> _objects = new List<uint>();
            for (int i = 0; i < _entities.Count; i++)
            {
                _objects.AddRange(_entities[i].Nodes);
                _entities[i].Parent.Children.Remove(_entities[i]);
                if (TagGlobals.autoRename)
                {
                    if (_entities[i].IsNameable)
                    {
                        RenameUsingStructure(_entities[i].Nodes.ToList());
                    }
                }
            }
            
        }
        public static void MergeEntities(TagNode _toMerge, TagNode _target)
        {
            _toMerge.Nodes.AddRange(_target.Nodes);
            _target.Parent.Children.Remove(_target);
        }
        public static void RenameUsingStructure(List<uint> _objects)
        {
            SortableObservableCollection<TagNode> entitiesContainingObjects = GetEntitiesContainingObjects(_objects);

            // !!!!!!! Retrieving The entities Of all the objects even if the particular object doesn't use the entity

            SortableObservableCollection<TagNode> nameableEntitiesContainingObjects = entitiesContainingObjects.Where(x => x.IsNameable == true).ToSortableObservableCollection();
            SortableObservableCollection<List<string>> branchesElementsNames = new SortableObservableCollection<List<string>>();
            foreach (TagNode _entity in nameableEntitiesContainingObjects)
            {
                branchesElementsNames.Add(_entity.GetNodeBranchElementsNames(true));
            }
            branchesElementsNames.Sort(x => x.Count);
            List<List<string>> _listBranchElements = branchesElementsNames.Reverse().ToList();
            string _namePrefix = TagHelperMethods.ConcateneNameFromElements(_listBranchElements.SelectMany(x=> x).ToList());
            if (_namePrefix != "")
            {
                foreach (uint _nodeHandle in _objects)
                {
                    IINode _object = MaxPluginUtilities.GetNodeByHandle(_nodeHandle);
                    if (_object.Name.Length > 4)
                    {
                        string _objectPrefix = _object.Name.Substring(0, _object.Name.Length - 4);
                        if (_objectPrefix != _namePrefix)
                        {
                            string finalName = MaxPluginUtilities.MakeNameUnique(_namePrefix + TagGlobals.delimiter);
                            _object.Name = finalName;
                            _object.NotifyNameChanged();
                        }
                    }
                }
            }
        }
        public static void RenameUsingStructure()
        {
            RenameUsingStructure(MaxPluginUtilities.Selection.ToListHandles());
        }
        public static void SelectEntityHoldingObject()
        {
            if (TagGlobals.selectionChain.Count == 0)
            {
                SelectEntityHoldingObject(MaxPluginUtilities.Selection.ToListHandles());
            }
            else
            {
                GrowEntity();
            }
        }
        public static void SelectEntityHoldingObject(List<uint> _nodeHandles)
        {
            List<TagNode> _currentEntities = GetEntitiesContainingObjects(_nodeHandles).ToList();
            TagGlobals.selectionChain.Push(_currentEntities);
            SelectEntities(_currentEntities);
        }
        public static void SelectParentEntityHoldingObject()
        {
            SelectParentEntityHoldingObject(MaxPluginUtilities.Selection.ToListHandles());
        }
        public static void SelectParentEntityHoldingObject(List<uint> _nodeHandles)
        {
            List<TagNode> _parentNodeList = GetEntitiesContainingObjects(_nodeHandles).ToList().SelectMany(x => x.Parent.GetNodeList()).ToList();
            SelectEntities(_parentNodeList);
        }
        public static void GrowEntity()
        {
            List<TagNode> _currentEntities = TagGlobals.selectionChain.First();
            List<TagNode> _parents = new List<TagNode>();
            foreach (TagNode _node in _currentEntities)
            {
                if (_node.Parent != null)
                {
                    _parents.Add(_node.Parent);
                    _parents.AddRange(_node.Parent.GetNodeList());
                }
            }
            _parents.AddRange(_currentEntities);
            TagGlobals.selectionChain.Push(_parents.Distinct().ToList());

            SelectEntities(TagGlobals.selectionChain.First());
        }
        public static void ShrinkEntity()
        {
            if (TagGlobals.selectionChain.Count>1)
            {
                List<TagNode> _toRemove = TagGlobals.selectionChain.Pop();
            }
            SelectEntities(TagGlobals.selectionChain.First());
        }
        public static void RemoveObjectsFromEntities()
        {
            List<TagNode> _currentEntities = GetEntitiesContainingObjects(MaxPluginUtilities.Selection.ToListHandles()).ToList();
            RemoveObjects(_currentEntities, MaxPluginUtilities.Selection.ToListHandles());
        }
    }
}
