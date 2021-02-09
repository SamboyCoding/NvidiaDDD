namespace NvidiaDriverThing.Models
{
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