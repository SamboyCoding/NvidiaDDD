using System.Text.Json.Serialization;

namespace NvidiaDriverThing.Models
{
    public class MenuItem
    {
        [JsonConverter(typeof(StringToIntConverter))]
        public int id { get; set; }
        public string menutext { get; set; }
    }
}