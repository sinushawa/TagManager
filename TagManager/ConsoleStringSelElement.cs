using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagManager
{
    class ConsoleStringSelElement : ConsoleElement
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

    }
}
