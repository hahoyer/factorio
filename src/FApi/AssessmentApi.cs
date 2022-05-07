using Newtonsoft.Json;

namespace FactorioApi.Assessment;

sealed class Classes
{
    [JsonProperty]
    public string FactorioVersion;

    [JsonProperty]
    public string[] Relevant;

    [JsonProperty]
    public string[] Irrelevant;

    [JsonProperty]
    public string[] New;
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
{
    [JsonProperty]
    public string FactorioVersion;

    [JsonProperty]
    public ClassMembers[] Specific;

    [JsonProperty]
    public string[] AlwaysRelevant;

    [JsonProperty]
    public string[] OtherwiseRelevant;

    [JsonProperty]
    public string[] AlwaysIrrelevant;

    [JsonProperty]
    public string[] OtherwiseIrrelevant;

    [JsonProperty]
    public string[] New;
}