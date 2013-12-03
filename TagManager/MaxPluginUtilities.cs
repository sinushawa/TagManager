using Autodesk.Max;
using System;
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
        public static IINodeTab Selection
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
        private static IINodeTab GetSelection()
        {
            IINodeTab selectedNodes = Global.NodeTab.Create();
            for (int i = 0; i < Interface.SelNodeCount; i++)
            {
                selectedNodes.AppendNode(Interface.GetSelNode(i), false, 1);
            }
            return selectedNodes;
        }
        private static void SetSelection(IINodeTab _nodes)
        {
            Interface.SelectNodeTab(_nodes, TagGlobals.addToSelection, true);
        }
        public static void SetSelection(List<uint> _nodesHandles, bool _newSelection)
        {
            TagGlobals.addToSelection = _newSelection;
            Selection = _nodesHandles.GetNodesByHandles().ToNodeTab();
            TagGlobals.addToSelection = false;
        }
        private static void SetSelection(SortableObservableCollection<IINode> _nodes)
        {
            Selection = _nodes.ToList().ToNodeTab();
        }
        public static IINodeTab ToNodeTab(this List<IINode> _nodes)
        {
            IINodeTab nodeTab = Global.NodeTab.Create();
            foreach (IINode _node in _nodes)
            {
                nodeTab.AppendNode(_node, false, 1);
            }
            return nodeTab;
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
                listNodes.Add(_nodes[pointer]);
            }
            return listNodes;
        }
        public static List<uint> ToListHandles(this IINodeTab _nodes)
        {
            List<uint> listHandles = new List<uint>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                IntPtr pointer = (IntPtr)i;
                listHandles.Add(_nodes[pointer].Handle);
            }
            return listHandles;
        }
        public static SortableObservableCollection<uint> ToSOC(this IINodeTab _nodes)
        {
            SortableObservableCollection<uint> listHandles = new SortableObservableCollection<uint>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                IntPtr pointer = (IntPtr)i;
                listHandles.Add(_nodes[pointer].Handle);
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
            MaxPluginUtilities.Global.TheListener.EditStream.Flush();
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
        public static string RenameObject(string _namePrefix)
        {
            Interface.MakeNameUnique(ref _namePrefix);
            return _namePrefix;
        }
    }
}
