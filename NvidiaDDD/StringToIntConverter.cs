using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NvidiaDriverThing
{
    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : int.Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}