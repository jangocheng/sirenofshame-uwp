﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SirenOfShame.Device;
using SirenOfShame.Uwp.Background.Models;
using SirenOfShame.Uwp.Background.Services;

namespace SirenOfShame.Uwp.Background.Controllers
{
    internal class PlayLedPatternController : ControllerBase
    {
        public override string CommandName => "playLedPattern";
        public override async Task<SocketResult> Invoke(string frame)
        {
            var playLedRequest = JsonConvert.DeserializeObject<PlayLedRequest>(frame);

            if (SirenService.Instance.IsConnected)
            {
                var id = playLedRequest.Id;
                var durationStr = playLedRequest.Duration;
                var ledPattern = ToLedPattern(id);
                var duration = ToDuration(durationStr);
                await SirenService.Instance.PlayLightPattern(ledPattern, duration);
            }

            return new OkSocketResult();
        }

        private TimeSpan? ToDuration(int? duration)
        {
            if (duration == null) return null;
            return new TimeSpan(0, 0, duration.Value);
        }

        private LedPattern ToLedPattern(int? id)
        {
            if (id == null) return new LedPattern();
            return new LedPattern { Id = id.Value };
        }
    }

    internal class PlayLedRequest
    {
        public int? Id { get; set; }
        public int? Duration { get; set; }
    }
}
