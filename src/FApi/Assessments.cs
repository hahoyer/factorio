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

    Classes AssessmentArea<Classes>.IConfiguration.GetNewValue(Classes classes)
    {
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

    Members AssessmentArea<Members>.IConfiguration.GetNewValue(Members members)
    {
        NotImplementedMethod(members);
        return default;
    }

    string AssessmentArea<Members>.IConfiguration.JsonPath => JsonPath;

    void AssessmentArea<Members>.IConfiguration.Log(Members result)
    {
        "------------------------".Log();
        "New members: ".Log();
        var newClasses = result.New;
        newClasses.Stringify("\n").Log();
        "------------------------".Log();
        $"  Members: {result.Irrelevant.Length + result.Relevant.Length}".Log();
        $"    relevant: {result.Relevant.Length}".Log();
        $"    irrelevant: {result.Irrelevant.Length}".Log();
        $"    new: {newClasses.Length}".Log();
        "------------------------".Log();
    }

    public bool IsRelevant(Class arg) => arg.Name.In(Classes.Current.Relevant);

    public bool IsRelevant(Class argClass, Field argField)
    {
        var specific = Members.Current.Specific.SingleOrDefault(s => s.ClassName == argClass.Name);
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