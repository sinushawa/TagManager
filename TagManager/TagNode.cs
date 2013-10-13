using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace TagManager
{
    public class TagNode: DDNode
    {
        private Guid _ID;

        public Guid ID
        {
          get { return _ID; }
          set { _ID = value; }
        }
        private SortableObservableCollection<IINode> nodes;
        public SortableObservableCollection<IINode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        public SortableObservableCollection<string> shortcuts;
        public System.Drawing.Color wireColor;

        public TagNode()
        {
        }
        public TagNode(string _label) : this(Guid.NewGuid(), _label, new List<IINode>())
        {
        }
        public TagNode(string _label, List<IINode> _objects) : this(Guid.NewGuid(), _label, _objects)
        {
        }
        public TagNode(Guid _ID, string _label, List<IINode> _objects)
        {
            ID = _ID;
            Name = _label;
            nodes = new SortableObservableCollection<IINode>();
            nodes.AddRange(_objects);
            AllowDrag = true;
            AllowDrop = true;
        }
    }
}
