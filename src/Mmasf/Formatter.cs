using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ManageModsAndSavefiles
{
    sealed class Formatter
    {
        static BindingFlags AnyBinding
            => BindingFlags.Public
            | BindingFlags.Instance;

        const int BufferSize = short.MaxValue;

        public static T Deserialize<T>(Func<Stream> getStream) where T : new()
        {
            var position = (long) 0;
            using(var stream = getStream())
                return DeserializeInternal<T>(stream, position);
        }

        static T DeserializeInternal<T>(Stream stream, long position) where T : new()
        {
            stream.Seek(position, SeekOrigin.Begin);
            var result = new T();
            return result;
        }

        void DeserializeData(Type type, object data)
        {
            var memberCheck = GetMemberCheck(type);
            var results = type
                .GetFields(AnyBinding)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(AnyBinding))
                .Where(memberInfo => IsRelevant(memberInfo, type, data))
                .Where(memberInfo => memberCheck(memberInfo, data))
                .ToArray();
        }

        static Func<MemberInfo, object, bool> GetMemberCheck(Type type) { return (s, o) => true; }

        static bool IsRelevant(MemberInfo memberInfo, Type type, object x)
        {
            if(memberInfo.DeclaringType != type)
                return false;

            var propertyInfo = memberInfo as PropertyInfo;
            return propertyInfo == null || propertyInfo.GetIndexParameters().Length <= 0;
        }
    }
}