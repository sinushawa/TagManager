using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace TagManager
{
    public enum ConsoleElementModifier
    {
        None,
        Selection,
        Containing,
        Children,
        Not,
        Visible
    }

    [Serializable]
    public class ConsoleStringSelElement : ConsoleElement, ISerializable
    {
        public string name;
        public ConsoleElementModifier modifier;

        public ConsoleStringSelElement(string _name, ConsoleElementModifier _modifier)
        {
            name = _name;
            modifier = _modifier;
        }

        public override List<uint> getCorrespondingSel()
        {
            List<uint> sel = new List<uint>();
            if(modifier == ConsoleElementModifier.None)
            {
                TagNode _entity = TagHelperMethods.RetrieveEntityFromTag(name);
                if (!_entity.IsShortcut)
                {
                    sel = _entity.Nodes.ToList();
                }
                else
                {
                    sel = _entity.Shortcut.getCorrespondingSel();
                }
            }
            if(modifier == ConsoleElementModifier.Selection)
            {
                sel = MaxPluginUtilities.Selection.ToListHandles();
            }
            if (modifier == ConsoleElementModifier.Containing)
            {
                List<uint> children = new List<uint>();
                List<TagNode> _entities = TagHelperMethods.RetrieveEntitiesContainsTag(name);
                foreach(TagNode tagNode in _entities)
                {
                    sel.AddRange(TagHelperMethods.GetBranchObjects(tagNode));
                }
            }
            if (modifier == ConsoleElementModifier.Children)
            {
                TagNode _entity = TagHelperMethods.RetrieveEntityFromTag(name);
                sel = TagHelperMethods.GetBranchObjects(_entity);
            }
            if (modifier == ConsoleElementModifier.Not)
            {

                sel = (TagHelperMethods.GetBranchObjects(TagGlobals.root).Except(TagHelperMethods.RetrieveEntityFromTag(name).Nodes.ToList())).ToList();
            }
            if (modifier == ConsoleElementModifier.Visible)
            {
                TagNode _entity = TagHelperMethods.RetrieveEntityFromTag(name);
                List<uint> _objects = _entity.Nodes.ToList();
                sel = MaxPluginUtilities.GetVisibleNodes(_objects);
            }
            return sel;
        }
        public override string getCorrespondingStr()
        {
            return name;
        }
        protected ConsoleStringSelElement(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            modifier = (ConsoleElementModifier)info.GetValue("modifier", typeof(ConsoleElementModifier));
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("modifier", modifier, typeof(ConsoleElementModifier));
        }

    }
}
