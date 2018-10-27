using System.Reflection;
using hw.Helper;
using Lua.NLua;

namespace Lua
{
    public static class Extension
    {
        static BindingFlags AnyBinding => BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        public static IContext Instance => HWLuaContext.Instance;

        public static T GetByReflection<T>(this object target, string name)
        {
            var t = target.GetType();
            var info = t
                .GetMember(name, AnyBinding)
                .Top
                (
                    IsRelevantForGetByReflection<T>,
                    enableMultiple: false
                );
            return info == null ? default(T) : (T) target.InvokeValue(info);
        }

        static bool IsRelevantForGetByReflection<T>(MemberInfo info)
        {
            switch(info.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo) info).FieldType == typeof(T);
                case MemberTypes.Property:
                {
                    var propertyInfo = (PropertyInfo) info;
                    return propertyInfo.CanRead && propertyInfo.PropertyType == typeof(T);
                }
                default: return false;
            }
        }
    }
}