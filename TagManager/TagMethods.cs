using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static void SelectEntities(List<TagNode> _entities, bool _newSelection)
        {
            List<uint> objectsToSelect = _entities.SelectMany(x => x.Nodes).ToList();
            MaxPluginUtilities.setSelection(objectsToSelect, _newSelection);
        }
        public static SortableObservableCollection<TagNode> GetEntitiesContainingObjects(List<uint> _objects, TagNode _root)
        {
            SortableObservableCollection<TagNode> nodes = new SortableObservableCollection<TagNode>();
            nodes.AddRange(_root.GetNodeList().Cast<TagNode>().Where(x => x.Nodes.Intersect(_objects).ToList().Count>0));
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
    }
}
