using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class BinaryRead : DumpableObject
    {
        readonly Stream Reader;
        public long Position;

        public BinaryRead(Stream reader) { Reader = reader; }

        public bool IsEnd => Position >= Reader.Length;

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
            Reader.Position = Position;
            Reader.Read(result, 0, count);
            return result;
        }

        public T GetNext<T>() where T : new() => (T) GetNext(typeof(T));

        public object GetNext(Type target)
        {
            if(target == typeof(byte))
                return GetNextBytes(1)[0];
            if(target == typeof(short))
                return BitConverter.ToInt16(GetNextBytes(Marshal.SizeOf(target)), 0);
            if(target == typeof(int))
                return BitConverter.ToInt32(GetNextBytes(Marshal.SizeOf(target)), 0);

            if(target == typeof(string))
                return GetNextString<int>();

            var result = Activator.CreateInstance(target);
            var readerResult = result as IReaderProvider;
            if(readerResult != null)
            {
                readerResult.ReadFromAndAdvance(this);
                return result;
            }

            var members = target
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsRelevant)
                .Select
                (
                    memberInfo => new
                    {
                        member = memberInfo,
                        attribute = memberInfo.GetAttribute<Data>(false)
                    }
                )
                .GroupBy(i => i.attribute.FileName)
                .Single()
                .OrderBy(i => i.attribute.LineNumber);

            foreach(var member in members)
                Data.AssignValue(result, member.member, this);

            return result;
        }

        static bool IsRelevant(MemberInfo member)
        {
            var dataAttribute = member.GetAttribute<Data>(false);
            if(dataAttribute == null)
                return false;

            var fieldInfo = member as FieldInfo;
            if(fieldInfo != null)
                return !fieldInfo.IsInitOnly;

            var propertyInfo = (PropertyInfo) member;
            return !propertyInfo.GetIndexParameters().Any()
                   && propertyInfo.GetAccessors().Any(a => a.ReturnType != typeof(void));
        }


        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public sealed class Data : Attribute
        {
            internal readonly string FileName;
            internal readonly int LineNumber;

            public Data
                ([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
            {
                FileName = fileName;
                LineNumber = lineNumber;
            }

            internal static void AssignValue(object target, MemberInfo member, BinaryRead reader)
            {
                var fieldInfo = member as FieldInfo;
                if(fieldInfo == null)
                {
                    var propertyInfo = (PropertyInfo) member;
                    propertyInfo.SetValue(target, GetValue(propertyInfo.PropertyType, reader, member));
                }
                else
                    fieldInfo.SetValue(target, GetValue(fieldInfo.FieldType, reader, member));
            }

            static object GetValue(Type type, BinaryRead reader, MemberInfo member, int level = 0)
            {
                if(!type.IsArray)
                    return reader.GetNext(type);

                var arraySetup = member.GetAttributes<ArraySetup>(false).Single(i => i.Level == level);
                var count = arraySetup.Count;
                if(arraySetup.CountType != null)
                    count = Convert.ToInt32(reader.GetNext(arraySetup.CountType));

                if(arraySetup.MaxCount > 0 && count > arraySetup.MaxCount)
                    throw new InvalidArrayException("Too big array encountered");

                var elementType = type.GetElementType();
                var result = Array.CreateInstance(elementType, count);
                foreach(var o in count.Select(i => new {i, value = GetValue(elementType, reader, member, level + 1)}))
                    result.SetValue(o.value, o.i);
                return result;
            }

            public sealed class InvalidArrayException : Exception
            {
                public InvalidArrayException(string message)
                    : base(message) { }
            }
        }


        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public sealed class ArraySetup : Attribute
        {
            public int Level = 0;
            public readonly Type CountType;
            public readonly int Count;
            public int MaxCount;

            public ArraySetup(int count) { Count = count; }
            public ArraySetup(Type countType) { CountType = countType; }

            public void AssertValid()
            {
                if(CountType == null && Count == 0)
                    throw new InvalidException("CountType and Count not set");
                if(CountType != null && Count > 0)
                    throw new InvalidException("CountType and Count are both set");
                if(Count < 0)
                    throw new InvalidException("Count is negative");
            }

            public sealed class InvalidException : Exception
            {
                public InvalidException(string message)
                    : base(message) { }
            }
        }

        public interface IReaderProvider
        {
            void ReadFromAndAdvance(BinaryRead reader);
        }

        public string GetNextString<T>()
            where T : new()
        {
            var length = Convert.ToInt32(GetNext<T>());
            return GetNextString(length);
        }

        public T[] GetNextArray<TLength, T>(int? maxLength = null)
            where TLength : new()
            where T : new()
        {
            var length = Convert.ToInt32(GetNext<TLength>());
            if(maxLength != null)
                Tracer.Assert(length < maxLength.Value);
            return GetNextArray<T>(length);
        }

        public T[] GetNextArray<T>(int length)
            where T : new()
        {
            return length
                .Select(i => GetNext<T>())
                .ToArray();
        }

        public string GetNextString(int length) => Encoding.UTF8.GetString(GetNextBytes(length));
    }
}