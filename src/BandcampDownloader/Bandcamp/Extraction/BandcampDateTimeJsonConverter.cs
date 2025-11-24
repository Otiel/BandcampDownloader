using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BandcampDownloader.Bandcamp.Extraction;

/// <summary>
/// Used to parse dates in Bandcamp data formatted as "01 Jan 2005 00:00:00 GMT"
/// </summary>
public class BandcampDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
