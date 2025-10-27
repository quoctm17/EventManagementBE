using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventManagement.Application.Helpers
{
    // A System.Text.Json converter that accepts either a string token or a numeric token and returns string
    public class StringOrNumberJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Unsupported JSON token for string field: {reader.TokenType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
