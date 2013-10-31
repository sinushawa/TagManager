using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace TagManager
{
    public static class DependencyObjExtensions
    {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }
        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
        /// <summary>
        /// Find the first child of the specified type (the child must exist)
        /// by walking down the logical/visual trees
        /// Will throw an exception if a matching child does not exist. If you're not sure, use the TryFindChild method instead.
        /// </summary>
        /// <typeparam name="T">The type of child you want to find</typeparam>
        /// <param name="parent">The dependency object whose children you wish to scan</param>
        /// <returns>The first descendant of the specified type</returns>
        /// <remarks> usage: myWindow.FindChild<StackPanel>() </StackPanel></remarks>
        public static T FindChild<T>(this DependencyObject parent)
            where T : DependencyObject
        {
            return parent.FindChild<T>(child => true);
        }

        /// <summary>
        /// Find the first child of the specified type (the child must exist)
        /// by walking down the logical/visual trees, which meets the specified criteria
        /// Will throw an exception if a matching child does not exist. If you're not sure, use the TryFindChild method instead.
        /// </summary>
        /// <typeparam name="T">The type of child you want to find</typeparam>
        /// <param name="parent">The dependency object whose children you wish to scan</param>
        /// <param name="predicate">The child object is selected if the predicate evaluates to true</param>
        /// <returns>The first matching descendant of the specified type</returns>
        /// <remarks> usage: myWindow.FindChild<StackPanel>( child => child.Name == "myPanel" ) </StackPanel></remarks>
        public static T FindChild<T>(this DependencyObject parent, Func<T, bool> predicate)
            where T : DependencyObject
        {
            return parent.FindChildren<T>(predicate).First();
        }

        /// <summary>
        /// Use this overload if the child you're looking may not exist.
        /// </summary>
        /// <typeparam name="T">The type of child you're looking for</typeparam>
        /// <param name="parent">The dependency object whose children you wish to scan</param>
        /// <param name="foundChild">out param - the found child dependencyobject, null if the method returns false</param>
        /// <returns>True if a child was found, else false</returns>
        public static bool TryFindChild<T>(this DependencyObject parent, out T foundChild)
            where T : DependencyObject
        {
            return parent.TryFindChild<T>(child => true, out foundChild);
        }

        /// <summary>
        /// Use this overload if the child you're looking may not exist.
        /// </summary>
        /// <typeparam name="T">The type of child you're looking for</typeparam>
        /// <param name="parent">The dependency object whose children you wish to scan</param>
        /// <param name="predicate">The child object is selected if the predicate evaluates to true</param>
        /// <param name="foundChild">out param - the found child dependencyobject, null if the method returns false</param>
        /// <returns>True if a child was found, else false</returns>
        public static bool TryFindChild<T>(this DependencyObject parent, Func<T, bool> predicate, out T foundChild)
            where T : DependencyObject
        {
            foundChild = null;
            var results = parent.FindChildren<T>(predicate);
            if (results.Count() == 0)
                return false;

            foundChild = results.First();
            return true;
        }

        /// <summary>
        /// Get a list of descendant dependencyobjects of the specified type and which meet the criteria
        /// as specified by the predicate
        /// </summary>
        /// <typeparam name="T">The type of child you want to find</typeparam>
        /// <param name="parent">The dependency object whose children you wish to scan</param>
        /// <param name="predicate">The child object is selected if the predicate evaluates to true</param>
        /// <returns>The first matching descendant of the specified type</returns>
        /// <remarks> usage: myWindow.FindChildren<StackPanel>( child => child.Name == "myPanel" ) </StackPanel></remarks>
        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent, Func<T, bool> predicate)
            where T : DependencyObject
        {
            var children = new List<DependencyObject>();

            if ((parent is Visual) || (parent is Visual3D))
            {
                var visualChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int childIndex = 0; childIndex < visualChildrenCount; childIndex++)
                    children.Add(VisualTreeHelper.GetChild(parent, childIndex));
            }
            foreach (var logicalChild in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
                if (!children.Contains(logicalChild))
                    children.Add(logicalChild);

            foreach (var child in children)
            {
                var typedChild = child as T;
                if ((typedChild != null) && predicate.Invoke(typedChild))
                    yield return typedChild;

                foreach (var foundDescendant in FindChildren(child, predicate))
                    yield return foundDescendant;
            }
            yield break;
        }

    }
}
