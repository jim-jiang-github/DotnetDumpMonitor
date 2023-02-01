using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetDumpMonitor.Models
{
    public class ProcessDumpInfo
    {
        public int ProcessID { get; }
        public string Name { get; }

        public ProcessDumpInfo(int processID, string name)
        {
            ProcessID = processID;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
