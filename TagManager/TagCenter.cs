using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.Plugins;
using System.Reflection;
using System.Windows.Forms;

namespace TagManager
{
    public static class AssemblyFunctions
    {
        public static void AssemblyMain()
        {
            IGlobal _global = Autodesk.Max.GlobalInterface.Instance;
            IInterface14 _interface = _global.COREInterface14;
            _interface.AddClass(new TagCenter.Descriptor(_global));
        }

        public static void AssemblyShutdown()
        {

        }
    } 
    public class TagCenter : ReferenceMaker,IPlugin
    {
        public class Descriptor : Autodesk.Max.Plugins.ClassDesc2
        {
            protected IGlobal _global;
            internal static IClass_ID _classID;
            public bool saveIsNeeded = true;

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
                get { return "Robin plugins"; }
            }

            public override IClass_ID ClassID
            {
                get { return _classID; }
            }

            public override string ClassName
            {
                get { return "TagManager"; }
            }

            public override object Create(bool loading)
            {
                return new TagCenter(this);
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
        public TagCenter()
        {
        }
        public TagCenter(Descriptor descriptor) 
        { 
            this._descriptor = descriptor;
        }
        public System.ComponentModel.ISynchronizeInvoke Sync
        {
            get;
            private set;
        }
        public static TagCenter Instance
        {
            get;
            private set;
        }
        internal bool InitialLaunch
        {
            get;
            set;
        }
        internal testForm MainForm
        {
            get
            {
                if (this._testForm == null)
                {
                    this._testForm = new testForm(this);
                }
                return this._testForm;
            }
        }
        private testForm _testForm;


        /// <summary>
        /// This Function as delegate for AssemblyResolve event permit to load outside assembly alongside the current one.
        /// Necessary for MAX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Autodesk.Max.StateSetsDeploy"))
            {
                return Assembly.GetExecutingAssembly();
            }
            return null;
        }

        public void Cleanup()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(TagCenter.CurrentDomain_AssemblyResolve);
        }

        public void Initialize(IGlobal global, System.ComponentModel.ISynchronizeInvoke sync)
        {
            TagCenter.Instance = this;
            this.Sync = sync;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(TagCenter.CurrentDomain_AssemblyResolve);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(MaxStartup)), null, (SystemNotificationCode)80);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.SelectionsetChanged);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeCreated);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeCloned);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeRenamed);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(NodeDeleted)), null, SystemNotificationCode.ScenePreDeletedNode);
        }

        /// <summary>
        /// MIGHT NOT BE NECESSARY USED TO PUT ANCHORCLASSDESC
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void MaxStartup(IntPtr obj, IntPtr infoHandle)
        {
            GlobalInterface.Instance.UnRegisterNotification(new GlobalDelegates.Delegate5(this.MaxStartup), null);
        }
        private void SelChanged(IntPtr obj, IntPtr infoHandle)
        {
            INotifyInfo notifyInfo = GlobalInterface.Instance.NotifyInfo.Marshal(infoHandle);
            Type callParamType = notifyInfo.CallParam.GetType();
        }
        private void NodeDeleted(IntPtr obj, IntPtr infoHandle)
        {
            INotifyInfo notifyInfo = GlobalInterface.Instance.NotifyInfo.Marshal(infoHandle);
            Type callParamType = notifyInfo.CallParam.GetType();
            PropertyInfo PI = callParamType.GetProperty("Handle");
            object resultat = PI.GetValue(notifyInfo.CallParam);
        }
        internal testForm LaunchDefault()
        {
            IntPtr mAXHWnd = GlobalInterface.Instance.COREInterface.MAXHWnd;
            if (!this.MainForm.InvokeRequired)
            {
                return this.LaunchDefaultUnsafe(mAXHWnd);
            }
            return this.MainForm.Invoke(new Func<IntPtr, testForm>(this.LaunchDefaultUnsafe), new object[]
			{
				mAXHWnd
			}) as testForm;
        }
        private testForm LaunchDefaultUnsafe(IntPtr hwnd)
        {
            testForm MainForm = this.LaunchBlankUnsafe(hwnd);
            if (MainForm == null)
            {
                return null;
            }
            if (this.InitialLaunch)
            {
                this.InitialLaunch = false;
            }
            return MainForm;
        }
        private testForm LaunchBlankUnsafe(IntPtr hwnd)
        {
            if (!this.MainForm.Visible)
            {
                    this.MainForm.Show(NativeWindow.FromHandle(hwnd));
            }
            EnsureWindowWithinScreenBounds(this.MainForm);
            return this.MainForm;
        }
        public void HideMainFrame()
        {
            this.MainForm.Hide();
        }
        public static void EnsureWindowWithinScreenBounds(System.Windows.Forms.Control control)
        {
            if (control.Bounds.X < 100 || control.Bounds.Y < 100 || control.Bounds.X > SystemInformation.VirtualScreen.Width - 100 || control.Bounds.Y > SystemInformation.VirtualScreen.Height - 100)
            {
                control.SetBounds(Math.Min(Math.Max(control.Bounds.X, 100), SystemInformation.VirtualScreen.Width - 100), Math.Min(Math.Max(control.Bounds.Y, 100), SystemInformation.VirtualScreen.Height - 100), -1, -1, BoundsSpecified.Location);
            }
        }

        public override RefResult NotifyRefChanged(IInterval changeInt, IReferenceTarget hTarget, ref UIntPtr partID, RefMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
