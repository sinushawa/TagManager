using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagManager
{
    interface IConsoleSelElement
    {
        List<uint> getCorrespondingSel();
        string getCorrespondingStr();
    }
}
