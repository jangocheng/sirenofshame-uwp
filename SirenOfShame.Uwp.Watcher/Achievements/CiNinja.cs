﻿using SirenOfShame.Uwp.Core.Models;
using SirenOfShame.Uwp.Watcher.Settings;

namespace SirenOfShame.Uwp.Watcher.Achievements
{
    public class CiNinja : AchievementBase
    {
        private readonly int _howManyTimesHasFixedSomeoneElsesBuild;

        public CiNinja(PersonSetting personSetting)
            : base(personSetting)
        {
            _howManyTimesHasFixedSomeoneElsesBuild = personSetting.NumberOfTimesFixedSomeoneElsesBuild;
        }

        public override AchievementEnum AchievementEnum
        {
            get { return AchievementEnum.CiNinja; }
        }

        protected override bool MeetsAchievementCriteria()
        {
            return _howManyTimesHasFixedSomeoneElsesBuild >= 1;
        }
    }
}
