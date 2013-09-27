using System;
using UiViewModels.Actions;
namespace TagManager
{
    public class ActionRegister : CuiActionCommandAdapter
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
        public override void Execute(object param)
        {
            try
            {
                MaxPluginUtilities.WriteLine("TagManager Loaded");
            }
            catch (Exception ex)
            {
                MaxPluginUtilities.WriteLine("Error occured " + ex.Message);
            }
        }
    }
}