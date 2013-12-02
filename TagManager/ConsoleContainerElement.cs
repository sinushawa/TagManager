using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagManager
{
    public class ConsoleContainerElement :ConsoleElement
    {
        public ConsoleContainerElement parent;
        public List<ConsoleElement> content = new List<ConsoleElement>();
        public List<concat> ops = new List<concat>();
        public bool open = true;

        public ConsoleContainerElement()
        {
        }
        public ConsoleContainerElement(ConsoleContainerElement _parent)
        {
            parent = _parent;
        }

        public override List<uint> getCorrespondingSel()
        {
            List<uint> result = new List<uint>();
            try
            {
                
                // treat intersections first
                while (ops.Contains(concat.intersection) && (ops.Contains(concat.addition) || ops.Contains(concat.substraction)))
                {
                    int _index = ops.FindIndex(y => y == concat.intersection);
                    ConsoleElement c1 = content[_index];
                    ConsoleElement c2 = content[_index + 1];
                    ConsoleContainerElement _brackElem = new ConsoleContainerElement(this);
                    _brackElem.content.Add(c1);
                    _brackElem.content.Add(c2);
                    _brackElem.ops.Add(ops[_index]);
                    content.Remove(c1);
                    content.Remove(c2);
                    ops.RemoveAt(_index);
                    content.Insert(_index, _brackElem);
                }
                Queue<ConsoleElement> queuedContent = new Queue<ConsoleElement>(content);
                Queue<concat> queuedOps = new Queue<concat>(ops);
                result = queuedContent.Dequeue().getCorrespondingSel();
                while (queuedOps.Count > 0)
                {
                    result = Eval(result, queuedContent.Dequeue(), queuedOps.Dequeue());
                }
            }
            catch
            {
            }
            return result;
        }

        public override string getCorrespondingStr()
        {
            string result = "";
            try
            {
                Queue<ConsoleElement> queuedContent = new Queue<ConsoleElement>(content);
                Queue<concat> queuedOps = new Queue<concat>(ops);
                result += "[";
                result += queuedContent.Dequeue().getCorrespondingStr();
                while (queuedContent.Count > 0)
                {
                    result += queuedOps.Dequeue().GetEnumDescription();
                    result += queuedContent.Dequeue().getCorrespondingStr();
                }
                while (queuedOps.Count > 0)
                {
                    result += queuedOps.Dequeue().GetEnumDescription();
                }
                result += "]";
            }
            catch
            {
            }
            return result;
        }

        private List<uint> Eval(List<uint> c1, ConsoleElement c2, concat _ops)
        {
            List<uint> result = new List<uint>();
            if (_ops == concat.addition)
            {
                result = c1 + c2;
                return result;
            }
            if (_ops == concat.substraction)
            {
                result = c1 - c2;
                return result;
            }
            if (_ops == concat.intersection)
            {
                result = c1 * c2;
                return result;
            }
            return result;
        }
    }
}
