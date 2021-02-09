using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NvidiaDriverThing.Models;

namespace NvidiaDriverThing
{
    internal static class GpuUtilities
    {
        [SupportedOSPlatform("windows")]
        private static ManagementObject? GetNvidiaGPUUsingWindowsManagement()
        {
            using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");

            var gpus = searcher.Get().Cast<ManagementObject>().ToList();

            var nvidiaGpu = gpus.FirstOrDefault(gpu => (string) gpu["AdapterCompatibility"] == "NVIDIA");

            if (nvidiaGpu == null)
            {
                Console.WriteLine("[Error] Could not detect an Nvidia GPU.");
                return null;
            }

            return nvidiaGpu;
        }
        
        [SupportedOSPlatform("windows")]
        internal static string? GetGpuModel()
        {
            //This function is currently windows-specific but it shouldn't be hard to make a linux version.
            Console.WriteLine("Detecting GPU using Win32Management...");

            var nvidiaGpu = GetNvidiaGPUUsingWindowsManagement();

            if (nvidiaGpu == null)
                return null;

            var rawName = (string) nvidiaGpu["VideoProcessor"];

            if (rawName.EndsWith("GB"))
                //Fix for 1060 3GB / 1060 6GB
                rawName = string.Join(" ", rawName.Split(" ").SkipLast(1));

            return rawName;
        }

        [SupportedOSPlatform("windows")]
        internal static (string? driverVersion, string? driverDate) GetCurrentGpuDriverVersion()
        {
            Console.WriteLine("Getting installed driver version using Win32Management...");

            var nvidiaGpu = GetNvidiaGPUUsingWindowsManagement();

            if (nvidiaGpu == null)
                return (null, null);

            //Of the form 27.21.14.6079 where the 4.6079 is the interesting bit (driver v460.79)
            //Also checked against 461.40 which is 27.21.14.6140
            var rawVersion = (string) nvidiaGpu["DriverVersion"];
            Console.WriteLine($"Currently installed driver reports itself as version {rawVersion}");
            
            //Discard the first two dotted sections
            var split = rawVersion.Split('.');
            if (split.Length != 4)
            {
                Console.WriteLine("Unknown driver version format.");
                return (null, null);
            }

            //E.g. 46079
            var driverVersionNoDot = split[2].Substring(1) + split[3];
            var driverVersion = driverVersionNoDot.Substring(0, 3) + "." + driverVersionNoDot.Substring(3);
            
            Console.WriteLine($"Which is the version that NVIDIA call {driverVersion}");
            
            var rawDate = (string) nvidiaGpu["DriverDate"]; //e.g 20201203000000.000000-000, which appears to be of format YYYYMMDDHHmmSS.Ticks-offset

            var formattedDate = DateTime.ParseExact(rawDate.Substring(0, 8), "yyyyMMdd", null).ToString("yyyy MMMM dd");

            return (driverVersion, driverDate: formattedDate);
        }

        internal static string GetGpuFamilyFromModel(string model)
        {
            if (!model.Contains("GTX") && !model.Contains("RTX"))
                throw new NotImplementedException("Family detection for non-GTX, non-RTX cards NYI");
            
            //Take specifically the second word, because there could be a 'Ti' on the end.
            var modelNumber = model.Split(" ")[1];
            if (model.Contains("RTX"))
            {
                //All RTX cards are currently 4-digit model numbers, and the first two digits are the series number (20 or 30).
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
            
            //Older, three-digit model. Series name is hundreds digit + 00
            var threeDigitSeriesNumber = modelNumber[0] + "00";
            if (isMobile)
                threeDigitSeriesNumber += "M"; //Mobile (Laptop) GPU line.

            return $"GeForce {threeDigitSeriesNumber} Series{(isMobile ? " (Notebooks)" : "")}";
        }

        internal static async Task<List<DriverInfo>?> GetMostRecentDrivers(string productType, string familyName, string gpuName, string model)
        {
            //Fetch options so we can populate productId
            var menuItems = await NvidiaAPI.GetMenuItems(1, 0);

            var productId = menuItems[0].GetOptionId(productType);

            if (productId != 1)
            {
                //Fetch again if needed so we can populate familyId
                menuItems = await NvidiaAPI.GetMenuItems(productId, 0);
            }

            var familyId = menuItems[1].GetOptionId(familyName);

            //Fetch with productId and familyId so we can populate the rest of the options
            menuItems = await NvidiaAPI.GetMenuItems(productId, familyId);

            //Find GPU Id
            Console.WriteLine($"Looking for GPU {gpuName} in list of gpus...");
            var gpuId = menuItems[2].GetOptionId(gpuName);

            //Find OS Id
            var osName = OsDetection.GetOsName();

            if (osName == "unsupported")
            {
                Console.WriteLine("Could not detect operating system.");
                return null;
            }

            int osId;
            try
            {
                osId = menuItems[4].GetOptionId(osName);
            }
            catch (Exception)
            {
                Console.WriteLine("NVIDIA does not provide drivers for this GPU and OS version.");
                return null;
            }

            //Find language ID.
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

            //Get drivers with the four IDs we have.
            return await NvidiaAPI.GetDrivers(familyId, gpuId, osId, langId);
        }
    }
}