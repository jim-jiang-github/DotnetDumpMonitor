using CliWrap;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetDumpMonitor.Commons
{
    internal class DotnetGcDumpHelper
    {
        public static async Task<string?> GetGcDumpVersionStr()
        {
            var version = "unknow";
            try
            {
                var cmd = await Cli.Wrap("dotnet-gcdump")
                    .WithArguments(args => args
                    .Add("--version")
                    )
                    .WithStandardOutputPipe(PipeTarget.ToDelegate((msg) =>
                    {
                        version = msg;
                    }))
                    .ExecuteAsync();
                return version;
            }
            catch (Exception ex) 
            {
                Log.Logger.Error(ex.ToString());
                return null;
            }
        }


        public static async Task<bool> InstallDotnetGcDump()
        {
            var outputMsg = string.Empty;
            try
            {
                var cmd = await Cli.Wrap("dotnet")
                    .WithArguments(args => args
                    .Add("tool")
                    .Add("install")
                    .Add("--global")
                    .Add("dotnet-gcdump")
                    )
                    .WithStandardOutputPipe(PipeTarget.ToDelegate((msg) =>
                    {
                        outputMsg += msg;
                    }))
                    .WithStandardErrorPipe(PipeTarget.ToDelegate((msg) =>
                    {
                        outputMsg += msg;
                    }))
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync();
                return outputMsg.Contains("successfully installed");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
                return false;
            }
        }
    }
}
