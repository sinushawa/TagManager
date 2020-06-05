using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TagManager
{
    public static class LayerManager
    {
        public static IILayerManager manager = MaxPluginUtilities.LayerManager;
        public static IILayer rootLayer = manager.RootLayer;

        public static void CreateLayer(string _layerName)
        {
            manager.CreateLayer(_layerName);
        }
    }
}
