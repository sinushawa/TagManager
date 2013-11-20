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
    public abstract class DDNode
    {
        #region Public Properties

        private SortableObservableCollection<DDNode> children;
        public SortableObservableCollection<DDNode> Children
        {
            get { return children; }
            set { children = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private DDNode parent;
        public DDNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        #endregion

        #region Public Methods

        #region Constructors and Destructors

        public DDNode()
        {
            Children = new SortableObservableCollection<DDNode>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (DDNode _node in e.NewItems)
                {
                    _node.Parent = this;
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
                    DDNode oldNode = (DDNode)item;
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

        public void OnDrop(object obj)
        {
            DragContent content = obj as DragContent;
            if (content != null)
            {
                foreach (var item in content.Items.Reverse())
                {
                    DDNode oldNode = (DDNode)item;
                    if (oldNode != this)
                    {
                        oldNode.Parent.Children.Remove(oldNode);
                        Children.Add(oldNode);
                    }
                }
            }
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
        public List<DDNode> GetNodeBranch()
        {
            DDNode _entity = this;
            List<DDNode> _hierarchy = new List<DDNode>();
            _hierarchy.Add(this);
            while (_entity.Parent != null)
            {
                DDNode _parentJoint = _entity.Parent;
                _hierarchy.Add(_parentJoint);
                _entity = _entity.Parent;
            }
            _hierarchy.Reverse();
            return _hierarchy;
        }
        public string GetNodeBranchName(string _delimiter)
        {
            string result = "";
            List<DDNode> _entities = GetNodeBranch();
            foreach (DDNode _entity in _entities)
            {
                if (result != "")
                {
                    result += _delimiter;
                }
                result += _entity;
            }
            return result;
        }

        #endregion
    }
}
