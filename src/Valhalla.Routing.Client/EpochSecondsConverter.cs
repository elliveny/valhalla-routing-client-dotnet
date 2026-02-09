using System.Text.Json;
using System.Text.Json.Serialization;

namespace Valhalla.Routing;

/// <summary>
/// JSON converter for DateTimeOffset that serializes to/from Unix epoch seconds.
/// Used for trace point timestamps in map matching requests.
/// </summary>
public class EpochSecondsConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// Reads and converts JSON to DateTimeOffset from Unix epoch seconds.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>The DateTimeOffset value.</returns>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var seconds = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        throw new JsonException($"Expected number token for epoch seconds, got {reader.TokenType}");
    }

    /// <summary>
    /// Writes DateTimeOffset as Unix epoch seconds to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The DateTimeOffset value to write.</param>
    /// <param name="options">Serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
