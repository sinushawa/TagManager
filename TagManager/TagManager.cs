﻿using System;
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
    public class testSub : Autodesk.Max.Plugins.UtilityObj
    {
        public class Descriptor : Autodesk.Max.Plugins.ClassDesc2
        {
            protected IGlobal _global;
            internal static IClass_ID _classID;

            public IGlobal Global
            {
                get { return this._global; }
            }

            public Descriptor(IGlobal global)
            {
                this._global = global;
                _classID = _global.Class_ID.Create(0x8962d7, 0x285b3ff9);
            }

            public override string Category
            {
                get { return "NetPluginTests"; }
            }

            public override IClass_ID ClassID
            {
                get { return _classID; }
            }

            public override string ClassName
            {
                get { return "TestPlugin01"; }
            }

            public override object Create(bool loading)
            {
                return new testSub(this);
            }

            public override bool IsPublic
            {
                get { return true; }
            }

            public override SClass_ID SuperClassID
            {
                get { return SClass_ID.Utility; }
            }
        } 
        Descriptor _descriptor;

        public testSub(Descriptor descriptor) 
        { 
            this._descriptor = descriptor; 
        } 

        public override void BeginEditParams(IInterface ip, IIUtil iu) 
        { 
            ip.PushPrompt("This is a prompt msg :D"); 
        } 

        public override void EndEditParams(IInterface ip, IIUtil iu) 
        { 
            ip.PopPrompt(); 
        } 
    }
}
