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

                if (drivers == null || drivers.Count == 0)
                    goto end;
                
                var (currentDriverVersion, driverFileDate) = GpuUtilities.GetCurrentGpuDriverVersion();

                //Put most recent last
                drivers.Reverse();
                
                //Print out
                foreach (var driver in drivers)
                {
                    Console.WriteLine($"Driver {driver.ID}\n\tType: {HttpUtility.UrlDecode(driver.Name)}\n\tVersion {driver.Version}\n\tReleased {driver.ReleaseDateTime}\n\tSize: {driver.DownloadURLFileSize}\n\tDownload Link: {driver.DownloadURL}\n");
                }
                
                Console.WriteLine("Most recent driver is at the bottom for your convenience. Enjoy!");

                if (currentDriverVersion == null) 
                    return;

                var installedDriverInfo = drivers.FirstOrDefault(d => d.Version == currentDriverVersion);
                var publishedDriverDate = installedDriverInfo?.ReleaseDateTime;  
                Console.WriteLine($"Latest driver is {drivers.Last().Version}, released on {drivers.Last().ReleaseDateTime}, you are on {currentDriverVersion}, released {(publishedDriverDate != null ? "on " + publishedDriverDate : "a long time ago (self-reports date " + driverFileDate + ", but this may be wrong)")}.");

                if (currentDriverVersion == drivers.Last().Version)
                {
                    Console.WriteLine("You are up to date.");
                }
                else if (installedDriverInfo != null)
                {
                    drivers.Reverse();
                    var versionsBehind = drivers.IndexOf(installedDriverInfo);
                    Console.WriteLine($"You are {versionsBehind} version{(versionsBehind == 1 ? "" : "s")} out of date.");
                }
                else
                {
                    Console.WriteLine("You are more than 10 versions out of date.");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            end:
            Console.WriteLine("\nPress enter to exit...");
            Console.ReadLine();
        }
    }
}