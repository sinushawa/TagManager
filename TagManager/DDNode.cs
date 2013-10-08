using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.DragNDrop;
using System.Linq;

namespace TagManager
{
    public class DDNode
    {
        #region Constructors and Destructors

        public DDNode()
        {
            Children = new ObservableCollection<DDNode>();
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

        #region Public Properties

        private ObservableCollection<DDNode> children;
        public ObservableCollection<DDNode> Children 
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

        private ObservableCollection<uint> nodes;
        public ObservableCollection<uint> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }
        #endregion

        #region Public Methods

        public List<Autodesk.Max.IINode> GetNodes()
        {
            List<Autodesk.Max.IINode> nodes = new List<Autodesk.Max.IINode>();
            foreach (uint _nodeHandle in Nodes)
            {
                nodes.Add(MaxPluginUtilities.Interface.GetINodeByHandle(_nodeHandle));
            }
            return nodes;
        }
        public void SelectNodes(bool newSelection)
        {
            List<Autodesk.Max.IINode> nodesToSelect = new List<Autodesk.Max.IINode>();
            if (!newSelection)
            {
                nodesToSelect.AddRange(MaxPluginUtilities.Selection);
            }
            nodesToSelect.AddRange(GetNodes());
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
