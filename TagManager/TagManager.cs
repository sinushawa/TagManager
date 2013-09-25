using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace TagManager
{
    public class TagManager
    {
        public IGlobal maxGlobal;
        public IInterface13 maxInterface;

        public TagManager()
        {
            maxGlobal = Autodesk.Max.GlobalInterface.Instance;
            maxInterface = maxGlobal.COREInterface13;
        }

        public static void Manage()
        {
            TagManager _tagManager = new TagManager();
            IListener _listener = _tagManager.maxGlobal.TheListener;
            int _nbNodes = _tagManager.maxInterface.SelNodeCount;
            List<IINode> _nodes = new List<IINode>();
            for (int i=0; i<_nbNodes; i++)
            {
                IINode _node = _tagManager.maxInterface.GetSelNode(i);
                _nodes.Add(_node);
                _listener.EditStream.Wputs(_node.Name+ "\n");
                _listener.EditStream.Flush();
            }
            
        }
    }
}
