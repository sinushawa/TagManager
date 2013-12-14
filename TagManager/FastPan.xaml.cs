using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Form=System.Windows.Forms;

namespace TagManager
{
    public partial class FastPan : System.Windows.Controls.UserControl
    {
        # region Selection dependency definition
        public static readonly DependencyProperty selectionProperty = DependencyProperty.Register("Selection", typeof(SortableObservableCollection<uint>), typeof(FastPan), new FrameworkPropertyMetadata(default(SortableObservableCollection<uint>), new PropertyChangedCallback(onCollectionChanged)));
        public SortableObservableCollection<uint> Selection
        {
            get
            {
                return (SortableObservableCollection<uint>)GetValue(selectionProperty);
            }
            set
            {
                SetValue(selectionProperty, value);
            }
        }
        private static void onCollectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        # endregion
        private Point dragStartPoint;
        private Stopwatch stopwatch;

        public TagNode Root
        {
            get { return TagGlobals.root; }
            set { TagGlobals.root = value; }
        }

        public FastPan()
        {
            InitializeComponent();
            Selection = new SortableObservableCollection<uint>();
            stopwatch = new Stopwatch();
            LoadSource();
        }

        public void LoadSource()
        {
            DataContext = Root;
            ItemToContextMenuConverter.StdContextMenu = this.Resources["StdMenu"] as ContextMenu;
            ItemToContextMenuConverter.RootContextMenu = this.Resources["RootMenu"] as ContextMenu;
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Start();
            dragStartPoint = e.GetPosition(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = dragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) && stopwatch.ElapsedMilliseconds>500)
            {
                var frameworkElem = ((FrameworkElement)e.OriginalSource);
                TreeViewExItem treeViewItem = frameworkElem.TryFindParent<TreeViewExItem>();
                DragDrop.DoDragDrop(treeViewItem, new DataObject(treeViewItem.DataContext), DragDropEffects.Move);
            }
        }
        private void onMouseUp(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Reset();
        }
        // DragNDrop fonctionnality are implemented inside DDNode (treeviewEx way)
        private void onDrop(object sender, DragEventArgs e)
        {
            stopwatch.Reset();
        }
        private void onApplyEntity(object sender, RoutedEventArgs e)
        {
            MenuItem ctrl = sender as MenuItem;
            TagNode _currentEntity=(TagNode)ctrl.DataContext;
            TagMethods.ApplyEntities(new List<TagNode>() { _currentEntity }, MaxPluginUtilities.Selection.ToListHandles());
        }
        private void onSelectEntity(object sender, RoutedEventArgs e)
        {
            MenuItem ctrl = sender as MenuItem;
            TagNode _currentEntity = (TagNode)ctrl.DataContext;
            if (_currentEntity.IsShortcut)
            {
                MaxPluginUtilities.SetSelection(_currentEntity.Shortcut.getCorrespondingSel());
            }
            else
            {
                TagMethods.SelectEntities(new List<TagNode>() { _currentEntity });
            }
        }
        private void onSelectCommonObjects(object sender, RoutedEventArgs e)
        {

        }
        private void onSubstractEntity(object sender, RoutedEventArgs e)
        {
            
        }
        private void onRemoveObjects(object sender, RoutedEventArgs e)
        {
            if (TV.SelectedItems.Count > 0)
            {
                TagMethods.RemoveObjects(TV.SelectedItems.Cast<TagNode>().ToList(), MaxPluginUtilities.Selection.ToListHandles());
            }
            else
            {
                MenuItem ctrl = sender as MenuItem;
                TagNode _currentEntity = (TagNode)ctrl.DataContext;
                TagMethods.RemoveObjects(new List<TagNode>() { _currentEntity }, MaxPluginUtilities.Selection.ToListHandles());
            }
        }
        private void onAddEntity(object sender, RoutedEventArgs e)
        {
            if (TV.SelectedItems.Count > 0)
            {
                TagNode selectedEntity = (TagNode)TV.SelectedItems[0];
                TagNode newNode = new TagNode("untitled");
                selectedEntity.Children.AddRange(new List<TagNode>(){newNode});
                newNode.IsInEditMode = true;
            }
            else
            {
                MenuItem ctrl = sender as MenuItem;
                TagNode selectedEntity = (TagNode)ctrl.DataContext;
                TagNode newNode = new TagNode("untitled");
                selectedEntity.Children.AddRange(new List<TagNode>(){newNode});
                newNode.IsInEditMode = true;
            }
        }
        private void onCreateEntityFromName(object sender, RoutedEventArgs e)
        {
            MenuItem ctrl = sender as MenuItem;
            TagNode selectedEntity = (TagNode)ctrl.DataContext;
            List<TagNode> entityBranch = selectedEntity.GetNodeBranch();
            List<Autodesk.Max.IINode> selectedObjects = MaxPluginUtilities.Selection.ToListNode();
            foreach (Autodesk.Max.IINode obj in selectedObjects)
            {

            }
        }
        private void onRenameFromEntity(object sender, RoutedEventArgs e)
        {

        }
        private void onDeleteEntity(object sender, RoutedEventArgs e)
        {
            MenuItem ctrl = sender as MenuItem;
            TagNode _currentEntity = (TagNode)ctrl.DataContext;
            TagMethods.DeleteEntities(new List<TagNode>() { _currentEntity });
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void onDataChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        
    }
}
