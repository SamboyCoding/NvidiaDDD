using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NvidiaDriverThing.Models
{
    //Disable nullability checks, this class is reflection-instantiated
#pragma warning disable 8618

    //Initialized using reflection, so these are actually used.
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class DriverListResponse
    {
        public string Success { get; set; }
        public List<DriverObject> IDS { get; set; }
    }
}