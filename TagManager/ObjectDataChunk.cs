using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TagManager
{
    [Serializable]
    public class ObjectDataChunk
    {
        public List<Guid> entitiesIDs;

        public ObjectDataChunk(List<TagNode> _entities)
        {
            entitiesIDs = new List<Guid>();
            foreach (TagNode _entity in _entities)
            {
                entitiesIDs.Add(_entity.ID);
            }
        }

        public ObjectDataChunk(List<Guid> list)
        {
            entitiesIDs = list;
        }
        
        public static ObjectDataChunk ByteArrayToObjectDataChunk(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            ObjectDataChunk _data = (ObjectDataChunk)binForm.Deserialize(memStream);
            return _data;
        }
    }
    public static class ObjectDataChunkExtensions
    {
        public static byte[] ToByteArray(this ObjectDataChunk _data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, _data);
            return ms.ToArray();
        }
    }
}
