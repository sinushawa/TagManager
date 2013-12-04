using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.DragNDrop;
using System.Linq;

namespace TagManager
{
    [Serializable]
    public abstract class DDNode<T> where T : DDNode<T>
    {
        public event EventHandler ChangedParent;
        public event EventHandler ChangedName;
        public event EventHandler ChangedLongName;
        #region Public Properties

        //private SortableObservableCollection<T> children;
        public SortableObservableCollection<T> Children
        {
            get;
            set;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string longName;
        public string LongName
        {
            get { return longName; }
            set 
            { 
                longName = value;
                ChangedLongName(this, null);
            }
        }

        private T parent;
        public T Parent
        {
            get { return parent; }
            set 
            { 
                parent = value;
                ChangedParent(this, null);
            }
        }
        protected virtual void OnChangedParent(EventArgs e)
        {
            if (ChangedParent != null)
                ChangedParent(this, e);
        }
        private SortableObservableCollection<uint> nodes;
        public SortableObservableCollection<uint> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
        #endregion

        #region Public Methods

        #region Constructors and Destructors

        public DDNode()
        {
            Children = new SortableObservableCollection<T>();
            Children.CollectionChanged += Children_CollectionChanged;
            ChangedParent += DDNode_ChangedParent;
            ChangedName += DDNode_ChangedName;
            ChangedLongName += DDNode_ChangedLongName;
        }

        void DDNode_ChangedName(object sender, EventArgs e)
        {
            LongName = TagHelperMethods.ConcateneNameFromElements(GetNodeBranchElementsNames(true));
        }

        void DDNode_ChangedLongName(object sender, EventArgs e)
        {
            if (TagGlobals.autoRename)
            {
                foreach (uint _nodeHandle in Nodes)
                {
                    Autodesk.Max.IINode _object = MaxPluginUtilities.GetNodeByHandle(_nodeHandle);
                    _object.RenameNode(LongName);
                }
            }
        }

        public void DDNode_ChangedParent(object sender, EventArgs e)
        {
            LongName = TagHelperMethods.ConcateneNameFromElements(GetNodeBranchElementsNames(true));
        }

        public void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (T _node in e.NewItems)
                {
                    _node.Parent = (T)this;
                }
            }
        }

        public void OnInsert(int index, object obj)
        {
            DragContent content = obj as DragContent;
            if (content != null)
            {
                foreach (var item in content.Items.Reverse())
                {
                    T oldNode = (T)item;
                    if (oldNode != this)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        Children.Insert(index, oldNode);
                    }
                }
            }
        }

        public bool CanInsertFormat(int index, string format)
        {
            return true;
        }

        public bool CanInsert(int index, object obj)
        {
            return AllowInsert;
        }

        public bool CanDropFormat(string arg)
        {
            return true;
        }

        public bool AllowDrop { get; set; }

        public bool AllowDrag { get; set; }

        public bool AllowInsert { get; set; }

        public bool CanDrop(object obj)
        {
            return AllowDrop;
        }

        public virtual void OnDrop(object obj)
        {
            /*
            DragContent content = obj as DragContent;
            if (content != null)
            {
                foreach (var item in content.Items.Reverse())
                {
                    T oldNode = (T)item;
                    if (oldNode != this && oldNode.Name != this.Name)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        oldNode.Parent = null;
                        Children.Add(oldNode);
                        oldNode.Parent = (T)this;
                    }
                    if (oldNode.Name == this.Name)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        this.Nodes.AddRange(oldNode.Nodes);
                    }
                }
            }
            */
        }

        public bool CanDrag()
        {
            return AllowDrag;
        }

        public object OnDrag()
        {
            return this;
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
        public List<T> GetNodeBranch()
        {
            T _entity = (T)this;
            List<T> _hierarchy = new List<T>();
            _hierarchy.Add((T)this);
            while (_entity.Parent != null)
            {
                T _parentJoint = _entity.Parent;
                _hierarchy.Add(_parentJoint);
                _entity = _entity.Parent;
            }
            _hierarchy.Reverse();
            return _hierarchy;
        }
        public List<string> GetNodeBranchElementsNames(bool _toRemove)
        {
            T _entity = (T)this;
            List<string> _names = new List<string>();
            _names.Add(this.Name);
            while (_entity.Parent != null)
            {
                T _parentJoint = _entity.Parent;
                _names.Add(_parentJoint.Name);
                _entity = _entity.Parent;
            }
            if (_toRemove)
            {
                _names.RemoveAll(x => TagGlobals.baseNames.Contains(x));
            }
            _names.Reverse();
            return _names;
        }
        public string BranchName(List<T> _entities, string _delimiter)
        {
            string result = "";
            foreach (T _entity in _entities)
            {
                if (result != "")
                {
                    result += _delimiter;
                }
                result += _entity;
            }
            return result;
        }
        public string GetNodeBranchName(string _delimiter)
        {
            List<T> _entities = GetNodeBranch();
            return BranchName(_entities, _delimiter);
        }
        public string GetNodeBranchName(string _delimiter, List<string> _toRemove)
        {
            List<T> _entities = GetNodeBranch();
            _entities.RemoveAll(x => _toRemove.Contains(x.Name));
            return BranchName(_entities, _delimiter);
        }

        #endregion
    }
}
