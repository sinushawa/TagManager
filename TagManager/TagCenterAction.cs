using System;
using System.Windows.Forms;
using UiViewModels.Actions;

namespace TagManager.Actions
{
    public class OpenClose : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "TagManager";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagCenter.Instance.CreateTagManagerWin();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class FastTag : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "FastTag";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagCenter.Instance.CreateFastTagWin();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ContainingEntity : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Containing Entity";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.SelectEntityHoldingObject();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ContainingEntityGrow : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Containing Entity Grow";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.GrowEntity();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ContainingEntityShrink : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Containing Entity Shrink";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.ShrinkEntity();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class RemoveObjectsFromEntities : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Remove objects from entities";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.RemoveObjectsFromEntities();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ToggleAutoCloneTag : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Toggle AutoClone Tag";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagGlobals.autoCloneTag = !TagGlobals.autoCloneTag;
                TagGlobals.SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class CreateSelectionSet : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "To Selection Set";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.CreateSelectionSetFromEntities(TagGlobals.root, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ApplyEntityFromLayer : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Entity from layer";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.ApplyEntityFromLayer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }

    public class ApplyLastEntity : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Apply the last Entity";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.ApplyLastEntity();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }

    public class CopyEntity : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Get the selected Object Entities";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagMethods.CopyEntities();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ToggleAutoRename : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Toggle AutoRename";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagGlobals.autoRename = !TagGlobals.autoRename;
                TagGlobals.SaveSettings();
                TagGlobals.tagCenter.fastPan.RefreshStatusBar();
                string _s = "AutoRename is " + TagGlobals.autoRename.ToString();
                MaxPluginUtilities.Interface.DisplayTempPrompt(_s, 5000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
    public class ToggleAutoLayer : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get
            {
                return "Toggle AutoLayer";
            }
        }
        public override string Category
        {
            get
            {
                return "Robin plugins";
            }
        }
        public override string InternalActionText
        {
            get
            {
                return this.ActionText;
            }
        }
        public override string InternalCategory
        {
            get
            {
                return this.Category;
            }
        }
        public override void Execute(object parameter)
        {
            try
            {
                TagGlobals.autoLayer = !TagGlobals.autoLayer;
                TagGlobals.SaveSettings();
                TagGlobals.tagCenter.fastPan.RefreshStatusBar();
                string _s = "AutoLayer is " + TagGlobals.autoLayer.ToString();
                MaxPluginUtilities.Interface.DisplayTempPrompt(_s, 5000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
}
