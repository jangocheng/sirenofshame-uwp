﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SirenOfShame.Uwp.Server.Models;
using SirenOfShame.Uwp.Watcher.Settings;

namespace SirenOfShame.Uwp.Server.Commands
{
    internal class GetCiEntryPointSettingsCommand : CommandBase
    {
        public override string CommandName => "getCiEntryPointSettings";
        public override async Task<SocketResult> Invoke(string frame)
        {
            IEnumerable<CiEntryPointSetting> ciEntryPoints = new []
            {
                new CiEntryPointSetting { Name = "Jenkins", Url = "http://win7ci3:8081"},
            };
            await Task.Yield();
            return new GetCiEntryPointSettingsResult(ciEntryPoints);
        }
    }

    internal class GetCiEntryPointSettingsResult : SocketResult
    {
        public GetCiEntryPointSettingsResult(IEnumerable<CiEntryPointSetting> projects)
        {
            Type = "getProjectsResult";
            ResponseCode = 200;
            Result = projects;
        }
    }
}