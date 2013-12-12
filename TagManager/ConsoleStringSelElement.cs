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
        Children
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
                sel = _entity.Nodes.ToList();
            }
            if(modifier == ConsoleElementModifier.Selection)
            {
                sel = MaxPluginUtilities.Selection.ToListHandles();
            }
            if (modifier == ConsoleElementModifier.Containing)
            {
                sel = TagHelperMethods.RetrieveEntitiesContainsTag(name).SelectMany((TagNode x) => x.Nodes).ToList<uint>();
            }
            if (modifier == ConsoleElementModifier.Children)
            {
                TagNode _entity = TagHelperMethods.RetrieveEntityFromTag(name);
                List<TagNode> _entities = _entity.GetNodeList();
                sel = _entities.SelectMany(x => x.Nodes).ToList<uint>();
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
