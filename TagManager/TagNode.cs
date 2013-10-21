using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TagManager
{
    [Serializable]
    public class TagNode: DDNode, ISerializable
    {
        private Guid _ID;

        public Guid ID
        {
          get { return _ID; }
          set { _ID = value; }
        }
        private SortableObservableCollection<uint> nodes;
        public SortableObservableCollection<uint> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
        private bool isInEditMode;
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set { isInEditMode = value; }
        }

        public SortableObservableCollection<string> shortcuts;
        public System.Drawing.Color wireColor;

        public TagNode()
        {
        }
        public TagNode(string _label) : this(Guid.NewGuid(), _label, new List<uint>())
        {
        }
        public TagNode(string _label, List<uint> _objects) : this(Guid.NewGuid(), _label, _objects)
        {
        }
        public TagNode(Guid _ID, string _label, List<uint> _objects)
        {
            ID = _ID;
            Name = _label;
            nodes = new SortableObservableCollection<uint>();
            nodes.AddRange(_objects);
            AllowDrag = true;
            AllowDrop = true;
        }

        #region Serialize
        
        protected TagNode(SerializationInfo info, StreamingContext context)
        {
            ID = (Guid)info.GetValue("ID", typeof(Guid));
            Name = (string)info.GetValue("Name", typeof(string));
            Children = (SortableObservableCollection<DDNode>)info.GetValue("Children", typeof(SortableObservableCollection<DDNode>));
            Nodes = (SortableObservableCollection<uint>)info.GetValue("Nodes", typeof(SortableObservableCollection<uint>));
            shortcuts = (SortableObservableCollection<string>)info.GetValue("shortcuts", typeof(SortableObservableCollection<string>));
            wireColor = (System.Drawing.Color)info.GetValue("wireColor", typeof(System.Drawing.Color));
        }
        #endregion

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID, typeof(Guid));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Children", Children, typeof(SortableObservableCollection<DDNode>));
            info.AddValue("Nodes", Nodes, typeof(SortableObservableCollection<uint>));
            info.AddValue("shortcuts", shortcuts, typeof(SortableObservableCollection<string>));
            info.AddValue("wireColor", wireColor, typeof(System.Drawing.Color));
        }
    }
}
