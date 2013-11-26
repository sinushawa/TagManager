using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagManager
{
    [ValueConversion(typeof(TagNode), typeof(ContextMenu))]
    public class ItemToContextMenuConverter : IValueConverter
    {
        public static ContextMenu StdContextMenu;
        public static ContextMenu RootContextMenu;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TagNode item = (TagNode)value;
            if (item == null) return null;
            if (item.Name == "Root" || item.Name == "Project")
            {
                return RootContextMenu;
            }
            else
            {
                return StdContextMenu;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
