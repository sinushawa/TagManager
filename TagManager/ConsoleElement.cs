using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TagManager
{
    public enum concat
    {
        [Description("+")]
        addition,
        [Description("-")]
        substraction,
        [Description("*")]
        intersection,
        [Description("/")]
        except
    }

    [Serializable]
    public abstract class ConsoleElement : IConsoleSelElement, ISerializable
    {

        public ConsoleElement()
        {
        }



        public abstract List<uint> getCorrespondingSel();
        public abstract string getCorrespondingStr();

        public static List<uint> operator +(ConsoleElement c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.getCorrespondingSel());
            result.AddRange(c2.getCorrespondingSel());
            result=result.Distinct().ToList();
            return result;
        }
        public static List<uint> operator +(List<uint> c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1);
            result.AddRange(c2.getCorrespondingSel());
            result = result.Distinct().ToList();
            return result;
        }
        public static List<uint> operator -(ConsoleElement c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.getCorrespondingSel().Where(x=> !c2.getCorrespondingSel().Any(y=> x==y)) );
            result = result.Distinct().ToList();
            return result;
        }
        public static List<uint> operator -(List<uint> c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.Where(x => !c2.getCorrespondingSel().Any(y => x == y)));
            result = result.Distinct().ToList();
            return result;
        }
        public static List<uint> operator *(ConsoleElement c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.getCorrespondingSel().Intersect(c2.getCorrespondingSel()));
            return result;
        }
        public static List<uint> operator *(List<uint> c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.Intersect(c2.getCorrespondingSel()));
            return result;
        }
        public static List<uint> operator /(ConsoleElement c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.getCorrespondingSel().Except(c2.getCorrespondingSel()));
            return result;
        }
        public static List<uint> operator /(List<uint> c1, ConsoleElement c2)
        {
            List<uint> result = new List<uint>();
            result.AddRange(c1.Except(c2.getCorrespondingSel()));
            return result;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}
