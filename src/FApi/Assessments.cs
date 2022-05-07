using System.Collections.Generic;
using System.Linq;
using FactorioApi.Assessment;
using hw.DebugFormatter;
using hw.Helper;

namespace FactorioApi;

sealed class Assessments
    : DumpableObject
        , AssessmentArea<Classes>.IConfiguration
        , AssessmentArea<Members>.IConfiguration
{
    readonly string JsonPath;
    readonly GameApi GameApi;

    readonly AssessmentArea<Classes> Classes;
    readonly AssessmentArea<Members> Members;

    public Assessments(string jsonPath, GameApi gameApi)
    {
        JsonPath = jsonPath;
        GameApi = gameApi;
        Classes = new(this);
        Members = new(this);
    }

    Classes AssessmentArea<Classes>.IConfiguration.Default => new() { FactorioVersion = GameApi.Version };

    Classes AssessmentArea<Classes>.IConfiguration.GetNewValue(Classes old)
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

        if(!@new.Any())
            return null;

        var result = new Classes
        {
            FactorioVersion = GameApi.Version
            , Irrelevant = old.Irrelevant ?? new string[0]
            , Relevant = old.Relevant ?? new string[0]
            , New = @new
        };
        return result;
    }

    string AssessmentArea<Classes>.IConfiguration.JsonPath => JsonPath;

    void AssessmentArea<Classes>.IConfiguration.Log(Classes result)
    {
        "------------------------".Log();
        "New classes: ".Log();
        var newClasses = result.New;
        newClasses.Stringify("\n").Log();
        "------------------------".Log();
        $"  Classes: {result.Irrelevant.Length + result.Relevant.Length}".Log();
        $"    relevant: {result.Relevant.Length}".Log();
        $"    irrelevant: {result.Irrelevant.Length}".Log();
        $"    new: {newClasses.Length}".Log();
        "------------------------".Log();
    }

    Members AssessmentArea<Members>.IConfiguration.Default
        => new()
        {
            FactorioVersion = GameApi.Version
        };

    Members AssessmentArea<Members>.IConfiguration.GetNewValue(Members old)
    {
        var known = T(
                old.AlwaysIrrelevant,
                old.AlwaysRelevant,
                old.OtherwiseIrrelevant,
                old.OtherwiseRelevant
            )
            .ConcatMany()
            .ToArray();

        var @new = GameApi
            .Classes
            .SelectMany(GetMemberNames)
            .OrderBy(name => name)
            .Distinct()
            .Where(name => !name.In(known))
            .ToArray();

        if(!@new.Any())
            return null;

        var result = new Members
        {
            FactorioVersion = GameApi.Version
            , AlwaysIrrelevant = old.AlwaysIrrelevant ?? new string[0]
            , AlwaysRelevant = old.AlwaysRelevant ?? new string[0]
            , OtherwiseIrrelevant = old.OtherwiseIrrelevant ?? new string[0]
            , OtherwiseRelevant = old.OtherwiseRelevant ?? new string[0]
            , Specific = old.Specific ?? new ClassMembers[0]
            , New = @new
        };
        return result;
    }

    string AssessmentArea<Members>.IConfiguration.JsonPath => JsonPath;

    void AssessmentArea<Members>.IConfiguration.Log(Members result)
    {
        "------------------------".Log();
        "New members: ".Log();
        var @new = result.New;
        @new.Stringify("\n").Log();
        "------------------------".Log();
    }

    static IEnumerable<string> GetMemberNames(Class arg)
        => arg.Attributes.Select(item => item.Name);

    public bool IsRelevant(Class arg) => arg.Name.In(Classes.Current.Relevant);

    public bool IsRelevant(Class argClass, Field argField)
    {
        if(argField.Name.In(Members.Current.AlwaysRelevant))
            return true;
        if(argField.Name.In(Members.Current.AlwaysIrrelevant))
            return false;

        var specific = Members.Current.Specific.SingleOrDefault(s => s.ClassName == argClass.Name);
        if(specific != null)
        {
            if(argField.Name.In(specific.Relevant))
                return true;
            if(argField.Name.In(specific.Irrelevant))
                return false;
        }

        return argField.Name.In(Members.Current.OtherwiseRelevant);
    }
}