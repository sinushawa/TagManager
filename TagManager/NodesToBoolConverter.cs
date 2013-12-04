using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace TagManager
{
    public class NodesToBoolConverter :MarkupExtension, IMultiValueConverter
    {
        private static NodesToBoolConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new NodesToBoolConverter();
            }
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SortableObservableCollection<uint> NodesInEntity = (SortableObservableCollection<uint>)values[0];
            SortableObservableCollection<uint> NodesInSelection = (SortableObservableCollection<uint>)values[1];
            if (NodesInEntity == null) return null;
            if (NodesInEntity.Intersect(MaxPluginUtilities.Selection.ToListHandles()).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
