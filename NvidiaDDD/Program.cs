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
        internal static void Main(string[] args) => MainAsync().Wait();

        private static async Task MainAsync()
        {
            try
            {
                var gpuName = GpuUtilities.GetGpuModel(); //e.g. GeForce RTX 2080
                
                if(gpuName == null)
                    return;
                
                var split = gpuName.Split(" ");
                var productType = split[0]; //e.g. GeForce
                var model = string.Join(" ", split.Skip(1)); //e.g. RTX 2080
                var familyName = GpuUtilities.GetGpuFamilyFromModel(model); //e.g. GeForce RTX 20 Series

                var drivers = await GpuUtilities.GetMostRecentDrivers(productType, familyName, gpuName, model);
                
                if(drivers == null)
                    return;

                //Put most recent last
                drivers.Reverse();
                
                //Print out
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