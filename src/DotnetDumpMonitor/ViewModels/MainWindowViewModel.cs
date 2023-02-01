using CliWrap;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotnetDumpMonitor.Commons;
using DotnetDumpMonitor.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DotnetDumpMonitor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private IEnumerable<ObjectDumpInfo>? _lastObjectDumpInfos = null;
        private IEnumerable<ObjectDumpInfo>? _baseObjectDumpInfos = null;
        private string? _gcDumpVersionStr = null;

        [ObservableProperty]
        private bool _loaded = false;

        [ObservableProperty]
        private bool _processesLoaded = false;

        [ObservableProperty]
        private ObservableCollection<ProcessDumpInfo> _processes = new();

        [ObservableProperty]
        private ProcessDumpInfo? _selectProcess = null;

        [ObservableProperty]
        private ObservableCollection<ObjectDumpInfo> _diffObjectDumpInfos = new();

        [ObservableProperty]
        private string _title;
        public Version? GithubLastReleaseVersion { get; set; }

        public MainWindowViewModel()
        {
            RefreshTitle();
            BindingOperations.EnableCollectionSynchronization(Processes, this);
            DispatcherTimer dispatcherTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            dispatcherTimer.Tick += async (s, e) =>
            {
                if (SelectProcess == null)
                {
                    return;
                }
                await RefreshObjectDumpInfos(SelectProcess);
            };
            dispatcherTimer.Start();
        }

        public void RefreshTitle()
        {
            var gcDumpVersionStr = _gcDumpVersionStr == null ? "version loading..." : $"v{_gcDumpVersionStr}";
            var version = (GithubLastReleaseVersion == null || GithubUpgradeHelper.CurrentVersion == GithubLastReleaseVersion) 
                ? $"v{GithubUpgradeHelper.CurrentVersion}" : $"v{GithubUpgradeHelper.CurrentVersion}(lastest v{GithubLastReleaseVersion})";
            Title = $"Dump monitor {version} / Dotnet gc dump {gcDumpVersionStr}";
        }
        [RelayCommand]
        private async Task InitWindow()
        {
            var gcDumpVersionStr = await DotnetGcDumpHelper.GetGcDumpVersionStr();
            if (gcDumpVersionStr == null)
            {
                if (MessageBox.Show("You haven't installed 'dotnet gc dump' yet, do you want to install it now?", "Warning!!!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (!await DotnetGcDumpHelper.InstallDotnetGcDump())
                    {
                        MessageBox.Show("Installation error");
                        App.Current.Shutdown();
                        return;
                    }
                }
                else
                {
                    App.Current.Shutdown();
                    return;
                }
                gcDumpVersionStr = await DotnetGcDumpHelper.GetGcDumpVersionStr();
            }
            _gcDumpVersionStr = gcDumpVersionStr;
            RefreshTitle();
            Loaded = true;
            await RefreshProcesses();
        }
        [RelayCommand]
        private async Task RefreshProcesses()
        {
            Clean();
            SelectProcess = null;
            Processes.Clear();
            var cmd = await Cli.Wrap("dotnet-gcdump")
                .WithArguments(args => args
                .Add("ps")
                )
                .WithStandardOutputPipe(PipeTarget.ToDelegate((msg) =>
                {
                    var match = Regex.Match(msg, " (\\d.*?)  (.*?) (.*)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int id) && match.Groups[2].Value is string name)
                    {
                        ProcessDumpInfo processDumpInfo = new(id, name);
                        Processes.Add(processDumpInfo);
                    }
                }))
                .ExecuteAsync();
        }
        [RelayCommand]
        private async Task SetCurrentAsBase()
        {
            _baseObjectDumpInfos = _lastObjectDumpInfos;
            if (SelectProcess == null)
            {
                return;
            }
            await RefreshObjectDumpInfos(SelectProcess);
            Log.Logger.Information("SetCurrentAsBase");
        }

        [RelayCommand]
        private async Task ResetBase()
        {
            _baseObjectDumpInfos = null;
            if (SelectProcess == null)
            {
                return;
            }
            await RefreshObjectDumpInfos(SelectProcess);
            Log.Logger.Information("SetCurrentAsBase");
        }

        public async Task RefreshObjectDumpInfos(ProcessDumpInfo processDumpInfo)
        {
            List<string> lines = new();
            var cmd = await Cli.Wrap("dotnet-gcdump")
                 .WithArguments(args => args
                 .Add("report")
                 .Add("-p")
                 .Add(processDumpInfo.ProcessID.ToString())
                 )
                 .WithStandardOutputPipe(PipeTarget.ToDelegate((msg) =>
                 {
                     lines.Add(msg);
                 }))
                 .WithValidation(CommandResultValidation.None)
                 .ExecuteAsync();
            var objectDumpInfos = lines
                //"      3,882,796  GC Heap bytes"
                //"         48,015  GC Heap objects"
                //"        101,692  Total references"
                //""
                //"   Object Bytes     Count  Type"
                .Skip(5)
                .Select(line =>
                {
                    var groups = Regex.Split(line, @"\s\s+");
                    if (groups.Length == 5
                        && long.TryParse(groups[1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out long size)
                        && int.TryParse(groups[2], out int count)
                        && groups[3] is string name
                        && groups[4] is string lib)
                    {
                        ObjectDumpInfo objectDumpInfo = new(name, count, size, lib);
                        return objectDumpInfo;
                    }
                    return null;
                })
                .Where(x => x != null)
                .Cast<ObjectDumpInfo>();
            //.Where(a => a.Lib.Contains("RoomsHost"));


            _lastObjectDumpInfos = objectDumpInfos;

            var diffObjectDumpInfos = await Task.Run(() =>
            {
                List<ObjectDumpInfo> insideDiffObjectDumpInfos = new();
                foreach (var objectDumpInfo in objectDumpInfos)
                {
                    var lastObjectDumpInfo = _baseObjectDumpInfos?.FirstOrDefault(x => x.Lib == objectDumpInfo.Lib && x.Name == objectDumpInfo.Name);
                    if (lastObjectDumpInfo == null || lastObjectDumpInfo.Count < objectDumpInfo.Count)
                    {
                        insideDiffObjectDumpInfos.Add(objectDumpInfo);
                    }
                }
                return insideDiffObjectDumpInfos;
            });
            DiffObjectDumpInfos.Clear();
            if (!ProcessesLoaded || processDumpInfo != SelectProcess)
            {
                return;
            }
            foreach (var objectDumpInfo in diffObjectDumpInfos)
            {
                DiffObjectDumpInfos.Add(objectDumpInfo);
            }
        }

        async partial void OnSelectProcessChanged(ProcessDumpInfo? value)
        {
            ProcessesLoaded = value != null;
            if (value == null)
            {
                return;
            }
            Clean();
            await RefreshObjectDumpInfos(value);
        }

        private void Clean()
        {
            _lastObjectDumpInfos = null;
            _baseObjectDumpInfos = null;
            DiffObjectDumpInfos.Clear();
        }
    }
}
