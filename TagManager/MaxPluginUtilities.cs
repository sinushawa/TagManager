using Autodesk.Max;
using System;
namespace TagManager
{
    public static class MaxPluginUtilities
    {
        public static IInterface14 Interface
        {
            get
            {
                return MaxPluginUtilities.Global.COREInterface14;
            }
        }
        public static IGlobal Global
        {
            get
            {
                return GlobalInterface.Instance;
            }
        }
        public static void Write(string s, params string[] args)
        {
            MaxPluginUtilities.Write(string.Format(s, args));
        }
        public static void Write(string s)
        {
            MaxPluginUtilities.Global.TheListener.EditStream.Wputs(s);
            MaxPluginUtilities.Global.TheListener.EditStream.Flush();
        }
        public static void WriteLine(string s)
        {
            MaxPluginUtilities.Write(s);
            MaxPluginUtilities.Write("\n");
        }
        public static void WriteLine(string s, params string[] args)
        {
            MaxPluginUtilities.WriteLine(string.Format(s, args));
        }
    }
}
