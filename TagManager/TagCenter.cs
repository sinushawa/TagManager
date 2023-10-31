using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.Plugins;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Force.DeepCloner;

namespace TagManager
{
    public static class AssemblyFunctions
    {
        public static void AssemblyMain()
        {
            Autodesk.Max.IGlobal _global = Autodesk.Max.GlobalInterface.Instance;
            IInterface14 _interface = _global.COREInterface14;
            _interface.AddClass(new TagCenter.Descriptor(_global));
        }

        public static void AssemblyShutdown()
        {

        }
    }

    

    public class TagCenter : ReferenceMaker,IPlugin
    {

        private EntityPan _fastPan;

        public EntityPan fastPan
        {
            get { return _fastPan; }
            set { _fastPan = value; }
        }

        private FastWPFTag _fastTag;

        public FastWPFTag fastTag
        {
            get { return _fastTag; }
            set { _fastTag = value; }
        }

        public class Descriptor : Autodesk.Max.Plugins.ClassDesc2
        {

            internal class PostLoadCallback : Autodesk.Max.Plugins.PostLoadCallback
            {
                public PostLoadCallback()
                {
                }
                public override int Priority
                {
                    get
                    {
                        return 1;
                    }
                }
                public override void Proc(IILoad iload)
                {
                    if (iload != null)
                    {
                        for (int i = 0; i < iload.RootNode.NumChildren; i++)
                        {
                            IINode _node = iload.RootNode.GetChildNode(i);
                            uint handle = _node.Handle;
                            IAppDataChunk chunk = _node.GetAppDataChunk(TagGlobals.tagCenter._descriptor.ClassID, TagGlobals.tagCenter._descriptor.SuperClassID, 0);
                            if (chunk != null)
                            {
                                ObjectDataChunk odc = ObjectDataChunk.ByteArrayToObjectDataChunk(chunk.Data);
                                foreach (string ID in odc.entitiesIDs)
                                {
                                    if (TagGlobals.mergedRoot != null)
                                    {
                                        TagNode _nodeMerged = TagGlobals.mergedRoot.GetNodeList().FirstOrDefault(x => x.ID == Guid.Parse(ID));
                                        if (_nodeMerged != null)
                                        {
                                            TagNode entity = TagHelperMethods.GetLonguestMatchingTag(_nodeMerged.GetNodeBranchName(TagGlobals.delimiter, TagGlobals.baseNames), true, _nodeMerged.IsNameable);
                                            if (entity.Name != "Project")
                                            {
                                                TagMethods.ApplyEntities(new List<TagNode>() { entity }, new List<uint>() { _node.Handle });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            protected IGlobal _global;
            public static IClass_ID _classID;
            public bool saveIsNeeded = true;

            public IGlobal Global
            {
                get { return _global; }
            }
            public Descriptor(IGlobal global)
            {
                TagGlobals.tagCenter._descriptor = this;
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

            public override string NonLocalizedClassName => "TagManager";

            public override IOResult Save(IISave isave)
            {
                if (TagGlobals.root != null && TagGlobals.project.Children.Count>0)
                {
                    TagNode _copied = TagGlobals.root.DeepClone();
                    TagMethods.RecursivePruneEntities(_copied);
                    IOResult result = isave.Save(_copied);
                    return result;
                }
                else
                {
                    return IOResult.Ok;
                }
            }
            public override IOResult Load(IILoad iload)
            {
                if (!TagGlobals.isMerging)
                {
                    TagNode openingRoot = (TagNode)iload.LoadObject();
                    if (openingRoot != null)
                    {
                        openingRoot.ReParent(false);
                        TagGlobals.root = openingRoot;
                        if (TagGlobals.root.Children.Where(x => x.Name == "Project").FirstOrDefault() == null)
                        {
                            TagGlobals.root = new TagNode("Root");
                            TagNode firstchild = new TagNode("Project");
                            TagGlobals.root.Children.Add(firstchild);
                        }
                        // created to remove objects getting tagged with projects because of merging longuest match returns project in case of null
                        else
                        {
                            TagGlobals.root.Children.Where(x => x.Name == "Project").FirstOrDefault().Nodes = new SortableObservableCollection<uint>();
                        }
                        TagGlobals.tagCenter.fastPan.DataContext = TagGlobals.root.Children[0];
                    }
                    return base.Load(iload);
                }
                else
                {
                    Descriptor.PostLoadCallback cb = new Descriptor.PostLoadCallback();
                    TagNode openingRoot = (TagNode)iload.LoadObject();
                    if (openingRoot != null)
                    {
                        bool actualValue = TagGlobals.autoRename;
                        TagGlobals.autoRename = false;
                        openingRoot.ReParent(true);
                        TagGlobals.autoRename = actualValue;
                        TagGlobals.mergedRoot = openingRoot;
                        iload.RegisterPostLoadCallback(cb);
                    }
                    return IOResult.Ok;
                }
            }
        }
        public Descriptor _descriptor;
        public TagCenter()
        {
        }
        public TagCenter(Descriptor descriptor) 
        { 
            _descriptor = descriptor;
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
            if (null == System.Windows.Application.Current)
            {
                new System.Windows.Application();
            }
            TagCenter.Instance = this;
            this.Sync = sync;
            fastPan = new EntityPan();
            TagGlobals.tagCenter = this;
            TagGlobals.selectionChain = new Stack<List<TagNode>>();
            InitializeTree();
            TagGlobals.addToSelection = false;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(TagCenter.CurrentDomain_AssemblyResolve);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(MaxStartup)), null, (SystemNotificationCode)80);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.SelectionsetChanged);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(NodeCreated)), null, SystemNotificationCode.NodeCreated);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(PostNodeCloned)), null, SystemNotificationCode.PostNodesCloned);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SelChanged)), null, SystemNotificationCode.NodeRenamed);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(NodeDeleted)), null, SystemNotificationCode.ScenePreDeletedNode);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileReset)), null, SystemNotificationCode.SystemPostReset);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileReset)), null, SystemNotificationCode.SystemPostNew);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileSaving)), null, SystemNotificationCode.FilePreSave);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileSaved)), null, SystemNotificationCode.FilePostSave);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FilePreMerging)), null, SystemNotificationCode.FilePreMerge);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileMerged)), null, SystemNotificationCode.FilePostMerge);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(SceneAddedNode)), null, SystemNotificationCode.SceneAddedNode);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(FileReset)), null, SystemNotificationCode.FilePreOpen);
            //GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(ViewportChanged)), null, SystemNotificationCode.ViewportChange);
        }
        public void InitializeTree()
        {
            TagGlobals.root = new TagNode("Root");
            TagNode firstchild = new TagNode("Project");
            TagGlobals.project = firstchild;
            TagGlobals.root.Children.Add(firstchild);
            
        }

        /// <summary>
        /// MIGHT NOT BE NECESSARY USED TO PUT ANCHORCLASSDESC
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void MaxStartup(IntPtr obj, INotifyInfo param)
        {
            GlobalInterface.Instance.UnRegisterNotification(new GlobalDelegates.Delegate5(this.MaxStartup), null);
        }
        private void SelChanged(IntPtr obj, INotifyInfo param)
        {
            fastPan.Selection = MaxPluginUtilities.Selection.ToSOC();
            if(!TagGlobals.internalSelectionSwitch)
            {
                TagGlobals.selectionChain = new Stack<List<TagNode>>();
            }
            else if(TagGlobals.internalSelectionSwitch && TagGlobals.internalSelectionCounter==0)
            {
                TagGlobals.internalSelectionCounter = 1;
            }
            else if (TagGlobals.internalSelectionSwitch && TagGlobals.internalSelectionCounter == 1)
            {
                TagGlobals.internalSelectionSwitch = !TagGlobals.internalSelectionSwitch;
                TagGlobals.internalSelectionCounter = 0;
            }
        }
        private void PostNodeCloned(IntPtr obj, INotifyInfo param)
        {
            SortableObservableCollection<uint> _originalNodes = ((param.CallParam as IPostNodesClonedParams).OriginalNodes).ToListNode().ToSOC();
            SortableObservableCollection<uint> _clonedNodes = ((param.CallParam as IPostNodesClonedParams).ClonedNodes).ToListNode().ToSOC();
            for(int i = 0; i < _originalNodes.Count; i++)
            {
                uint _originalNodeID = _originalNodes[i];
                uint _clonedNodeID = _clonedNodes[i];
                SortableObservableCollection<TagNode> _tags = TagMethods.GetEntitiesContainingObjects(_originalNodeID);
                foreach(TagNode _tag in _tags)
                {
                    _tag.Nodes.Add(_clonedNodeID);
                }
            }
        }
        private void NodeCreated(IntPtr obj, INotifyInfo param)
        {
            if (TagGlobals.autoCloneTag)
            {
                fastPan.Selection = MaxPluginUtilities.Selection.ToSOC();
                IINode _node = param.CallParam as IINode;
            }
        }
        private void SceneAddedNode(IntPtr obj, INotifyInfo param)
        {
            if (TagGlobals.autoCloneTag)
            {
                IINode _node = param.CallParam as IINode;
            }
        }
        private void FileReset(IntPtr obj, INotifyInfo param)
        {
            InitializeTree();
            fastPan.UpdateSource();
            if (fastTag != null)
            {
                fastTag.CreateAutoCompleteSource();
            }
        }
        private void FileSaving(IntPtr obj, INotifyInfo param)
        {
            var _nodes = (from node in TagGlobals.root.GetNodeList() from objet in node.Nodes group node.ID by objet).ToDictionary();
            foreach (KeyValuePair<uint,List<Guid>> _nodeHandle in _nodes)
            {
                IINode _node = MaxPluginUtilities.GetNodeByHandle(_nodeHandle.Key);
                if (_node != null)
                {
                    ObjectDataChunk odc = new ObjectDataChunk(_nodeHandle.Value);
                    _node.AddAppDataChunk(TagGlobals.tagCenter._descriptor.ClassID, TagGlobals.tagCenter._descriptor.SuperClassID, 0, odc.ToByteArray());
                }
            }
        }
        private void FileSaved(IntPtr obj, INotifyInfo param)
        {
            var _nodes = (from node in TagGlobals.root.GetNodeList() from objet in node.Nodes group node.ID by objet).ToDictionary();
            foreach (KeyValuePair<uint, List<Guid>> _nodeHandle in _nodes)
            {
                IINode _node = MaxPluginUtilities.GetNodeByHandle(_nodeHandle.Key);
                if (_node != null)
                {
                    _node.ClearAllAppData();
                }
            }
        }
        private void FilePreMerging(IntPtr obj, INotifyInfo param)
        { 
            // if it is not an Xref
            if (param.CallParam != null)
            {
                var _nodes = (from node in TagGlobals.root.GetNodeList() from objet in node.Nodes group node.ID by objet).ToDictionary();
                foreach (KeyValuePair<uint, List<Guid>> _nodeHandle in _nodes)
                {
                    IINode _node = MaxPluginUtilities.GetNodeByHandle(_nodeHandle.Key);
                    if (_node != null)
                    {
                        _node.ClearAllAppData();
                    }
                }
                TagGlobals.isMerging = true;
            }
             
        }
        private void FileMerged(IntPtr obj, INotifyInfo param)
        {
            TagGlobals.isMerging = false;
        }
        private void NodeDeleted(IntPtr obj, INotifyInfo param)
        {
            IINode _node = param.CallParam as IINode;
            TagMethods.RemoveObjects(TagGlobals.root.GetNodeList(), new List<uint>() { _node.Handle});
        }

        private void FilePreOpen(IntPtr obj, INotifyInfo param)
        {

        }

        public void CreateTagManagerWin()
        {
            // Create a new managed window to contain the WPF control
            System.Windows.Window dialog = new System.Windows.Window();

            // Name the window
            dialog.Title = "Entity Manager v1.1";

            // Example of setup size and location
            // ...
            //dialog.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            dialog.Width = 220;
            dialog.Height = 650;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

            // Assign the window's content to be the WPF control
            dialog.Content = fastPan;
            dialog.ShowInTaskbar = false;

            // Create an interop helper
            System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(dialog);
            // Assign the 3ds Max HWnd handle to the interop helper
            windowHandle.Owner = ManagedServices.AppSDK.GetMaxHWND(); 
            
            // Setup 3ds Max to handle the WPF dialog correctly
            ManagedServices.AppSDK.ConfigureWindowForMax(dialog);

            TagGlobals.tagCenter.fastPan.UpdateSource();

            dialog.Loaded += dialog_Loaded;
            dialog.Closing += dialog_Closing;
            // Show the dialog box
            dialog.Show();
        }

        void dialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Window dialog = (System.Windows.Window)sender;
            string path = MaxPluginUtilities.GetMaxDir(MaxDirectory.Plugcfg);
            if (File.Exists(path + "\\Entities_ini.xml"))
            {
                try
                {
                    XElement documentBase = XElement.Load(path + "\\Entities_ini.xml");
                    XElement documentSize = documentBase.Element("size");

                    int width = Int32.Parse(documentSize.Element("width").Value);
                    int height = Int32.Parse(documentSize.Element("height").Value);

                    XElement documentPosition = documentBase.Element("position");
                    int X = Int32.Parse(documentPosition.Element("X").Value);
                    int Y = Int32.Parse(documentPosition.Element("Y").Value);

                    dialog.Width = width;
                    dialog.Height = height;
                    dialog.Left = X;
                    dialog.Top = Y;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException(ex.Message);
                }
            }
            System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(dialog);
            WindowExtensions.HideMinimizeAndMaximizeButtons(windowHandle);
            WindowExtensions.RemoveIcon(windowHandle);
        }

        // function to save window size and position before closing
        private void dialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { 
            System.Windows.Window dialog = (System.Windows.Window)sender;
            string path = MaxPluginUtilities.GetMaxDir(MaxDirectory.Plugcfg);
            using (XmlTextWriter writer = new XmlTextWriter(path + "\\Entities_ini.xml", Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument();
                writer.WriteStartElement("display"); // <-- Important root element
                writer.WriteStartElement("size");
                writer.WriteElementString("width", dialog.Width.ToString());
                writer.WriteElementString("height", dialog.Height.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("position");
                writer.WriteElementString("X", dialog.Left.ToString());
                writer.WriteElementString("Y", dialog.Top.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();              // <-- Closes it
                writer.WriteEndDocument();
            }
            e.Cancel = false;
        }
        public void CreateFastTagWin()
        {
            // Create a new managed window to contain the WPF control
            System.Windows.Window dialog = new System.Windows.Window();

            // Name the window
            dialog.Title = "";
            dialog.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 64, 64));
            // Example of setup size and location
            // ...
            //dialog.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            dialog.Width = 320;
            dialog.Height = 30;
            dialog.WindowStyle = System.Windows.WindowStyle.None;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            dialog.ResizeMode = System.Windows.ResizeMode.NoResize;

            // Assign the window's content to be the WPF control
            fastTag = new FastWPFTag();
            fastTag.actbFastBox.Focusable = true;
            
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
            System.Windows.Input.FocusManager.SetFocusedElement(dialog, fastTag.actbFastBox);
            var result = System.Windows.Input.Keyboard.Focus(fastTag.actbFastBox);
            fastTag.actbFastBox.Focus();

        }

        public override RefResult NotifyRefChanged(IInterval changeInt, IReferenceTarget hTarget, ref UIntPtr partID, RefMessage message, bool propagate)
        {
            return RefResult.Succeed;
        }
    }
}
