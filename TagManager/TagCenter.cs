using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using System.Reflection;
using System.Windows.Forms;

namespace TagManager
{
    public class TagCenter : IPlugin
    {
        public IGlobal Global
        {
            get;
            private set;
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
        public TagGUP GUP
        {
            get;
            private set;
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
            this.Global = MaxPluginUtilities.Global;
            this.Sync = sync;
            GUP = new TagGUP(this);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(TagCenter.CurrentDomain_AssemblyResolve);
            GlobalInterface.Instance.RegisterNotification((new GlobalDelegates.Delegate5(this.MaxStartup)), null, (SystemNotificationCode)80);
        }

        /// <summary>
        /// MIGHT NOT BE NECESSARY USED TO PUT ANCHORCLASSDESC
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void MaxStartup(IntPtr arg1, IntPtr arg2)
        {
            GlobalInterface.Instance.UnRegisterNotification(new GlobalDelegates.Delegate5(this.MaxStartup), null);
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
        public static void EnsureWindowWithinScreenBounds(Control control)
        {
            if (control.Bounds.X < 100 || control.Bounds.Y < 100 || control.Bounds.X > SystemInformation.VirtualScreen.Width - 100 || control.Bounds.Y > SystemInformation.VirtualScreen.Height - 100)
            {
                control.SetBounds(Math.Min(Math.Max(control.Bounds.X, 100), SystemInformation.VirtualScreen.Width - 100), Math.Min(Math.Max(control.Bounds.Y, 100), SystemInformation.VirtualScreen.Height - 100), -1, -1, BoundsSpecified.Location);
            }
        }
    }
}
