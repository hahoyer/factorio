using System.Linq;
using FactorioApi.Assessment;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace FactorioApi;

sealed class Assessments
    : DumpableObject
        , Assessments.Area<Classes>.IConfiguration
        , Assessments.Area<Members>.IConfiguration
{
    sealed class Area<T> : DumpableObject
    {
        internal interface IConfiguration
        {
            T Default { get; }
            string JsonPath { get; }
            T GetNewValue();
            void Log();
        }

        readonly IConfiguration Configuration;
        readonly ValueCache<T> NewCache;
        readonly ValueCache<T> PersistentCache;
        readonly ValueCache<SmbFile> File;

        public Area(IConfiguration configuration)
        {
            Configuration = configuration;
            PersistentCache = new(GetPersistentValue);
            NewCache = new(Configuration.GetNewValue);
            File = new(GetFile);
        }

        internal T Persistent => PersistentCache.Value;

        internal T New => NewCache.Value;

        internal void Save()
        {
            if(New == null)
                return;

            var result = New;
            File.Value.String = result.ToJSon();
            "------------------------".Log();
            $"New assessment saved to {File.Value.FullName}".Log();
            Configuration.Log();
        }

        T GetPersistentValue() => File.Value.String.FromJson<T>();

        SmbFile GetFile()
        {
            var result = Configuration.JsonPath.ToSmbFile() / "Assessment" / (typeof(T).Name + ".json");
            if(!result.Exists)
                result.String = Configuration.Default.ToJSon();
            return result;
        }
    }

    readonly string JsonPath;
    readonly GameApi GameApi;

    readonly Area<Classes> Classes;
    readonly Area<Members> Members;

    public Assessments(string jsonPath, GameApi gameApi)
    {
        JsonPath = jsonPath;
        GameApi = gameApi;
        Classes = new(this);
        Members = new(this);
    }

    Classes Area<Classes>.IConfiguration.Default => new() { FactorioVersion = GameApi.Version };

    Classes Area<Classes>.IConfiguration.GetNewValue()
    {
        var classes = Classes.Persistent;
        var relevantClasses = classes.Relevant ?? new string[0];
        var irrelevantClasses = classes.Irrelevant ?? new string[0];
        var knownClasses = relevantClasses.Concat(irrelevantClasses).ToArray();

        var newClasses = GameApi
            .Classes
            .Where(c => !c.Name.In(knownClasses))
            .Select(c => c.Name)
            .ToArray();

        if(!newClasses.Any())
            return null;

        var result = new Classes
        {
            FactorioVersion = GameApi.Version
            , Irrelevant = irrelevantClasses
            , Relevant = relevantClasses
            , New = newClasses
        };
        return result;
    }

    string Area<Classes>.IConfiguration.JsonPath => JsonPath;

    void Area<Classes>.IConfiguration.Log()
    {
        "------------------------".Log();
        "New classes: ".Log();
        var newClasses = Classes.New.New;
        newClasses.Stringify("\n").Log();
        "------------------------".Log();
        $"  Classes: {Classes.New.Irrelevant.Length + Classes.New.Relevant.Length}".Log();
        $"    relevant: {Classes.New.Relevant.Length}".Log();
        $"    irrelevant: {Classes.New.Irrelevant.Length}".Log();
        $"    new: {newClasses.Length}".Log();
        "------------------------".Log();
    }

    Members Area<Members>.IConfiguration.Default
        => new()
        {
            FactorioVersion = GameApi.Version
        };

    Members Area<Members>.IConfiguration.GetNewValue()
    {
        var classes = Classes.Persistent;
        var relevantClasses = classes.Relevant ?? new string[0];
        var irrelevantClasses = classes.Irrelevant ?? new string[0];
        var knownClasses = relevantClasses.Concat(irrelevantClasses).ToArray();

        var members = Members.Persistent;
        var relevantMembers = members.Relevant ?? new string[0];
        var irrelevantMembers = members.Irrelevant ?? new string[0];
        var knownMembers = relevantMembers.Concat(irrelevantMembers).ToArray();

        var special = Members.Persistent.Specific.ToDictionary(m => m.ClassName);

        var classGrouping = GameApi
            .Classes
            .GroupBy(c => c.Name.In(knownClasses)? (bool?)null : special[c.Name] == null);

        var newClasses = GameApi
            .Classes
            .Where(c => !c.Name.In(knownClasses))
            .Select(c => c.Name)
            .ToArray();

        if(!newClasses.Any())
            return null;

        var result = new Members
        {
            FactorioVersion = GameApi.Version
            , Irrelevant = irrelevantMembers
            , Relevant = relevantMembers
            , New = newClasses
        };
        return result;
    }

    string Area<Members>.IConfiguration.JsonPath => JsonPath;

    void Area<Members>.IConfiguration.Log()
    {
        "------------------------".Log();
        "New members: ".Log();
        var newClasses = Classes.New.New;
        newClasses.Stringify("\n").Log();
        "------------------------".Log();
        $"  Members: {Classes.New.Irrelevant.Length + Classes.New.Relevant.Length}".Log();
        $"    relevant: {Classes.New.Relevant.Length}".Log();
        $"    irrelevant: {Classes.New.Irrelevant.Length}".Log();
        $"    new: {newClasses.Length}".Log();
        "------------------------".Log();
    }

    public bool IsRelevant(Class arg) => arg.Name.In(Classes.New.Relevant);

    public bool IsRelevant(Class argClass, Field argField)
    {
        var specific = Members.New.Specific.SingleOrDefault(s => s.ClassName == argClass.Name);
        if(specific != null)
        {
            if(argField.Name.In(specific.Relevant))
                return true;
            if(argField.Name.In(specific.Irrelevant))
                return false;
        }

        return argField.Name.In(Members.New.Relevant);
    }
}