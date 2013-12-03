﻿using System;
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
        public static string delimiter = "_";
        public static bool addToSelection = false;
        public static bool newSelection = false;
        public static bool autoRename = true;
        public static bool childrenAutoSelect = false;
    }
}