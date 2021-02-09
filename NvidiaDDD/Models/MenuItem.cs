using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NvidiaDriverThing.Models
{
    //Disable nullability checks, this class is reflection-instantiated
#pragma warning disable 8618

    //Initialized using reflection, so these are actually used.
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class MenuItem
    {
        [JsonConverter(typeof(StringToIntConverter))]
        public int id { get; set; }
        public string menutext { get; set; }
    }
}