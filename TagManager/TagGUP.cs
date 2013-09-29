using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace TagManager
{
    public class TagGUP : IGUP
    {
        public string remember = "";
        public TagCenter parent;

        public TagGUP(TagCenter _parent)
        {
            parent = _parent;
        }
        public IOResult Save(IISave isave)
        {
            IOResult result = isave.Save(remember);
            return result;
        }

        public uint Start
        {
            get { throw new NotImplementedException(); }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public IBaseInterface GetInterface(IInterface_ID id)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IInterfaceServer other)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IntPtr Handle
        {
            get { throw new NotImplementedException(); }
        }

        public IBitmapManager Bmi
        {
            get { throw new NotImplementedException(); }
        }

        public UIntPtr Control(uint parameter)
        {
            throw new NotImplementedException();
        }

        public int EnumTree(IITreeEnumProc proc)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteFileScript(string file)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteStringScript(string string_)
        {
            throw new NotImplementedException();
        }

        public IOResult Load(IILoad iload)
        {
            string result = iload.LoadObject() as string;
            parent.MainForm.reader_tbx.Text = remember;
            return IOResult.Ok;
        }

        public IInterface Max
        {
            get { throw new NotImplementedException(); }
        }

        public IDllDir MaxDllDir
        {
            get { throw new NotImplementedException(); }
        }

        public IntPtr MaxInst
        {
            get { throw new NotImplementedException(); }
        }

        public IntPtr MaxWnd
        {
            get { throw new NotImplementedException(); }
        }
    }
}
