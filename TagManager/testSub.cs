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
    public static class AssemblyFunctions
    {
        public static void AssemblyMain()
        {
            var g = Autodesk.Max.GlobalInterface.Instance;
            var i = g.COREInterface13;
            i.AddClass(new testSub.Descriptor(g));
        }

        public static void AssemblyShutdown()
        {

        }
    } 
    public class testSub : Autodesk.Max.Plugins.UtilityObj
    {
        public class Descriptor : Autodesk.Max.Plugins.ClassDesc2
        {
            protected IGlobal _global;
            internal static IClass_ID _classID;
            public bool saveIsNeeded = false;

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
            public override bool NeedsToSave
            {
                get
                {
                    return saveIsNeeded;
                }
            }
            public override IOResult Save(IISave isave)
            {
                IOResult result = isave.Save("test");
                return result;
            }
            public override IOResult Load(IILoad iload)
            {
                string res = iload.LoadObject() as string;
                return base.Load(iload);
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
            _descriptor.saveIsNeeded = true;
        } 

        public override void EndEditParams(IInterface ip, IIUtil iu) 
        { 
            ip.PopPrompt(); 
        }
    }
}
