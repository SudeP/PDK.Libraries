using System.Text.Json;

namespace PDK.Json
{
    public static class JsonExtension
    {
        public static string ToJson(this object obj, JsonSerializerOptions jsonSerializerOptions = null) => JsonSerializer.Serialize(obj, jsonSerializerOptions);
        public static T FromJson<T>(this T _, string json, JsonSerializerOptions jsonSerializerOptions = null) => JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        public static T FromJson<T>(this string json, JsonSerializerOptions jsonSerializerOptions = null) => JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }
}
