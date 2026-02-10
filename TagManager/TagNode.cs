using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls.DragNDrop;

namespace TagManager
{
    /// <summary>
    /// TagNode is the container for entities it inehrites from DDNode
    /// </summary>
    [Serializable]
    public class TagNode : DDNode<TagNode>, ISerializable, INotifyPropertyChanged
    {
        private Guid _ID;

        public Guid ID
        {
          get { return _ID; }
          set { _ID = value; }
        }
        private bool isInEditMode;
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set 
            { 
                isInEditMode = value;
                NotifyPropertyChanged("Nodes");
            }
        }
        private SortableObservableCollection<uint> nodes;
        public SortableObservableCollection<uint> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
        private EntityVisibility visible;
        public EntityVisibility Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        private bool isNameable;
        public bool IsNameable
        {
            get { return isNameable; }
            set { isNameable = value; }
        }
        private bool isShortcut;
        public bool IsShortcut
        {
            get { return isShortcut; }
            set { isShortcut = value; }
        }
        private ConsoleContainerElement shortcut;
        public ConsoleContainerElement Shortcut
        {
            get { return shortcut; }
            set { shortcut = value; }
        }
        public System.Drawing.Color wireColor;

        public TagNode()
        {
        }
        public TagNode(string _label) : this(Guid.NewGuid(), _label, new List<uint>(), false, new ConsoleContainerElement())
        {
        }
        public TagNode(string _label, bool _nameable) : this(Guid.NewGuid(), _label, new List<uint>(), _nameable, false, new ConsoleContainerElement())
        {
        }
        public TagNode(string _label, List<uint> _objects) : this(Guid.NewGuid(), _label, _objects, false, new ConsoleContainerElement())
        {
        }
        public TagNode(string _label, ConsoleContainerElement _shortcut) : this(Guid.NewGuid(), _label, new List<uint>(), false, true, _shortcut)
        {
        }
        public TagNode(Guid _ID, string _label, List<uint> _objects, bool _isShortcut, ConsoleContainerElement _shortcut)
        {
            if (Parent != null)
            {
                Initialize(_ID, _label, _objects, Parent.IsNameable, _isShortcut, _shortcut);
            }
            else
            {
                Initialize(_ID, _label, _objects, true, _isShortcut, _shortcut);
            }
        }
        public TagNode(Guid _ID, string _label, List<uint> _objects, bool _isNameable, bool _isShortcut, ConsoleContainerElement _shortcut)
        {
            Initialize(_ID, _label, _objects, _isNameable, _isShortcut, _shortcut);
        }
        private void Initialize(Guid _ID, string _label, List<uint> _objects, bool _isNameable, bool _isShortcut, ConsoleContainerElement _shortcut)
        {
            ID = _ID;
            Name = _label;
            Nodes = new SortableObservableCollection<uint>();
            Nodes.CollectionChanged += Nodes_CollectionChanged;
            ChangedLongName += TagNode_ChangedLongName;
            Nodes.AddRange(_objects);
            IsInEditMode = false;
            IsNameable = _isNameable;
            IsShortcut = _isShortcut;
            Shortcut = _shortcut;
            AllowDrag = true;
            AllowDrop = true;
        }
        public void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Nodes");
        }
        void TagNode_ChangedLongName(object sender, EventArgs e)
        {
            if (TagGlobals.autoRename && !TagGlobals.isMerging && IsNameable)
            {
                foreach (uint _nodeHandle in Nodes)
                {
                    Autodesk.Max.IINode _object = MaxPluginUtilities.GetNodeByHandle(_nodeHandle);
                    if (_object != null)
                    {
                        _object.RenameNode(LongName);
                    }
                }
            }
        }
        public override void OnDrop(object obj)
        {
            DragContent content = obj as DragContent;
            if (content != null)
            {
                foreach (var item in content.Items.Reverse())
                {
                    TagNode oldNode = (TagNode)item;
                    if (!this.GetNodeBranch().Contains(oldNode))
                    {
                        if (oldNode != this && oldNode.Name != this.Name)
                        {
                            oldNode.Parent.Children.Remove(oldNode);
                            Children.AddRange(new List<TagNode>() { oldNode });
                        }
                        if (oldNode != this && oldNode.Name == Name)
                        {
                            TagHelperMethods.MergeEntities(oldNode, this);
                        }
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
            Nodes.CollectionChanged += Nodes_CollectionChanged;
            ChangedLongName += TagNode_ChangedLongName;
            // sort of versioning control not really graceful
            try
            {
                IsNameable = (bool)info.GetValue("IsNameable", typeof(bool));
            }
            catch
            {
                IsNameable = true;
            }
            IsShortcut = (bool)info.GetValue("IsShortcut", typeof(bool));
            Shortcut = (ConsoleContainerElement)info.GetValue("Shortcut", typeof(ConsoleContainerElement));
            wireColor = (System.Drawing.Color)info.GetValue("wireColor", typeof(System.Drawing.Color));
            IsInEditMode = false;
            AllowDrag = true;
            AllowDrop = true;
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID, typeof(Guid));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Children", Children, typeof(SortableObservableCollection<TagNode>));
            info.AddValue("Nodes", Nodes, typeof(SortableObservableCollection<uint>));
            info.AddValue("IsNameable", IsNameable, typeof(bool));
            info.AddValue("IsShortcut", IsShortcut, typeof(bool));
            info.AddValue("Shortcut", Shortcut, typeof(ConsoleContainerElement));
            info.AddValue("wireColor", wireColor, typeof(System.Drawing.Color));
        }
        internal void ReParent(bool clearObjects)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (clearObjects)
                {
                    Children[i].nodes.Clear();
                }
                Children[i].Parent = this;
                Children[i].ReParent(clearObjects);
            }
        }
        #endregion
    }
}
