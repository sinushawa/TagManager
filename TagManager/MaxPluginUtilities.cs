﻿using Autodesk.Max;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TagManager
{
    public static class MaxPluginUtilities
    {
        public static IInterface14 Interface
        {
            get
            {
                return MaxPluginUtilities.Global.COREInterface14;
            }
        }
        public static IGlobal Global
        {
            get
            {
                return GlobalInterface.Instance;
            }
        }
        public static void Write(string s, params string[] args)
        {
            MaxPluginUtilities.Write(string.Format(s, args));
        }
        public static void Write(string s)
        {
            MaxPluginUtilities.Global.TheListener.EditStream.Wputs(s);
            MaxPluginUtilities.Global.TheListener.EditStream.Flush();
        }
        public static void WriteLine(string s)
        {
            MaxPluginUtilities.Write(s);
            MaxPluginUtilities.Write("\n");
        }
        public static void WriteLine(string s, params string[] args)
        {
            MaxPluginUtilities.WriteLine(string.Format(s, args));
        }
        public static IOResult Save(this IISave isave, object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, obj);
                uint num = 0u;
                byte[] array = memoryStream.ToArray();
                isave.WriteVoid(BitConverter.GetBytes(array.Length), ref num);
                if (array.Length > 0)
                {
                    isave.WriteVoid(array, ref num);
                }
            }
            return IOResult.Ok;
        }
        public static object LoadObject(this IILoad iload)
        {
            uint num = 0u;
            if (iload.CurChunkLength < 4uL)
            {
                return null;
            }
            ulong arg_17_0 = iload.CurChunkLengthRemaining;
            byte[] array = new byte[4];
            iload.ReadVoid(array, ref num);
            int num2 = BitConverter.ToInt32(array, 0);
            ulong arg_37_0 = iload.CurChunkLengthRemaining;
            if (num == 4u && num2 > 0 && num2 == (int)iload.CurChunkLengthRemaining)
            {
                array = new byte[num2];
                iload.ReadVoid(array, ref num);
                using (MemoryStream memoryStream = new MemoryStream(array))
                {
                    try
                    {
                        object result = new BinaryFormatter().Deserialize(memoryStream);
                        return result;
                    }
                    catch (SerializationException)
                    {
                        object result = null;
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
