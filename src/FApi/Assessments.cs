using System.Linq;
using FactorioApi.Assessment;
using hw.DebugFormatter;
using hw.Helper;

namespace FactorioApi;

sealed class Assessments
    : DumpableObject
        , AssessmentDomain<Classes>.IConfiguration
        , AssessmentDomain<Members>.IConfiguration
{
    readonly string JsonPath;
    readonly GameApi GameApi;

    readonly AssessmentDomain<Classes> Classes;
    readonly AssessmentDomain<Members> Members;

    public Assessments(string jsonPath, GameApi gameApi)
    {
        JsonPath = jsonPath;
        GameApi = gameApi;
        Classes = new(this);
        Members = new(this);
    }

    Classes AssessmentDomain<Classes>.IConfiguration.Default => new() { FactorioVersion = GameApi.Version };

    Classes AssessmentDomain<Classes>.IConfiguration.GetNewValue(Classes old)
    {
        var known = T(
                old.Irrelevant,
                old.Relevant
            )
            .ConcatMany()
            .ToArray();

        var @new = GameApi
            .Classes
            .Where(c => !c.Name.In(known))
            .Select(c => c.Name)
            .ToArray();

        if(!@new.Any() && old.New.Length == 0)
            return null;

        var result = new Classes
        {
            FactorioVersion = GameApi.Version
            , Irrelevant = UnifyStrings(old.Irrelevant)
            , Relevant = UnifyStrings(old.Relevant)
            , New = UnifyStrings(@new)
        };
        return result;
    }

    string AssessmentDomain<Classes>.IConfiguration.JsonPath => JsonPath;

    Members AssessmentDomain<Members>.IConfiguration.Default
        => new()
        {
            FactorioVersion = GameApi.Version
        };

    Members AssessmentDomain<Members>.IConfiguration.GetNewValue(Members old)
    {
        var known = T(
                old.AlwaysIrrelevant,
                old.AlwaysRelevant,
                old.Irrelevant,
                old.Relevant
            )
            .ConcatMany()
            .ToArray();

        var @new = GameApi
            .Classes
            .Where(@class => !IsIrrelevant(@class))
            .SelectMany(GetMemberNames)
            .OrderBy(name => name)
            .Distinct()
            .Where(name => !name.In(known))
            .ToArray();

        if(!@new.Any() && old.New.Length == 0)
            return null;

        var newClasses = GameApi
            .Classes
            .Where(@class => !IsIrrelevant(@class))
            .Select(@class => GetNewClassForMembers(@class, known))
            .Where(@class => @class != null)
            .ToArray();

        var rescan = old
            .RescanClasses?
            .Select(GetNewClassForMembers)
            .ToArray();


        var result = new Members
        {
            FactorioVersion = GameApi.Version
            , AlwaysIrrelevant = UnifyStrings(old.AlwaysIrrelevant)
            , AlwaysRelevant = UnifyStrings(old.AlwaysRelevant)
            , Irrelevant = UnifyStrings(old.Irrelevant)
            , Relevant = UnifyStrings(old.Relevant)
            , Specific = UnifyStrings(old.Specific)
            , RescanClasses = new string[0]
            , New = newClasses.Any()? newClasses : rescan
        };
        return result;
    }

    string AssessmentDomain<Members>.IConfiguration.JsonPath => JsonPath;

    public bool HasNewEntries => Classes.Current.New.Length + Members.Current.New.Length > 0;

    ClassMembers GetNewClassForMembers(string name)
    {
        var @class = GameApi.Classes.Single(@class => @class.Name == name);
        return GetNewClassForMembers(@class, new string[0]);
    }

    ClassMembers[] UnifyStrings(ClassMembers[] target)
    {
        if(target == null)
            return null;

        NotImplementedMethod(target, "", "");
        return default;
    }

    static string[] UnifyStrings(string[] target)
        => target == null? new string[0] : target.Distinct().OrderBy(item => item).ToArray();

    static ClassMembers GetNewClassForMembers(Class arg, string[] knownMembers)
    {
        var newMembers = GetMemberNames(arg).Except(knownMembers).ToArray();
        if(newMembers.Any())
            return new()
            {
                ClassName = arg.Name
                , New = newMembers
            };

        return null;
    }

    static string[] GetMemberNames(Class arg)
        => arg.Attributes.Select(item => item.Name).ToArray();

    public bool IsRelevant(Class arg) => arg.Name.In(Classes.Current.Relevant);
    bool IsIrrelevant(Class arg) => arg.Name.In(Classes.Current.Irrelevant);

    public bool IsRelevant(Class argClass, Field argField)
    {
        if(argField.Name.In(Members.Current.AlwaysRelevant))
            return true;
        if(argField.Name.In(Members.Current.AlwaysIrrelevant))
            return false;

        var specific = Members.Current.Specific?.SingleOrDefault(s => s.ClassName == argClass.Name);
        if(specific != null)
        {
            if(argField.Name.In(specific.Relevant))
                return true;
            if(argField.Name.In(specific.Irrelevant))
                return false;
        }

        return argField.Name.In(Members.Current.Relevant);
    }
}