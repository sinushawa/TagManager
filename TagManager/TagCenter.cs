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
        private FastPan _fastPan;

        public FastPan fastPan
        {
            get { return _fastPan; }
            set { _fastPan = value; }
        }

        public class Descriptor : Autodesk.Max.Plugins.ClassDesc2
        {
            protected IGlobal _global;
            public static IClass_ID _classID;
            public bool saveIsNeeded = true;

            public IGlobal Global
            {
                get { return _global; }
            }
            public Descriptor(IGlobal global)
            {
                _global = global;
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
                IOResult result = isave.Save(TagGlobals.root);
                return result;
            }
            public override IOResult Load(IILoad iload)
            {
                TagNode res = (TagNode)iload.LoadObject();
                res.ReParent();
                TagGlobals.root = res;
                TagGlobals.tagCenter.fastPan.DataContext = res;
                return base.Load(iload);
            }
        }
        public Descriptor _descriptor;
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

        private SortableObservableCollection<uint> selectedObjects;
        public SortableObservableCollection<uint> SelectedObjects
        {
            get { return selectedObjects; }
            set { selectedObjects = value; }
        }


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
            fastPan = new FastPan();
            TagGlobals.tagCenter = this;
            InitializeTree();
            TagGlobals.addToSelection = false;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(TagCenter.CurrentDomain_AssemblyResolve);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(MaxStartup)), null, (SystemNotificationCode)80);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.SelectionsetChanged);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeCreated);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeCloned);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeRenamed);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(NodeDeleted)), null, SystemNotificationCode.ScenePreDeletedNode);
        }
        public void InitializeTree()
        {
            TagGlobals.root = new TagNode("Root");
            TagNode firstchild = new TagNode("Project");
            TagGlobals.root.Children.Add(firstchild);
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
            SelectedObjects = MaxPluginUtilities.Selection.ToSOC();
        }
        private void NodeDeleted(IntPtr obj, IntPtr infoHandle)
        {
            INotifyInfo notifyInfo = GlobalInterface.Instance.NotifyInfo.Marshal(infoHandle);
            IINode _node = notifyInfo.CallParam as IINode;
            TagMethods.RemoveObjects(TagGlobals.root.GetNodeList(), new List<uint>() { _node.Handle});
        }
        public void CreateTagManagerWin()
        {
            // Create a new managed window to contain the WPF control
            System.Windows.Window dialog = new System.Windows.Window();

            // Name the window
            dialog.Title = "Entity Manager";

            // Example of setup size and location
            // ...
            //dialog.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            dialog.Width = 220;
            dialog.Height = 650;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ResizeMode = System.Windows.ResizeMode.NoResize;

            // Assign the window's content to be the WPF control
            dialog.Content = fastPan;
            dialog.ShowInTaskbar = false;

            // Create an interop helper
            System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(dialog);
            // Assign the 3ds Max HWnd handle to the interop helper
            windowHandle.Owner = ManagedServices.AppSDK.GetMaxHWND();

            // Setup 3ds Max to handle the WPF dialog correctly
            ManagedServices.AppSDK.ConfigureWindowForMax(dialog);

            // Show the dialog box
            dialog.Show();
        }
        public void CreateFastTagWin()
        {
            // Create a new managed window to contain the WPF control
            System.Windows.Window dialog = new System.Windows.Window();

            // Name the window
            dialog.Title = "";

            // Example of setup size and location
            // ...
            //dialog.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            dialog.Width = 320;
            dialog.Height = 30;
            dialog.WindowStyle = System.Windows.WindowStyle.None;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            dialog.ResizeMode = System.Windows.ResizeMode.NoResize;

            // Assign the window's content to be the WPF control
            FastWPFTag fastTag = new FastWPFTag();
            fastTag.FastBox.Focusable = true;
            
            fastTag.CreateAutoCompleteSource();
            fastTag.winParent = dialog;
            dialog.Content = fastTag;
            dialog.ShowInTaskbar = false;

            // Create an interop helper
            System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(dialog);
            // Assign the 3ds Max HWnd handle to the interop helper
            windowHandle.Owner = ManagedServices.AppSDK.GetMaxHWND();

            // Setup 3ds Max to handle the WPF dialog correctly
            ManagedServices.AppSDK.ConfigureWindowForMax(dialog);

            // Show the dialog box
            dialog.Show();
            System.Windows.Input.FocusManager.SetFocusedElement(dialog, fastTag.FastBox);
            System.Windows.Controls.TextBox fastTXT = fastTag.FastBox.Template.FindName("Text", fastTag.FastBox) as System.Windows.Controls.TextBox; 
            var result = System.Windows.Input.Keyboard.Focus(fastTag.FastBox);
            fastTXT.Focus();

        }


        public override RefResult NotifyRefChanged(IInterval changeInt, IReferenceTarget hTarget, ref UIntPtr partID, RefMessage message)
        {
            return RefResult.Succeed;
        }
    }
}
