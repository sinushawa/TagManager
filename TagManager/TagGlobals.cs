using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Autodesk.Max;

namespace TagManager
{
    public static class TagGlobals
    {
        private static string SettingsFilePath
        {
            get { return Path.Combine(MaxPluginUtilities.GetMaxDir(MaxDirectory.PlugcfgLn), "TagManager_Settings.xml"); }
        }

        public static void SaveSettings()
        {
            try
            {
                string path = SettingsFilePath;
                using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("TagManagerSettings");
                    writer.WriteElementString("autoRename", autoRename.ToString());
                    writer.WriteElementString("autoLayer", autoLayer.ToString());
                    writer.WriteElementString("autoCloneTag", autoCloneTag.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                string path = SettingsFilePath;
                if (File.Exists(path))
                {
                    XElement doc = XElement.Load(path);
                    XElement el;

                    el = doc.Element("autoRename");
                    if (el != null) autoRename = bool.Parse(el.Value);

                    el = doc.Element("autoLayer");
                    if (el != null) autoLayer = bool.Parse(el.Value);

                    el = doc.Element("autoCloneTag");
                    if (el != null) autoCloneTag = bool.Parse(el.Value);
                }
            }
            catch { }
        }

        public static TagCenter tagCenter;
        public static TagNode root;
        public static TagNode project;
        public static TagNode mergedRoot;
        public static bool autoCloneTag = true;
        public static bool internalSelectionSwitch = false;
        public static int internalSelectionCounter = 0;
        public static bool isMerging = false;
        public static string delimiter = "_";
        public static bool addToSelection = false;
        public static bool autoRename = true;
        public static bool childrenAutoSelect = false;
        public static List<string> baseNames = new List<string>() { "Root", "Project" };
        public static Stack<List<TagNode>> selectionChain;
        public static bool displayEntities = false;
        public static bool autoLayer = true;
        public static int autoLayerDepth = 0;
        public static List<TagNode> lastUsedNode;
    }
}
