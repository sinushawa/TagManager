﻿using Autodesk.Max;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TagManager
{
    public static class MaxPluginUtilities
    {
        public static IInterface14 Interface
        {
            get
            {
                return MaxPluginUtilities.Global.COREInterface14;
            }
        }
        public static IGlobal Global
        {
            get
            {
                return GlobalInterface.Instance;
            }
        }
        public static IILayerManager LayerManager
        {
            get
            {
                return MaxPluginUtilities.Interface.LayerManager;
            }
        }

        public static List<IINode> Selection
        {
            get
            {
                return GetSelection();
            }
            set
            {
                SetSelection(value);
            }
        }
        public static string GetMaxDir(MaxDirectory dir)
        {
            string path = Global.IPathConfigMgr.PathConfigMgr.GetDir(dir);
            return path;
        }
        private static List<IINode> GetSelection()
        {
            List<IINode> selectedNodes = new List<IINode>();
            for (int i = 0; i < Interface.SelNodeCount; i++)
            {
                selectedNodes.Add(Interface.GetSelNode(i));
            }
            return selectedNodes;
        }
        private static void SetSelection(List<IINode> _nodes)
        {
            try
            {
                _nodes = _nodes.Distinct().ToList();
                IINodeTab selectedNodes = Global.INodeTab.Create();
                selectedNodes.Resize(_nodes.Count);
                foreach (IINode _node in _nodes)
                {
                    if (_node != null)
                    {
                        selectedNodes.AppendNode(_node, false, 1);
                    }
                    //Interface.SelectNode(_node, false);
                }
                Interface.SelectNodeTab(selectedNodes, true, true);
            }
            catch
            {
            }
        }
        public static void SetSelection(List<uint> _nodesHandles)
        {
            if (!TagGlobals.addToSelection)
            {
                Interface.ClearNodeSelection(false);
            }
            Selection = _nodesHandles.GetNodesByHandles();
        }
        private static void SetSelection(SortableObservableCollection<IINode> _nodes)
        {
            Selection = _nodes.ToList();
        }
        public static IINodeTab ToNodeTab(this List<IINode> _nodes)
        {
            IINodeTab nodeTab = Global.INodeTab.Create();
            foreach (IINode _node in _nodes)
            {
                nodeTab.AppendNode(_node, false, 1);
                Interface.SelectNode(_node, false);
            }
            return nodeTab;
        }
        public static ITab<IINode> ToITab(this List<IINode> _nodes)
        {
            
            ITab<IINode> _Itab = _nodes.ToNodeTab() as ITab<IINode>;

            return _Itab;
        }
        public static IINode GetNodeByHandle(uint _handle)
        {
            return Interface.GetINodeByHandle(_handle);
        }
        public static List<IINode> GetNodesByHandles(this IEnumerable<uint> _handles)
        {
            List<IINode> nodes = new List<IINode>();
            foreach(uint _handle in _handles)
            {
                nodes.Add(GetNodeByHandle(_handle));
            }
            return nodes;
        }
        public static List<IINode> ToListNode(this IINodeTab _nodes)
        {
            List<IINode> listNodes = new List<IINode>();
            for (int i = 0; i < _nodes.Count; i++ )
            {
                IntPtr pointer = (IntPtr)i;
                listNodes.Add(_nodes[i]);
            }
            return listNodes;
        }
        public static List<uint> ToListHandles(this List<IINode> _nodes)
        {
            List<uint> listHandles = new List<uint>();
            foreach (IINode _node in _nodes)
            {
                listHandles.Add(_node.Handle);
            }
            return listHandles;
        }
        public static SortableObservableCollection<uint> ToSOC(this List<IINode> _nodes)
        {
            SortableObservableCollection<uint> listHandles = new SortableObservableCollection<uint>();
            foreach (IINode _node in _nodes)
            {
                listHandles.Add(_node.Handle);
            }
            return listHandles;
        }

        public static void Write(string s, params string[] args)
        {
            MaxPluginUtilities.Write(string.Format(s, args));
        }
        public static void Write(string s)
        {
            MaxPluginUtilities.Global.TheListener.EditStream.Wputs(s);
            MaxPluginUtilities.Global.TheListener.EditStream.Wflush();
        }
        public static void WriteLine(string s)
        {
            MaxPluginUtilities.Write(s);
            MaxPluginUtilities.Write("\n");
        }
        public static void WriteLine(string s, params string[] args)
        {
            MaxPluginUtilities.WriteLine(string.Format(s, args));
        }
        public static IOResult Save(this IISave isave, object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, obj);
                uint num = 0u;
                byte[] array = memoryStream.ToArray();
                isave.WriteVoid(BitConverter.GetBytes(array.Length), ref num);
                if (array.Length > 0)
                {
                    isave.WriteVoid(array, ref num);
                }
            }
            return IOResult.Ok;
        }
        public static object LoadObject(this IILoad iload)
        {
            uint num = 0u;
            if (iload.CurChunkLength < 4uL)
            {
                return null;
            }
            ulong arg_17_0 = iload.CurChunkLengthRemaining;
            byte[] array = new byte[4];
            iload.ReadVoid(array, ref num);
            int num2 = BitConverter.ToInt32(array, 0);
            ulong arg_37_0 = iload.CurChunkLengthRemaining;
            if (num == 4u && num2 > 0 && num2 == (int)iload.CurChunkLengthRemaining)
            {
                array = new byte[num2];
                iload.ReadVoid(array, ref num);
                using (MemoryStream memoryStream = new MemoryStream(array))
                {
                    try
                    {
                        object result = new BinaryFormatter().Deserialize(memoryStream);
                        return result;
                    }
                    catch (SerializationException)
                    {
                        object result = null;
                        return result;
                    }
                }
            }
            return null;
        }
        public static bool GetNodeHidden(uint _nodeHandle)
        {
            return GetNodeByHandle(_nodeHandle).IsHidden(NodeHideFlags.All, false);
        }
        public static string MakeNameUnique(string _namePrefix)
        {
            Interface.MakeNameUnique(ref _namePrefix);
            return _namePrefix;
        }
        public static void RenameNode(this IINode _node, string _newName)
        {
            string finalName = MakeNameUnique(_newName+TagGlobals.delimiter);
            _node.Name = finalName;
            _node.NotifyNameChanged();
        }
        public static void RenameNode(this uint _nodeHandle, string _newName)
        {
            IINode _node = GetNodeByHandle(_nodeHandle);
            RenameNode(_node, _newName);
        }
        public static void NotifyNameChanged(this IINode _node)
        {
            // this part is only to refresh the node's name in the UI
            IInterval interval = MaxPluginUtilities.Global.Interval.Create();
            interval.SetInfinite();
            UIntPtr partID = (UIntPtr)RefMessage.NodeNamechange;
            SClass_ID[] classes = Autodesk.Max.Utilities.SClass_IDs.AllSuperClassIDs;
            for (int i = 0; i < classes.Length; i++)
            {
                //_node.NotifyDependents(interval, partID, RefMessage.NodeNamechange, classes[i], true, null);
                // max 2016
                //_node.NotifyDependents_(interval, partID, RefMessage.NodeNamechange, classes[i], true, null);
                _node.NotifyDependents(interval, partID, RefMessage.NodeNamechange, classes[i], true, null, NotifyDependentsOption.AllowOptimizations);
            }
        }
        public static IILayer GetObjectLayer(IINode _node)
        {
            return (IILayer)_node.GetReference(6);
        }
        public static void SetObjectLayer(IINode _node, string LayerName)
        {
            IILayer _layer = LayerManager.GetLayer(LayerName);
            _layer.AddToLayer(_node);
        }
        public static void SetObjectsLayer(List<IINode> _nodes, string LayerName)
        {
            IILayer _layer = LayerManager.GetLayer(LayerName);
            foreach(IINode _node in _nodes)
            {
                _layer.AddToLayer(_node);
            }
        }

        public static bool IsLayerExisting(string _layerName)
        {
            IILayer _layer = LayerManager.GetLayer(_layerName);
            return (_layer != null);
        }
        public static IILayer CreateLayer(IILayer _parent, string _layerName)
        {
            IILayer _newLayer = LayerManager.CreateLayer(_layerName);
            _newLayer.SetParentLayer(_parent);
            return _newLayer;
        }
        public static IILayer CreateLayer(string _layerName)
        {
            IILayer _newLayer = LayerManager.CreateLayer(_layerName);
            return _newLayer;
        }
        public static IILayer CreateLayerBranch(TagNode _leaf)
        {
            IILayer _selectedLayer=LayerManager.RootLayer;
            for (int i=0; i< _leaf.GetNodeBranch(false).Count; i++)
            {
                TagNode _entity = _leaf.GetNodeBranch(false)[i];
                if (!IsLayerExisting(_entity.LongName))
                {
                    if (i != 0)
                    {
                        _selectedLayer = CreateLayer(_selectedLayer, _entity.LongName);
                    }
                    else
                    {
                        _selectedLayer = CreateLayer(_entity.LongName);
                    }
                }
                else
                {
                    _selectedLayer = LayerManager.GetLayer(_entity.LongName);
                }
            }
            return _selectedLayer;
        }
        public static List<IINode> GetAllSceneNodes()
        {
            IINode _root = Interface.RootNode;
            int _nodesNb = _root.NumberOfChildren;
            List<IINode> _allNodes = new List<IINode>();
            for(int i=0; i<_nodesNb; i++)
            {
                _allNodes.Add(_root.GetChildNode(i));
            }
            return _allNodes;
        }
        public static List<IINode> GetVisibleNodes(List<IINode> _nodes)
        {
            List<IINode> _visibleNodes = new List<IINode>();
            foreach(IINode _node in _nodes)
            {
                IILayer _layer = GetObjectLayer(_node);
                bool _layerHidden = _layer.IsHidden(true);
                if (!_node.IsObjectHidden && !_layerHidden)
                {
                    _visibleNodes.Add(_node);
                }
            }
            return _visibleNodes;
        }
        public static List<uint> GetVisibleNodes(List<uint> _nodeHandles)
        {
            List<IINode> _nodes = _nodeHandles.GetNodesByHandles();
            List<IINode> _visibleNodes = new List<IINode>();
            foreach (IINode _node in _nodes)
            {
                IILayer _layer = GetObjectLayer(_node);
                bool _layerHidden = _layer.IsHidden(true);
                if (!_node.IsObjectHidden && !_layerHidden)
                {
                    _visibleNodes.Add(_node);
                }
            }
            return _visibleNodes.ToListHandles();
        }
        public static List<IINode> GetVsibileNodesInScene()
        {
            return GetVisibleNodes(GetAllSceneNodes());
        }
    }
}
