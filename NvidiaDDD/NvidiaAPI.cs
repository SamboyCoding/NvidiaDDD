using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NvidiaDriverThing.Models;

namespace NvidiaDriverThing
{
    public static class NvidiaAPI
    {
        private static readonly HttpClient client = new()
        {
            BaseAddress = new Uri("https://gfwsl.geforce.com/")
        };

        internal static async Task<List<List<MenuItem>>> GetMenuItems(int productType, int productSeriesType)
        {
            Console.Write($"Fetching Menu Items for productType {productType}, productSeriesType {productSeriesType}...");
            var ret = await client.GetFromJsonAsync<List<List<MenuItem>>>($"nvidia_web_services/controller.php?com.nvidia.services.Drivers.getMenuArrays/{{\"pt\":{productType},\"pst\":{productSeriesType},\"driverType\":\"all\"}}");
            Console.WriteLine("Success");
            
            return ret!;
        }

        internal static async Task<List<DriverInfo>> GetDrivers(int familyId, int gpuId, int osId, int languageId)
        {
            var body = await client.GetFromJsonAsync<DriverListResponse>($"services_toolkit/services/com/nvidia/services/AjaxDriverService.php?func=DriverManualLookup&psid={familyId}&pfid={gpuId}&osID={osId}&languageCode={languageId}&beta=0&isWHQL=0&dltype=-1&dch=1&upCRD=0&qnf=0&sort1=0&numberOfResults=10");

            return body!.IDS.Select(d => d.downloadInfo).ToList();
        }
    }
}