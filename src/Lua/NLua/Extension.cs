using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using NLua;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Lua.NLua
{
    static class Extension
    {
        static (string FileName, int Position, string Text)? ParseExceptionMessage(string target, string value)
        {
            var fullMessage = ReplendishExceptionMessage(target, value);
            if(fullMessage == null)
                return null;

            var fileNameLength = fullMessage.Substring(2).Split(':')[0].Length + 2;
            var fileName = fullMessage.Substring(0, fileNameLength);
            var positionString = fullMessage.Substring(fileNameLength + 1).Split(':')[0];
            var position = int.Parse(positionString) - 1;
            var message = fullMessage.Substring(fileNameLength + positionString.Length + 3);
            return (fileName, position, message);
        }

        static string ReplendishExceptionMessage(string target, string value)
        {
            var match = MutilatedVersions(target).Top(s => value.StartsWith("..." + s.Split('?')[0]));
            if(match == null)
                return null;

            var indexInTarget = target.IndexOf(match);
            return target.Substring(0, indexInTarget) + value.Substring(3);
        }

        static IEnumerable<string> MutilatedVersions(string target)
        {
            var head = target.Split('?')[0];
            yield return head;
            foreach(var part in (head.Length - 5).Select(i => head.Substring(i)))
                yield return part;
        }

        public static string Dump(LuaTable value)
            => Tracer.Dump(value.GetTableAsDictionary());

        static IDictionary<object, object> GetTableAsDictionary(this object target)
            => (target as LuaTable)?.GetTableAsDictionary() ?? null;

        public static IDictionary<object, object> GetTableAsDictionary(this LuaTable luaTable)
        {
            var interpreter = luaTable.GetByReflection<global::NLua.Lua>("_Interpreter");
            return interpreter.GetTableDict(luaTable);
        }

        public static string ParseExceptionMessage(this string value, IContext context)
        {
            var message = context.PackagePath
                .Select(pp => ParseExceptionMessage(pp, value))
                .Top(p => p != null, enableEmpty: false)
                .AssertValue();

            return Tracer.FilePosn(message.FileName, message.Position, 0, message.Position, 0, "Lua") + message.Text;
        }
    }
}