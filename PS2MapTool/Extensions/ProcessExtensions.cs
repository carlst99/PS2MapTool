using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;

namespace System.Diagnostics
{
    public static class ProcessExtensions
    {
        [SupportedOSPlatform("Windows")]
        public static List<Process> GetChildren(this Process process)
        {
            List<Process> children = new();
            ManagementObjectSearcher mos = new(string.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

            foreach (ManagementObject mo in mos.Get())
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));

            return children;
        }
    }
}
