using Newtonsoft.Json;

namespace FactorioApi;

sealed class GameApi
{
    [JsonProperty(PropertyName = "application")]
    public string Application;

    [JsonProperty(PropertyName = "stage")]
    public string Stage;

    [JsonProperty(PropertyName = "application_version")]
    public string Version;

    [JsonProperty(PropertyName = "api_version")]
    public int ApiVersion;

    [JsonProperty(PropertyName = "classes")]
    public Class[] Classes;
}

abstract class ItemBase
{
    [JsonProperty(PropertyName = "name")]
    public string Name;

    [JsonProperty(PropertyName = "order")]
    public int Order;

    [JsonProperty(PropertyName = "description")]
    public string Description;

    public override string ToString() => Name;
}

sealed class Parameter : ItemBase
{
    [JsonProperty(PropertyName = "type")]
    public object Type;

    [JsonProperty(PropertyName = "optional")]
    public bool Optional;

    public override string ToString() => Name;
}

sealed class ReturnValue : ItemBase
{
    [JsonProperty(PropertyName = "type")]
    public object Type;

    [JsonProperty(PropertyName = "optional")]
    public bool Optional;
}

sealed class Member : ItemBase
{
    [JsonProperty(PropertyName = "type")]
    public object Type;

    [JsonProperty(PropertyName = "read")]
    public bool Read;

    [JsonProperty(PropertyName = "write")]
    public bool Write;

    [JsonProperty(PropertyName = "parameters")]
    public Parameter[] Parameters;

    [JsonProperty(PropertyName = "takes_table")]
    public bool TakesTable;

    [JsonProperty(PropertyName = "return_values")]
    public ReturnValue[] ReturnValues;
}

sealed class Field : ItemBase
{
    [JsonProperty(PropertyName = "type")]
    public object Type;

    [JsonProperty(PropertyName = "read")]
    public bool Read;

    [JsonProperty(PropertyName = "write")]
    public bool Write;
}

sealed class Method : ItemBase
{
    [JsonProperty(PropertyName = "parameters")]
    public Parameter[] Parameters;

    [JsonProperty(PropertyName = "takes_table")]
    public bool TakesTable;

    [JsonProperty(PropertyName = "return_values")]
    public ReturnValue[] ReturnValues;
}

sealed class Class : ItemBase
{
    [JsonProperty(PropertyName = "methods")]
    public Method[] Methods;

    [JsonProperty(PropertyName = "attributes")]
    public Field[] Attributes;

    [JsonProperty(PropertyName = "operators")]
    public Member[] Operators;
}