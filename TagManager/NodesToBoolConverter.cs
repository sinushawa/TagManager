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

        // Every tree row runs this on each selection change: build one set per selection
        // snapshot (EntityPan.Selection) instead of re-reading the Max selection per row.
        private static SortableObservableCollection<uint> _lastSelection;
        private static HashSet<uint> _lastSelectionSet = new HashSet<uint>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SortableObservableCollection<uint> NodesInEntity = values[0] as SortableObservableCollection<uint>;
            SortableObservableCollection<uint> NodesInSelection = values[1] as SortableObservableCollection<uint>;
            if (NodesInEntity == null) return null;
            if (NodesInSelection == null) return false;
            if (!ReferenceEquals(NodesInSelection, _lastSelection))
            {
                _lastSelection = NodesInSelection;
                _lastSelectionSet = new HashSet<uint>(NodesInSelection);
            }
            return NodesInEntity.Any(_lastSelectionSet.Contains);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
