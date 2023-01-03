using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetDumpMonitor.Models
{
    public class ObjectDumpInfo
    {
        public string Name { get; }
        public int Count { get; }
        public long Size { get; }
        public string Lib { get; }

        public ObjectDumpInfo(string name, int count, long size, string lib)
        {
            Name = name;
            Count = count;
            Size = size;
            Lib = lib;
        }

        public override string ToString()
        {
            return $"{Lib}-{Name}";
        }
    }
}
