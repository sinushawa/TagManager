using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace TagManager
{
    [Serializable]
    class ConsoleStringSelElement : ConsoleElement, ISerializable
    {
        public string name;
        public List<uint> objects;

        public ConsoleStringSelElement(string _name, List<uint> _objects)
        {
            name = _name;
            objects = _objects;
        }

        public override List<uint> getCorrespondingSel()
        {
            return objects;
        }
        public override string getCorrespondingStr()
        {
            return name;
        }
        protected ConsoleStringSelElement(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            objects = (List<uint>)info.GetValue("objects", typeof(List<uint>));
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("objects", objects, typeof(List<uint>));
        }

    }
}
