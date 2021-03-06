﻿using System.Collections.Generic;
using System.Linq;
using SirenOfShame.Uwp.Watcher.Settings;
using SirenOfShame.Uwp.Watcher.Watcher;

namespace SirenOfShame.Uwp.Watcher.StatCalculators
{
    public class SuccessInARow : StatCalculatorBase
    {
        public override void SetStats(PersonSetting personSetting, List<BuildStatus> allActiveBuildDefinitionsOrderedChronoligically)
        {
            personSetting.CurrentSuccessInARow = CalculateSuccessInARow(personSetting, allActiveBuildDefinitionsOrderedChronoligically);
        }

        public static int CalculateSuccessInARow(PersonSetting personSetting, IEnumerable<BuildStatus> allActiveBuildDefinitionsOrderedChronoligically)
        {
            return allActiveBuildDefinitionsOrderedChronoligically
                .Reverse()
                .Where(i => i.RequestedBy == personSetting.RawName)
                .TakeWhile(i => i.BuildStatusEnum != BuildStatusEnum.Broken)
                .Count();
        }
    }
}
