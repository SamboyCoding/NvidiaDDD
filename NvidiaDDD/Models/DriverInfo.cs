using System.Diagnostics.CodeAnalysis;

namespace NvidiaDriverThing.Models
{
    //Disable nullability checks, this class is reflection-instantiated
#pragma warning disable 8618

    //Initialized using reflection, so these are actually used.
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class DriverInfo
    {
        public string ID { get; set; }
        public string Name { get; set; } //Url-Encoded
        public string ShortDescription { get; set; } //Url-Encoded
        public string Version { get; set; }
        public string ReleaseDateTime { get; set; }
        public string DownloadURL { get; set; }
        public string DownloadURLFileSize { get; set; }
    }
}