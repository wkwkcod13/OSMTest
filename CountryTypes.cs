using System.Text.Json.Serialization;

namespace OSMTest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CountryTypes
    {
        AU,
        NZ,
        TW,
        US
    }  
}
