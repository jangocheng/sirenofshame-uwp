﻿using System;
using SirenOfShame.Uwp.Watcher.Device;

namespace SirenOfShame.Uwp.Watcher.Watcher {
    public class SetLightsEventArgs {
        public int? Duration { get; set; }
        public LedPattern LedPattern { get; set; }
        
        public TimeSpan? TimeSpan
        {
            get { return Duration == null ? (TimeSpan?)null : new TimeSpan(0, 0, 0, Duration.Value); }
        }
    }
}