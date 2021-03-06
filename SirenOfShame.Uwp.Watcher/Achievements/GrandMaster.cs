﻿using SirenOfShame.Uwp.Core.Models;
using SirenOfShame.Uwp.Watcher.Settings;

namespace SirenOfShame.Uwp.Watcher.Achievements
{
    public class GrandMaster : AchievementBase
    {
        private readonly int _reputation;

        public GrandMaster(PersonSetting personSetting, int reputation)
            : base(personSetting)
        {
            _reputation = reputation;
        }

        public override AchievementEnum AchievementEnum
        {
            get { return AchievementEnum.GrandMaster; }
        }

        protected override bool MeetsAchievementCriteria()
        {
            return _reputation >= 500;
        }
    }
}
