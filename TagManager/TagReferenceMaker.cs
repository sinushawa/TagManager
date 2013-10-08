using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace TagManager
{
    public class TagReferenceMaker:ReferenceMaker
    {
        public string remember = "";

        public TagReferenceMaker(TagCenter _parent)
        {

        }

        public override RefResult NotifyRefChanged(IInterval changeInt, IReferenceTarget hTarget, ref UIntPtr partID, RefMessage message)
        {
            return RefResult.Dontcare;
        }
        public override IOResult Save(IISave isave)
        {
            isave.Save("test");
            return IOResult.Ok;
        }
        public override IOResult Load(IILoad iload)
        {
            remember = iload.LoadObject() as string;
            return IOResult.Ok;
        }
    }
}
