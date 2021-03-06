﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SirenOfShame.Uwp.Core.Interfaces;
using SirenOfShame.Uwp.Core.Services;
using SirenOfShame.Uwp.Watcher.Exceptions;
using SirenOfShame.Uwp.Watcher.Settings;

namespace SirenOfShame.Uwp.Watcher.Watcher
{
    public abstract class WatcherBase : IDisposable
    {
        private static readonly ILog _log = MyLogManager.GetLog(typeof(WatcherBase));

        protected WatcherBase(SirenOfShameSettings settings)
        {
            Settings = settings;
        }

        protected void InvokeServerUnavailable(ServerUnavailableException args)
        {
            var e = ServerUnavailable;
            if (e != null) e(this, new ServerUnavailableEventArgs(args));
        }

        protected void InvokeBuildDefinitionNotFound(BuildDefinitionSetting buildDefinitionSetting)
        {
            var e = BuildDefinitionNotFound;
            if (e != null) e(this, new BuildDefinitionNotFoundArgs(buildDefinitionSetting));
        }

        protected void InvokeStatusChecked(IList<BuildStatus> args)
        {
            StatusChecked?.Invoke(this, new StatusCheckedEventArgsArgs
            {
                BuildStatuses = args
            });
        }

        private void GetBuildStatusAndFireEvents()
        {
            try
            {
                var newBuildStatus = GetBuildStatus();
                if (newBuildStatus.Count != 0)
                    InvokeStatusChecked(newBuildStatus);
            }
            catch (ServerUnavailableException ex)
            {
                InvokeServerUnavailable(ex);
            }
            catch (BuildDefinitionNotFoundException ex)
            {
                InvokeBuildDefinitionNotFound(ex.BuildDefinitionSetting);
            }
        }

        protected abstract IList<BuildStatus> GetBuildStatus();

        public virtual async Task StartWatching(CancellationToken token)
        {
            try
            {
                await _log.Debug(string.Format("Started watching build status, poling interval: {0} seconds",
                    Settings.PollInterval));
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    GetBuildStatusAndFireEvents();
                    if (token.IsCancellationRequested) break;
                    await Task.Delay(Settings.PollInterval * 1000, token);
                }
                await _log.Debug("Stopped watching build status");
                OnStoppedWatching();
            }
            catch (TaskCanceledException)
            {
                await _log.Debug("Cancelled watching");
                OnStoppedWatching();
            }
            catch (Exception ex)
            {
                await _log.Error("uncaught exception in watcher", ex);
                ExceptionMessageBox.Show(null, "Drat", "Error connecting to server", ex);
            }
            finally
            {
                StopWatching();
            }
        }

        public event StatusCheckedEvent StatusChecked;
        public event ServerUnavailableEvent ServerUnavailable;
        public event BuildDefinitionNotFoundEvent BuildDefinitionNotFound;
        public event StoppedWatchingEvent StoppedWatching;
        public SirenOfShameSettings Settings { private get; set; }
        public CiEntryPointSetting CiEntryPointSetting { protected get; set; }

        public abstract void StopWatching();

        public abstract void Dispose();

        protected void OnStoppedWatching()
        {
            StoppedWatching?.Invoke(this, new StoppedWatchingEventArgs());
        }
    }

    public delegate void StoppedWatchingEvent(object sender, StoppedWatchingEventArgs args);

    public class StoppedWatchingEventArgs
    {
    }

    public delegate void BuildDefinitionNotFoundEvent(object sender, BuildDefinitionNotFoundArgs args);

    public class BuildDefinitionNotFoundArgs : EventArgs
    {
        public BuildDefinitionSetting BuildDefinitionSetting { get; set; }

        public BuildDefinitionNotFoundArgs(BuildDefinitionSetting buildDefinitionSetting)
        {
            BuildDefinitionSetting = buildDefinitionSetting;
        }
    }

    public delegate void ServerUnavailableEvent(object sender, ServerUnavailableEventArgs args);

    public class ServerUnavailableEventArgs
    {
        public ServerUnavailableEventArgs(ServerUnavailableException ex)
        {
            Exception = ex;
        }
        public ServerUnavailableException Exception { get; set; }
    }
}
