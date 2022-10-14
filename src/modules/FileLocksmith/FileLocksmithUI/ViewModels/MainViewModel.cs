﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace PowerToys.FileLocksmithUI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using FileLocksmith.Interop;
    using global::FileLocksmithUI;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml.Controls;

#pragma warning disable CA1708 // Identifiers should differ by more than case
    public partial class MainViewModel : ObservableObject, IDisposable
#pragma warning restore CA1708 // Identifiers should differ by more than case
    {
        public IAsyncRelayCommand LoadProcessesCommand { get; }

        private bool _isLoading;
        private bool _isElevated;
        private string[] paths;
        private bool _disposed;

        public ObservableCollection<ProcessResult> Processes { get; } = new ();

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsElevated
        {
            get
            {
                return _isElevated;
            }

            set
            {
                _isElevated = value;
                OnPropertyChanged(nameof(IsElevated));
            }
        }

        public MainViewModel()
        {
            LoadProcessesCommand = new AsyncRelayCommand(LoadProcessesAsync);
        }

        private async Task LoadProcessesAsync()
        {
            IsLoading = true;
            Processes.Clear();

            // paths = NativeMethods.ReadPathsFromStdin();
            paths = new string[1] { "C:\\Program Files" };

            foreach (ProcessResult p in await FindProcesses(paths))
            {
                Processes.Add(p);
            }

            IsLoading = false;
        }

        private async Task<List<ProcessResult>> FindProcesses(string[] paths)
        {
            var results = new List<ProcessResult>();
            await Task.Run(() =>
            {
                results = NativeMethods.FindProcessesRecursive(paths).ToList();
            });
            return results;
        }

        [RelayCommand]
        public void EndTask(ProcessResult selectedProcess)
        {
            Processes.Remove(selectedProcess);

            // if (NativeMethods.KillProcess(process.pid))
            // {
            //   Processes.Remove(selectedProcess);
            // }
            // else
            // {
            //    // TODO show something on failure.
            // }
        }

        [RelayCommand]
        public void RestartElevated()
        {
            if (NativeMethods.StartAsElevated(paths))
            {
                // TODO gentler exit
                Environment.Exit(0);
            }
            else
            {
                // TODO report error?
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _disposed = true;
                }
            }
        }
    }
}