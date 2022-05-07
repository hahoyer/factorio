using Newtonsoft.Json;

namespace FactorioApi.Assessment;

sealed class Classes
    : AssessmentDomain<Classes>.ITarget
{
    [JsonProperty]
    public string FactorioVersion;

    [JsonProperty]
    public string[] Relevant;

    [JsonProperty]
    public string[] Irrelevant;

    [JsonProperty]
    public string[] New;

    int AssessmentDomain<Classes>.ITarget.NewLength => New.Length;
}

sealed class ClassMembers
{
    [JsonProperty]
    public string ClassName;

    [JsonProperty]
    public string[] Relevant;

    [JsonProperty]
    public string[] Irrelevant;

    [JsonProperty]
    public string[] New;
}

sealed class Members
    : AssessmentDomain<Members>.ITarget
{
    [JsonProperty]
    public string FactorioVersion;

    [JsonProperty]
    public string[] AlwaysRelevant;

    [JsonProperty]
    public string[] AlwaysIrrelevant;

    [JsonProperty]
    public ClassMembers[] Specific;

    [JsonProperty]
    public string[] Relevant;

    [JsonProperty]
    public string[] Irrelevant;

    [JsonProperty]
    public string[] RescanClasses;

    [JsonProperty]
    public ClassMembers[] New;

    int AssessmentDomain<Members>.ITarget.NewLength => New.Length;
}