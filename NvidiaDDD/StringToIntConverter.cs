using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NvidiaDriverThing
{
    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.String ? int.Parse(reader.GetString()!) : reader.GetInt32(); //Suppress null check because we've just verified there's a string here

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            //No-op.
        }
    }
}