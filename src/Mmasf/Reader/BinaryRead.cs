using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSaveFiles.Reader;

public sealed class BinaryRead : DumpableObject
{
    public interface IContext
    {
        void Got(BinaryRead reader, MemberInfo member, object captureIdentifier, object result);
    }


    public interface ISelfReader
    {
        void ReadFromAndAdvance(BinaryRead reader);
    }

    public interface IReaderProvider
    {
        object ReadAndAdvance(BinaryRead reader, Type type, MemberInfo member);
    }

    internal interface IAdvancer
    {
        int Value { get; }
    }

    class Exception : SystemException
    {
        internal Exception(string message)
            : base(message) { }
    }

    sealed class InvalidException : Exception
    {
        public InvalidException(string message)
            : base(message) { }
    }

    sealed class InvalidArrayException : System.Exception
    {
        public InvalidArrayException(string message)
            : base(message) { }
    }

    public long Position;
    public IContext UserContext;
    readonly Stream Reader;

    public BinaryRead(Stream reader) => Reader = reader;

    public bool IsEnd => Position >= Reader.Length;

    byte[] GetNextBytes(int count)
    {
        var result = GetBytes(count);
        Position += count;
        return result;
    }

    byte[] GetBytes(int count)
    {
        (count < 10000).Assert();
        var result = new byte[count];
        Reader.Position = Position;
        Reader.Read(result, 0, count);
        return result;
    }

    public T GetNext<T>()
        where T : new() => (T)GetNext(typeof(T));

    object GetNext(Type target)
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
        if(result is ISelfReader readerResult)
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

        foreach(var ignore in a.Select(advance => advance as Ignore))
            if(ignore == null)
                AssignAndAdvance(result, member);
            else
                ignore.Execute(this, member);
    }

    static IEnumerable<MemberInfo> GetRelevantMembers(Type target) => target
        .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(IsRelevant)
        .Select
        (
            memberInfo => new
            {
                member = memberInfo, attribute = memberInfo.GetAttribute<DataItem>(false)
            }
        )
        .GroupBy(i => i.attribute.FileName)
        .Single()
        .OrderBy(i => i.attribute.LineNumber)
        .Select(i => i.member);

    static bool IsRelevant(MemberInfo member)
    {
        var dataAttribute = member.GetAttribute<DataItem>(false);
        if(dataAttribute == null)
            return false;

        var fieldInfo = member as FieldInfo;
        if(fieldInfo != null)
            return !fieldInfo.IsPrivate && !fieldInfo.IsInitOnly;

        var propertyInfo = (PropertyInfo)member;
        if(propertyInfo.GetIndexParameters().Any())
            return false;

        var accessors = propertyInfo.GetAccessors();
        return accessors.Length == 2 &&
            accessors.All(a => !a.IsPrivate) &&
            accessors.Any(a => a.ReturnType != typeof(void)) &&
            accessors.Any(a => a.ReturnType == typeof(void));
    }


    public string GetNextString<T>()
        where T : new()
    {
        var length = Convert.ToInt32(GetNext<T>());
        return GetNextString(length);
    }

    public string GetNextString(int length) => Encoding.UTF8.GetString(GetNextBytes(length));

    void AssignAndAdvance(object target, MemberInfo member)
    {
        var fieldInfo = member as FieldInfo;
        var propertyInfo = member as PropertyInfo;
        (fieldInfo != null || propertyInfo != null).Assert();

        var type = fieldInfo?.FieldType ?? propertyInfo.PropertyType;

        var value = GetNextWithOrWithoutReader(type, member);

        if(fieldInfo == null)
            propertyInfo.SetValue(target, value);
        else
            fieldInfo.SetValue(target, value);

        var ci = member.GetAttribute<DataItem>(false)?.CaptureIdentifier;
        SignalToUserContext(ci, member, value);
    }

    internal void SignalToUserContext(object captureIdentifier, MemberInfo member, object value)
    {
        if(UserContext == null || captureIdentifier == null)
            return;

        UserContext.Got(this, member, captureIdentifier, value);
    }

    object GetNextWithOrWithoutReader(Type type, MemberInfo member)
    {
        var readerType = member.GetAttribute<DataItem>(false)?.Reader;
        return readerType == null
            ? GetNext(type, member)
            : GetNextWithReader(readerType, type, member);
    }

    public object GetNext(Type type, MemberInfo member)
        => GetNext(type, member, 0);

    object GetNext(Type type, MemberInfo member, int level)
        => type.IsArray
            ? GetNextArray(type, member, level)
            : type == typeof(string)
                ? GetNextString(member)
                : GetNext(type);

    object GetNextElement(Type readerType, Type type, MemberInfo member, int level)
        => readerType == null
            ? type.IsArray
                ? GetNextArray(type, member, level)
                : type == typeof(string)
                    ? GetNextString(member)
                    : GetNext(type)
            : GetNextWithReader(readerType, type, member);

    string GetNextString(MemberInfo member)
    {
        var countType = member.GetAttribute<StringSetup>(false)?.CountType ?? typeof(int);
        var length = Convert.ToInt32(GetNext(countType));
        if(length > 1000000)
            throw new Exception($"String not recognized in stream. Length would be {length}.");
        return GetNextString(length);
    }

    object GetNextWithReader(Type readerType, Type type, MemberInfo member)
    {
        var result = ((IReaderProvider)Activator.CreateInstance(readerType))
            .ReadAndAdvance(this, type, member);
        if(result == null || result.GetType().Is(type))
            return result;

        throw new InvalidException
        (
            $"Return type of {readerType.Name} is {result.GetType().Name}, which is not a {type.Name}."
        );
    }

    object GetNextArray(Type type, MemberInfo member, int level)
    {
        var arraySetup = member
            .GetAttributes<ArrayItem>(false)
            .SingleOrDefault(i => i.Level == level);

        var count = GetCount(arraySetup);

        if(arraySetup?.MaxCount > 0 && count > arraySetup.MaxCount)
            throw new InvalidArrayException("Too big array encountered");

        var readerType = arraySetup?.Reader;

        var elementType = type.GetElementType().AssertNotNull();
        var array = Array.CreateInstance(elementType, count);
        foreach(var o in count.Select
                (
                    i => new
                    {
                        i, value = GetNextElement(readerType, elementType, member, level + 1)
                    }))
            array.SetValue(o.value, o.i);

        return array;
    }

    int GetCount(ArrayItem arrayItem)
        =>
            arrayItem?.Count > 0
                ? arrayItem.Count
                : Convert.ToInt32(GetNext(arrayItem?.CountType ?? typeof(int)));

    internal byte[] LookAhead(int count = 100) => GetBytes(count);
}