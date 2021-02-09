using System.Collections.Generic;

namespace NvidiaDriverThing.Models
{
    public class DriverListResponse
    {
        public string Success { get; set; }
        public List<DriverObject> IDS { get; set; }
    }
}