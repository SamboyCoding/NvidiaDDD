using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using NvidiaDriverThing.Models;

namespace NvidiaDriverThing
{
    internal static class Program
    {
        private static readonly HttpClient client = new()
        {
            BaseAddress = new Uri("https://gfwsl.geforce.com/")
        };
        
        internal static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task<List<List<MenuItem>>> GetMenuItems(int productType, int productSeriesType)
        {
            return await client.GetFromJsonAsync<List<List<MenuItem>>>($"nvidia_web_services/controller.php?com.nvidia.services.Drivers.getMenuArrays/{{\"pt\":{productType},\"pst\":{productSeriesType},\"driverType\":\"all\"}}");
        }

        private static async Task<List<DriverInfo>> GetDrivers(int familyId, int gpuId, int osId, int languageId)
        {
            var body = await client.GetFromJsonAsync<DriverListResponse>($"services_toolkit/services/com/nvidia/services/AjaxDriverService.php?func=DriverManualLookup&psid={familyId}&pfid={gpuId}&osID={osId}&languageCode={languageId}&beta=0&isWHQL=0&dltype=-1&dch=1&upCRD=0&qnf=0&sort1=0&numberOfResults=10");

            return body.IDS.Select(d => d.downloadInfo).ToList();
        }

        private static string GetGpuModel()
        {
            //This function is currently windows-specific but it shouldn't be hard to make a linux version.
            Console.WriteLine("Detecting GPU using Win32Management...");
            using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");

            var gpus = searcher.Get().Cast<ManagementObject>().ToList();

            var nvidiaGpu = gpus.FirstOrDefault(gpu => (string) gpu["AdapterCompatibility"] == "NVIDIA");

            if (nvidiaGpu == null)
            {
                Console.WriteLine("[Error] Could not detect an Nvidia GPU.");
                return null;
            }

            var props = nvidiaGpu.Properties.Cast<PropertyData>().ToDictionary(p => p.Name, p => p.Value);

            var rawName = (string) props["VideoProcessor"];

            if (rawName.EndsWith("GB"))
                //Fix for 1060 3GB / 1060 6GB
                rawName = string.Join(" ", rawName.Split(" ").SkipLast(1));

            return rawName;
        }

        private static string GetGpuFamilyFromModel(string model)
        {
            if (!model.Contains("GTX") && !model.Contains("RTX"))
                throw new NotImplementedException("Family detection for non-GTX, non-RTX cards NYI");
            
            var modelNumber = model.Split(" ")[1];
            if (model.Contains("RTX"))
            {
                var seriesNumber = modelNumber.Substring(0, 2);
                return $"GeForce RTX {seriesNumber} Series";
            }

            var isMobile = modelNumber.ToLowerInvariant().EndsWith("m");
            if (modelNumber.Length == 4 && !isMobile)
            {
                //Newer model (16 or 10 series)
                if (modelNumber[1] == '6')
                    return "GeForce 16 Series";

                return "GeForce 10 Series";
            }
            
            //Older, three-digit model
            var threeDigitSeriesNumber = modelNumber[0] + "00";
            if (isMobile)
                threeDigitSeriesNumber += "M";

            return $"GeForce {threeDigitSeriesNumber} Series{(isMobile ? " (Notebooks)" : "")}";
        }

        private static async Task MainAsync()
        {
            try
            {
                var gpuName = GetGpuModel();
                
                if(gpuName == null)
                    return;
                
                var split = gpuName.Split(" ");
                var productType = split[0];
                var model = string.Join(" ", split.Skip(1));
                var familyName = GetGpuFamilyFromModel(model);

                Console.Write("\nFetching menu options...");
                var menuItems = await GetMenuItems(1, 0);
                Console.WriteLine($"Got {menuItems[0].Count} productTypes");

                var productId = menuItems[0].GetOptionId(productType);

                if (productId != 1)
                {
                    Console.Write($"Re-fetching menu options for ProductId {productId}...");
                    menuItems = await GetMenuItems(productId, 0);
                    Console.WriteLine("Success");
                }

                var familyId = menuItems[1].GetOptionId(familyName);

                Console.Write($"Re-fetching menu options for family {familyId}...");
                menuItems = await GetMenuItems(productId, familyId);
                Console.WriteLine("Success");

                Console.WriteLine($"Looking for GPU {gpuName} in list of gpus...");
                var gpuId = menuItems[2].GetOptionId(gpuName);

                var osName = OsDetection.GetOsName();

                if (osName == "unsupported")
                {
                    Console.WriteLine("Could not detect operating system.");
                    return;
                }

                int osId;
                try
                {
                    osId = menuItems[4].GetOptionId(osName);
                }
                catch (Exception)
                {
                    Console.WriteLine("NVIDIA does not provide drivers for this GPU and OS version.");
                    return;
                }

                var langName = CultureInfo.InstalledUICulture.EnglishName
                    .Replace("United Kingdom", "UK")
                    .Replace("United States", "US");

                int langId;
                try
                {
                    langId = menuItems[5].GetOptionId(langName);
                }
                catch (Exception)
                {
                    Console.WriteLine($"[Warning] Language is not supported with English Name, trying native name {CultureInfo.InstalledUICulture.NativeName}...");
                    try
                    {
                        langId = menuItems[5].GetOptionId(CultureInfo.InstalledUICulture.NativeName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("[Warning] Language is not supported with native name either, falling back to 'Other'...");
                        langId = menuItems[5].GetOptionId("Other");
                    }
                }
                
                Console.WriteLine("System Details:");
                Console.WriteLine($"\t-gpu: {gpuName}");
                Console.WriteLine($"\t-gpuModel: {model}");
                Console.WriteLine($"\t-gpuProduct: {productType}");
                Console.WriteLine($"\t-gpuFamily: {familyName}");
                Console.WriteLine($"\t-osName: {osName}");
                Console.WriteLine($"\t-osLang: {langName}");
                
                Console.WriteLine("\nQuery Details:");
                Console.WriteLine($"\t-gpuId: {gpuId}");
                Console.WriteLine($"\t-productId: {productId}");
                Console.WriteLine($"\t-familyId: {familyId}");
                Console.WriteLine($"\t-osId: {osId}");
                Console.WriteLine($"\t-languageId: {langId}");

                Console.Write("\nFetching up to 10 of the most recent drivers...");
                var drivers = await GetDrivers(familyId, gpuId, osId, langId);
                Console.WriteLine($"Got {drivers.Count}\n");

                drivers.Reverse();
                foreach (var driver in drivers)
                {
                    Console.WriteLine($"Driver {driver.ID}\n\tType: {HttpUtility.UrlDecode(driver.Name)}\n\tVersion {driver.Version}\n\tReleased {driver.ReleaseDateTime}\n\tSize: {driver.DownloadURLFileSize}\n\tDownload Link: {driver.DownloadURL}\n");
                }
                
                Console.WriteLine("Most recent driver is at the bottom for your convenience. Enjoy!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.WriteLine("\nPress enter to exit...");
            Console.ReadLine();
        }
    }
}