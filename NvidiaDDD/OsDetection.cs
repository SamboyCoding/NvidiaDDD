using System;
using System.Runtime.InteropServices;

namespace NvidiaDriverThing
{
    internal static class OsDetection
    {
        internal static string GetOsName()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            {
                return "Windows 11";
            }
            
            //This is NOT an exhaustive list, this is just for the versions nvidia supports
            if(OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                //Windows 10
                return RuntimeInformation.OSArchitecture == Architecture.X64 ? "Windows 10 64-bit" : "Windows 10 32-bit";
            }

            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                //Windows 7
                return RuntimeInformation.OSArchitecture == Architecture.X64 ? "Windows 7 64-bit" : "Windows 7 32-bit"; 
            }

            if (OperatingSystem.IsLinux())
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.Arm64:
                        return "Linux aarch64";
                    case Architecture.X64:
                        return "Linux 64-bit";
                    case Architecture.X86:
                        return "Linux 32-bit";
                }
            }

            if (OperatingSystem.IsFreeBSD() && RuntimeInformation.OSArchitecture == Architecture.X64)
                return "FreeBSD x64";
            
            //Solaris is also supported but .net doesn't support it.

            return "unsupported";
        }
    }
}