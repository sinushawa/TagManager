using System;
using System.Windows.Forms;
using UiViewModels.Actions;

namespace TagManager
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
}
