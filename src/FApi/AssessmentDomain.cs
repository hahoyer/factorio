using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace FactorioApi;

sealed class AssessmentDomain<T> : DumpableObject
    where T : class, AssessmentDomain<T>.ITarget
{
    internal interface ITarget
    {
        int NewLength { get; }
    }

    internal interface IConfiguration
    {
        T Default { get; }
        string JsonPath { get; }
        T GetNewValue(T old);
    }

    readonly IConfiguration Configuration;
    readonly ValueCache<T> NewCache;
    readonly ValueCache<T> OldCache;
    readonly ValueCache<T> CurrentCache;
    readonly ValueCache<SmbFile> File;

    public AssessmentDomain(IConfiguration configuration)
    {
        Configuration = configuration;
        OldCache = new(GetOldValue);
        NewCache = new(GetNewValue);
        CurrentCache = new(GetCurrentValue);
        File = new(GetFile);
    }

    internal T Current => CurrentCache.Value;

    T GetNewValue() => Configuration.GetNewValue(OldCache.Value);

    T GetCurrentValue()
    {
        if(NewCache.Value == null)
            return OldCache.Value;

        var result = NewCache.Value;
        File.Value.String = result.ToJSon();
        $"***Information: New assessment saved to {File.Value.FullName.Quote()}.".Log();
        if(result.NewLength > 0)
            $"***Information: Number of new {typeof(T).Name} found: {result.NewLength}.".Log();

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