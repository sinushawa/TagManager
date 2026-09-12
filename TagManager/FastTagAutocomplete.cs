using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using dragonz.actb.core;
using dragonz.actb.provider;

namespace TagManager
{
    // Adapter for the bundled dragonz.actb version. Its public API does not expose opening
    // the popup for empty input. Reuse its exact popup/list instead of adding another UI.
    internal static class FastTagAutocomplete
    {
        private static readonly MethodInfo Populate = typeof(AutoCompleteManager).GetMethod("PopulatePopupList", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo List = typeof(AutoCompleteManager).GetField("_listBox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void Show(AutoCompleteManager manager, IEnumerable<string> paths)
        {
            if (Populate == null || List == null) throw new InvalidOperationException("Unsupported FastTag autocomplete library");
            Populate.Invoke(manager, new object[] { paths.ToList() });
        }
        public static string Selected(AutoCompleteManager manager)
        {
            var list = List == null ? null : List.GetValue(manager) as ListBox;
            return list == null ? null : list.SelectedItem as string;
        }
    }

    public partial class FastWPFTag
    {
        private string _proposalHandles = "";
        private List<string> _suggestedPaths = new List<string>();
        private bool _typed;
        private bool _refreshScheduled;

        private void SuggestionsLoaded(object sender, RoutedEventArgs e)
        {
            _proposalHandles = string.Join(",", MaxPluginUtilities.Selection.Select(n => n.Handle));
            // OwnerWindow_Loaded initializes the existing popup first.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsLoaded) return;
                TagApi.PrepareFastTag(_proposalHandles);
                RefreshSuggestionsWhenEmpty();
            }), DispatcherPriority.ContextIdle);
        }

        public void RefreshSuggestionsWhenEmpty()
        {
            if (!IsLoaded || _typed || !string.IsNullOrEmpty(actbFastBox.Text) || _refreshScheduled) return;
            _refreshScheduled = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshScheduled = false;
                if (!IsLoaded || _typed || !string.IsNullOrEmpty(actbFastBox.Text)) return;
                _suggestedPaths = TagApi.FastTagPaths(_proposalHandles).Distinct().ToList();
                var all = _suggestedPaths.Concat(branchNames ?? new List<string>()).Distinct().ToList();
                actbFastBox.AutoCompleteManager.DataProvider = new DataProviderContains(all);
                actbFastBox.AutoCompleteManager.AutoAppend = false;
                FastTagAutocomplete.Show(actbFastBox.AutoCompleteManager, all);
            }), DispatcherPriority.ContextIdle);
        }

        private void SuggestionsPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Parent preview runs before the existing textbox/library key handlers. Keep
            // their navigation, slash tagging and typed Enter-to-select behaviour intact.
            if ((e.Key == Key.Return || e.Key == Key.Oem2) && !_typed && _proposalHandles.Length > 0)
            {
                string path = FastTagAutocomplete.Selected(actbFastBox.AutoCompleteManager) ?? actbFastBox.Text;
                if (string.IsNullOrEmpty(path)) path = _suggestedPaths.FirstOrDefault();
                if (path != null && _suggestedPaths.Contains(path))
                {
                    e.Handled = true;
                    var result = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(TagApi.AcceptFastTag(_proposalHandles, path)) as Dictionary<string, object>;
                    if (result != null && result.ContainsKey("ok") && (bool)result["ok"]) winParent.Close();
                    else { FastPop.IsOpen = true; fastTip.Text = result == null ? "Unable to apply suggestion" : Convert.ToString(result["error"]); }
                    return;
                }
            }
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Left && e.Key != Key.Right &&
                e.Key != Key.Return && e.Key != Key.Escape && e.Key != Key.Tab && e.Key != Key.LeftShift && e.Key != Key.RightShift)
                _typed = true;
        }
    }
}
