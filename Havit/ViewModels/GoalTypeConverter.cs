using System.Text.Json;
using System.Text.Json.Serialization;

public class GoalTypeConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle different JSON token types
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? "none",
            JsonTokenType.Number => reader.GetDouble().ToString(), 
            _ => "none"
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}