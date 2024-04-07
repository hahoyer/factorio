using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles;

sealed class ModSettings : DumpableObject
{
    internal interface IModSetting { }

    class BoolValue(bool value) : DumpableObject, IModSetting
    {
        readonly bool Value = value;
    }

    class IntValue(int value) : DumpableObject, IModSetting
    {
        readonly int Value = value;
    }

    class StringValue(string value) : DumpableObject, IModSetting
    {
        readonly string Value = value;
    }


    public Version Version;
    public Dictionary<string, object> Startup;
    public Dictionary<string, object> RuntimeGlobal;
    public Dictionary<string, object> RuntimePerUser;

    public ModSettings([CanBeNull] BinaryRead content)
    {
        if(content == null)
            return;

        Version = new(
            content.GetNext<short>(),
            content.GetNext<short>(),
            content.GetNext<short>(),
            content.GetNext<short>()
        );

        content.GetNext<byte>();
        var result = content.GetNextGeneric();

        Startup = GetDictionary(result, "startup");
        RuntimeGlobal = GetDictionary(result, "runtime-global");
        RuntimePerUser = GetDictionary(result, "runtime-per-user");

        NotImplementedMethod("content");
    }


    static Dictionary<string, object> GetDictionary(BinaryRead.Generic target, string tag)
    {
        var all = (IDictionary<string, BinaryRead.Generic>)((IDictionary<string, BinaryRead.Generic>)target.Value)[
            "startup"].Value;
        return all.ToDictionary(pair => pair.Key, pair => ToModSetting(pair.Value));
    }

    static object ToModSetting(BinaryRead.Generic value)
    {
        return ((IDictionary<string, BinaryRead.Generic>)value.Value)["value"].Value;
    }
}