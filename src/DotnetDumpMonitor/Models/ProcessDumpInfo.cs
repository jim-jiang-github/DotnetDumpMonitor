using DotnetDumpMonitor.Commons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotnetDumpMonitor.Models
{
    public class ProcessDumpInfo
    {

        public int ProcessID { get; }
        public string Name { get; }
        public IntPtr Handle { get; }

        public ProcessDumpInfo(int processID, string name)
        {
            ProcessID = processID;
            Handle = WindowsHelper.GetRootWindowOfProcess(processID);
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
