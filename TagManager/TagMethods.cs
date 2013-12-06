using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.Utilities;

namespace TagManager
{
    public static class TagMethods
    {
        public static void ApplyEntities(List<TagNode> _entities, List<uint> _objects)
        {
            foreach (TagNode _entity in _entities)
            {
                _entity.Nodes.AddRange(_objects);
            }
        }
        public static void SelectEntities(List<TagNode> _entities)
        {
            List<uint> objectsToSelect = _entities.SelectMany(x => x.Nodes).ToList();
            MaxPluginUtilities.SetSelection(objectsToSelect);
        }
        public static SortableObservableCollection<TagNode> GetEntitiesContainingObjects(List<uint> _objects, TagNode _root)
        {
            SortableObservableCollection<TagNode> nodes = new SortableObservableCollection<TagNode>();
            nodes.AddRange(_root.GetNodeList().Where(x => x.Nodes.Intersect(_objects).ToList().Count>0));
            return nodes;
        }
        public static void RemoveObjects(List<TagNode> _entities, List<uint> _objects)
        {
            foreach (TagNode _entity in _entities)
            {
                _entity.Nodes.RemoveRange(_objects);
            }
        }
        public static void DeleteEntities(List<TagNode> _entities)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].Parent.Children.Remove(_entities[i]);
            }
        }
        public static void MergeEntities(TagNode _toMerge, TagNode _target)
        {
            _toMerge.Nodes.AddRange(_target.Nodes);
            _target.Parent.Children.Remove(_target);
        }
        public static void RenameUsingStructure(List<uint> _objects, TagNode _root)
        {
            SortableObservableCollection<TagNode> entitiesContainingObjects = GetEntitiesContainingObjects(_objects, _root);
            SortableObservableCollection<List<string>> branchesElementsNames = new SortableObservableCollection<List<string>>();
            foreach (TagNode _entity in entitiesContainingObjects)
            {
                branchesElementsNames.Add(_entity.GetNodeBranchElementsNames(true));
            }
            branchesElementsNames.Sort(x => x.Count);
            List<List<string>> _listBranchElements = branchesElementsNames.Reverse().ToList();
            string _namePrefix = TagHelperMethods.ConcateneNameFromElements(_listBranchElements.SelectMany(x=> x).ToList());
            
            foreach (uint _nodeHandle in _objects)
            {
                string finalName = MaxPluginUtilities.MakeNameUnique(_namePrefix+TagGlobals.delimiter);
                IINode _object = MaxPluginUtilities.GetNodeByHandle(_nodeHandle);
                _object.Name = finalName;
                _object.NotifyNameChanged();
            }
        }
        public static void RenameUsingStructure(TagNode _root)
        {
            RenameUsingStructure(MaxPluginUtilities.Selection.ToListHandles(), _root);
        }
    }
}
