using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Controls.DragNDrop;

namespace TagManager
{
    [Serializable]
    public class TagNode: DDNode<TagNode>, ISerializable
    {
        private Guid _ID;

        public Guid ID
        {
          get { return _ID; }
          set { _ID = value; }
        }
        public SortableObservableCollection<uint> Nodes
        {
            get;
            set;
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
            Nodes = new SortableObservableCollection<uint>();
            Nodes.AddRange(_objects);
            AllowDrag = true;
            AllowDrop = true;
        }
        public override void OnDrop(object obj)
        {
            DragContent content = obj as DragContent;
            if (content != null)
            {
                foreach (var item in content.Items.Reverse())
                {
                    TagNode oldNode = (TagNode)item;
                    if (oldNode != this && oldNode.Name != this.Name)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        Children.AddRange(new List<TagNode>(){oldNode});
                    }
                    if (oldNode.Name == this.Name)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        this.Nodes.AddRange(oldNode.Nodes);
                    }
                }
            }
        }

        #region Serialize     
        protected TagNode(SerializationInfo info, StreamingContext context)
        {
            ID = (Guid)info.GetValue("ID", typeof(Guid));
            Name = (string)info.GetValue("Name", typeof(string));
            // there is two ways to do it either create a new Sortable collection and relink collection changed event or make a temporary collection and reset the existing one and add the temporary collection to the original one.
            Children = (SortableObservableCollection<TagNode>)info.GetValue("Children", typeof(SortableObservableCollection<TagNode>));
            Children.CollectionChanged+= Children_CollectionChanged;
            Nodes = (SortableObservableCollection<uint>)info.GetValue("Nodes", typeof(SortableObservableCollection<uint>));
            shortcuts = (SortableObservableCollection<string>)info.GetValue("shortcuts", typeof(SortableObservableCollection<string>));
            wireColor = (System.Drawing.Color)info.GetValue("wireColor", typeof(System.Drawing.Color));
            AllowDrag = true;
            AllowDrop = true;
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID, typeof(Guid));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Children", Children, typeof(SortableObservableCollection<TagNode>));
            info.AddValue("Nodes", Nodes, typeof(SortableObservableCollection<uint>));
            info.AddValue("shortcuts", shortcuts, typeof(SortableObservableCollection<string>));
            info.AddValue("wireColor", wireColor, typeof(System.Drawing.Color));
        }
        internal void ReParent()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ReParent();
                Children[i].Parent = this;
            }
        }
        #endregion
    }
}
