using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagManager
{
    public static class TagGlobals
    {
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
