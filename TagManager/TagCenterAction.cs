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
                TagCenter.Instance.CreateWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error");
            }
        }
    }
}
