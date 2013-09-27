using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using System.Reflection;

namespace TagManager
{
    class TagCenter : IPlugin
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


        /// <summary>
        /// BEWARE OF THIS MEMBER NOT SET CORRECTLY
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
            throw new NotImplementedException();
        }

        public void Initialize(IGlobal global, System.ComponentModel.ISynchronizeInvoke sync)
        {
            TagCenter.Instance = this;
            Autodesk.Max.Utilities.Global.Instance = global;
            this.Global = global;
            this.Sync = sync;
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
    }
}
