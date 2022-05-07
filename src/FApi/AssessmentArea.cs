using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace FactorioApi;

sealed class AssessmentArea<T> : DumpableObject
{
    internal interface IConfiguration
    {
        T Default { get; }
        string JsonPath { get; }
        T GetNewValue(T old);
        void Log(T result);
    }

    readonly IConfiguration Configuration;
    readonly ValueCache<T> NewCache;
    readonly ValueCache<T> OldCache;
    readonly ValueCache<T> CurrentCache;
    readonly ValueCache<SmbFile> File;

    public AssessmentArea(IConfiguration configuration)
    {
        Configuration = configuration;
        OldCache = new(GetOldValue);
        NewCache = new(() => Configuration.GetNewValue(OldCache.Value));
        CurrentCache = new(GetCurrentValue);
        File = new(GetFile);
    }

    internal T Current => CurrentCache.Value;

    T GetCurrentValue()
    {
        if(NewCache.Value == null)
            return OldCache.Value;

        var result = NewCache.Value;
        File.Value.String = result.ToJSon();
        "------------------------".Log();
        $"New assessment saved to {File.Value.FullName}".Log();
        Configuration.Log(result);
        return result;
    }

    T GetOldValue() => File.Value.String.FromJson<T>();

    SmbFile GetFile()
    {
        var result = Configuration.JsonPath.ToSmbFile() / "Assessment" / (typeof(T).Name + ".json");
        if(!result.Exists)
            result.String = Configuration.Default.ToJSon();
        return result;
    }
}