using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using hw.DebugFormatter;

namespace ManageModsAndSavefiles
{
    public sealed class BinaryRead : DumpableObject
    {
        readonly Stream Reader;
        public readonly long Length;
        public long Position;

        public BinaryRead(Stream reader, long length)
        {
            Reader = reader;
            Length = length;
        }

        public bool IsEnd => Position >= Length;
        public byte[] GetNextBytes(int count)
        {
            var result = GetBytes(count);
            Position += count;
            return result;
        }

        public byte[] GetBytes(int count)
        {
            Tracer.Assert(count < 1000);
            var result = new byte[count];
            Reader.Seek(Position, SeekOrigin.Begin);
            Reader.Read(result, 0, count);
            return result;
        }

        public T GetNext<T>()
        {
            if(typeof(T) == typeof(byte))
                return
                    (T)
                    (object)
                    GetNextBytes(1)[0];
            if(typeof(T) == typeof(short))
                return
                    (T)
                    (object)
                    BitConverter.ToInt16(GetNextBytes(Marshal.SizeOf(typeof(T))), 0);
            if(typeof(T) == typeof(int))
                return
                    (T)
                    (object)
                    BitConverter.ToInt32(GetNextBytes(Marshal.SizeOf(typeof(T))), 0);

            if(typeof(T) == typeof(string))
                return (T) (object) GetNextString<int>();

            NotImplementedMethod();
            return (T) (object) null;
        }

        public string GetNextString<T>()
        {
            var length = Convert.ToInt32(GetNext<T>());
            return GetNextString(length);
        }

        public string GetNextString(int length) => Encoding.UTF8.GetString(GetNextBytes(length));
    }
}                             