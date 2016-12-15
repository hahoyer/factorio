using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSavefiles.Reader
{
    public class BinaryRead : DumpableObject
    {
        readonly Stream Reader;
        public long Position;
        public IContext UserContext;

        public interface IContext
        {
            void Got(BinaryRead reader, MemberInfo member, object captureIdentifier, object result);
        }

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
            var readerResult = result as ISelfReader;
            if(readerResult != null)
            {
                readerResult.ReadFromAndAdvance(this);
                return result;
            }

            var members = GetRelevantMembers(target);

            foreach(var member in members)
                AssignAndAdvance(member, result);

            return result;
        }

        void AssignAndAdvance(MemberInfo member, object result)
        {
            var a = member.GetAttributes<Ignore>(false)
                .Cast<IAdvancer>()
                .Concat(member.GetAttributes<DataItem>(false))
                .OrderBy(aa => aa.Value);

            foreach(var ignore in a.Select(advancer => advancer as Ignore))
                if(ignore == null)
                    AssignAndAdvance(result, member);
                else
                    ignore.Execute(this, member);
        }

        static IEnumerable<MemberInfo> GetRelevantMembers(Type target)
        {
            return target
                .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(IsRelevant)
                .Select
                (
                    memberInfo => new
                    {
                        member = memberInfo,
                        attribute = memberInfo.GetAttribute<DataItem>(false)
                    }
                )
                .GroupBy(i => i.attribute.FileName)
                .Single()
                .OrderBy(i => i.attribute.LineNumber)
                .Select(i => i.member);
        }

        static bool IsRelevant(MemberInfo member)
        {
            var dataAttribute = member.GetAttribute<DataItem>(false);
            if(dataAttribute == null)
                return false;

            var fieldInfo = member as FieldInfo;
            if(fieldInfo != null)
                return !fieldInfo.IsPrivate && !fieldInfo.IsInitOnly;

            var propertyInfo = (PropertyInfo) member;
            if(propertyInfo.GetIndexParameters().Any())
                return false;

            var accessors = propertyInfo.GetAccessors();
            return accessors.Length == 2
                   && accessors.All(a => !a.IsPrivate)
                   && accessors.Any(a => a.ReturnType != typeof(void))
                   && accessors.Any(a => a.ReturnType == typeof(void));
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

        internal void AssignAndAdvance(object target, MemberInfo member)
        {
            object value;
            var fieldInfo = member as FieldInfo;
            if(fieldInfo == null)
            {
                var propertyInfo = (PropertyInfo) member;
                value = GetNext(propertyInfo.PropertyType, member);
                propertyInfo.SetValue(target, value);
            }
            else
            {
                value = GetNext(fieldInfo.FieldType, member);
                fieldInfo.SetValue(target, value);
            }

            var ci = member.GetAttribute<DataItem>(false)?.CaptureIdentifier;
            SignalToUserContext(ci, member,value);
        }

        internal void SignalToUserContext(object captureIdentifier, MemberInfo member, object value)
        {
            if (UserContext == null || captureIdentifier == null)
                return;
            UserContext.Got(this, member, captureIdentifier, value);
        }

        object GetNext(Type type, MemberInfo member, int level = 0)
        {
            var reader = member.GetAttribute<DataItem>(false)?.Reader;
            if(reader != null)
                return GetNextWithReader(type, reader);

            return type.IsArray ? GetNextArray(type, member, level) : GetNext(type);
        }

        object GetNextWithReader(Type type, Type reader)
        {
            var result = ((IReaderProvider) Activator.CreateInstance(reader))
                .ReadAndAdvance(this);
            if(result.GetType().Is(type))
                return result;

            throw new InvalidException
            (
                $"Return type of {reader.Name} is {result.GetType().Name}, whitch is not a {type.Name}."
            );
        }

        sealed class InvalidException : Exception
        {
            public InvalidException(string message)
                : base(message) { }
        }

        object GetNextArray(Type type, MemberInfo member, int level)
        {
            var arraySetup = member.GetAttributes<ArraySetup>(false).Single(i => i.Level == level);
            var count = arraySetup.Count;
            if(arraySetup.CountType != null)
                count = Convert.ToInt32(GetNext(arraySetup.CountType));

            if(arraySetup.MaxCount > 0 && count > arraySetup.MaxCount)
                throw new InvalidArrayException("Too big array encountered");

            var elementType = type.GetElementType();
            var array = Array.CreateInstance(elementType, count);
            foreach(var o in count.Select
            (
                i => new
                {
                    i,
                    value = GetNext(elementType, member, level + 1)
                }))
                array.SetValue(o.value, o.i);

            return array;
        }

        public sealed class InvalidArrayException : Exception
        {
            public InvalidArrayException(string message)
                : base(message) { }
        }

        internal interface IAdvancer
        {
            int Value { get; }
        }


        public interface ISelfReader
        {
            void ReadFromAndAdvance(BinaryRead reader);
        }

        public interface IReaderProvider
        {
            object ReadAndAdvance(BinaryRead reader);
        }

    }
}