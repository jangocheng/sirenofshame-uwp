﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SirenOfShame.Lib.Watcher;
using SirenOfShame.Uwp.Core.Interfaces;
using SirenOfShame.Uwp.Core.Models;
using SirenOfShame.Uwp.Core.Services;
using SirenOfShame.Uwp.Watcher.Device;
using SirenOfShame.Uwp.Watcher.Dto;
using SirenOfShame.Uwp.Watcher.HudsonServices;
using SirenOfShame.Uwp.Watcher.Services;
using SirenOfShame.Uwp.Watcher.Watcher;
using SirenOfShame.Uwp.Watcher.Watchers.MockCiServerServices;

namespace SirenOfShame.Uwp.Watcher.Settings
{
    /// <summary>
    /// This is essentially the database of all settings.  It represents the set of information 
    /// that is required to run SoS including server url's and 
    /// credentials, rules, users, reputation and achievements, etc.
    /// </summary>
    public class SirenOfShameSettings
    {
        public const int AVATAR_COUNT = 21;

        // todo: figure out DI
        //[Import(typeof(ISirenOfShameDevice))]
        //private ISirenOfShameDevice SirenOfShameDevice { get; set; }
        private static readonly ILog _log = MyLogManager.GetLog(typeof(SirenOfShameSettings));
        private string _updateLocationOther;
        private bool? _anyDuplicateSettingsCached;
        //private static int? _currentVersion;
        public const float DEFAULT_FONT_SIZE = 8.25f;

        //private static readonly UpgradeBase[] _upgrades =
        //{
        //    new Upgrade0To1(),
        //    new Upgrade1To2(),
        //    new Upgrade2To3(),
        //    new Upgrade3To4(),
        //    new Upgrade4To5(),
        //    new Upgrade5To6(AVATAR_COUNT),
        //    new Upgrade6To7(),
        //    new Upgrade7To8()
        //};

        //public void ResetRules()
        //{
        //    Rules.Clear();
        //    Rules.AddRange(_defaultRules);
        //    Save();
        //}

        protected internal SirenOfShameSettings()
        {
            //if (useMef)
            //    IocContainer.Instance.Compose(this);
            Rules = new List<Rule>();
            CiEntryPointSettings = new List<CiEntryPointSetting>();
            //AudioPatterns = new List<AudioPatternSetting>();
            //LedPatterns = new List<LedPatternSetting>();
            People = new List<PersonSetting>();
            UserMappings = new List<UserMapping>();
            //Sounds = new List<Sound>();
            SosOnlineWhatToSync = WhatToSyncEnum.BuildStatuses;
            //FontSize = DEFAULT_FONT_SIZE;
        }

        public int? Version { get; set; }

        //[XmlIgnore]
        //public static int CurrentVersion
        //{
        //    get
        //    {
        //        if (!_currentVersion.HasValue)
        //        {
        //            _currentVersion = _upgrades.Max(i => i.ToVersion);
        //        }
        //        return _currentVersion.Value;
        //    }
        //}

        public DateTime? LastCheckedForAlert { get; set; }

        public List<Rule> Rules { get; set; }

        public int Pattern { get; set; }

        public List<CiEntryPointSetting> CiEntryPointSettings { get; set; }

        public List<PersonSetting> People { get; set; }

        public List<UserMapping> UserMappings { get; set; }

        //public List<Sound> Sounds { get; set; }

        public bool SirenEverConnected { get; set; }

        public bool NeverShowGettingStarted { get; set; }

        public UpdateLocation UpdateLocation { get; set; }

        public int? SortColumn { get; set; }

        public bool SortDescending { get; set; }

        public WhatToSyncEnum SosOnlineWhatToSync { get; set; }

        public string SosOnlineUsername { get; set; }

        public string SosOnlinePassword { get; set; }

        public string SosOnlineProxyUrl { get; set; }

        public string SosOnlineProxyUsername { get; set; }

        public string SosOnlineProxyPasswordEncrypted { get; set; }

        public long? SosOnlineHighWaterMark { get; set; }

        public bool SosOnlineAlwaysOffline { get; set; }

        public bool SosOnlineAlwaysSync { get; set; }

        [XmlIgnore]
        public bool AnyDuplicateBuildNames
        {
            get
            {
                if (!_anyDuplicateSettingsCached.HasValue)
                {
                    // caching is a pre-optimization, but this will get called a lot and change ~never
                    _anyDuplicateSettingsCached = CiEntryPointSettings
                        .SelectMany(i => i.BuildDefinitionSettings)
                        .GroupBy(i => i.Name)
                        .Any(i => i.Count() > 1);
                }
                return _anyDuplicateSettingsCached.Value;
            }
        }

        public void ClearDuplicateNameCache()
        {
            _anyDuplicateSettingsCached = null;
        }

        public string UpdateLocationOther
        {
            get { return _updateLocationOther; }
            set
            {
                _updateLocationOther = value;

                // file:///c|/temp/

                if (!string.IsNullOrWhiteSpace(_updateLocationOther))
                {
                    _updateLocationOther = _updateLocationOther.Trim();

                    if (_updateLocationOther.Length > 2)
                    {
                        if (char.IsLetter(_updateLocationOther[0]) && _updateLocationOther[1] == ':')
                        {
                            _updateLocationOther = _updateLocationOther.Replace('\\', '/');
                            _updateLocationOther = _updateLocationOther.Substring(0, 1).ToLowerInvariant() + "|" + _updateLocationOther.Substring(2);
                            _updateLocationOther = "file:///" + _updateLocationOther;
                        }
                    }

                    if (!_updateLocationOther.EndsWith("/") && !_updateLocationOther.EndsWith("\\"))
                    {
                        _updateLocationOther += "/";
                    }
                }
            }
        }

        /// <summary>
        /// In seconds
        /// </summary>
        public int PollInterval { get; set; }

        //public List<AudioPatternSetting> AudioPatterns { get; set; }

        //public List<LedPatternSetting> LedPatterns { get; set; }

        public int? SoftwareInstanceId { get; set; }

        public bool Mute { get; set; }

        private string _fileName;

        public DateTime? AlertClosed { get; set; }

        public string FileName { get { return _fileName; } }

        public string MyRawName { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool StartInFullScreen { get; set; }

        public float FontSize { get; set; }

        [XmlIgnore]
        public bool ShowUpgradeWindowAtNextOpportunity { get; set; }

        public virtual void Dirty()
        {
            IsDirty = true;
        }

        public bool IsDirty { get; private set; }

        //public AchievementAlertPreferenceEnum AchievementAlertPreference { get; set; }

        //public void TryUpgrade()
        //{
        //    var sortedUpgrades = _upgrades.OrderBy(i => i.ToVersion);

        //    foreach (var upgrade in sortedUpgrades)
        //    {
        //        if (Version == upgrade.FromVersion)
        //        {
        //            upgrade.Upgrade(this);
        //            Version = upgrade.ToVersion;
        //            Save();
        //        }
        //    }
        //}

        //private void ErrorIfAnythingLooksBad()
        //{
        //    if (CiEntryPointSettings.SelectMany(i => i.BuildDefinitionSettings).Any(bds => string.IsNullOrEmpty(bds.BuildServer)))
        //        throw new Exception("A BuildDefinitionSetting.BuildServer was null or empty");
        //}

        //public Rule FindRule(TriggerType triggerType, string id, string triggerPerson)
        //{
        //    return Rules.Where(r =>
        //                (r.BuildDefinitionId == null || r.BuildDefinitionId == id) &&
        //                r.TriggerType == triggerType &&
        //                (r.TriggerPerson == null || r.TriggerPerson == triggerPerson))
        //        .OrderByDescending(r => r.PriorityId)
        //        .FirstOrDefault();
        //}

        // todo: Hard-coding the CiEntryPoints for now, but maybe use Reflection eventually?
        public static IEnumerable<ICiEntryPoint> CiEntryPoints => new[]
        {
            (ICiEntryPoint)new HudsonCIEntryPoint(),
            new MockCiEntryPoint()
        };

        public IEnumerable<PersonSetting> VisiblePeople
        {
            get { return People.Where(i => !i.Hidden); }
        }

        public static int GenericSosOnlineAvatarId
        {
            get { return AVATAR_COUNT; }
        }

        private void TrySetDefaultRule(TriggerType triggerType, int audioDuration, bool setLed, AudioPattern firstAudioPattern, LedPattern firstLedPattern)
        {
            Rule rule = Rules.FirstOrDefault(r => r.TriggerType == triggerType && r.BuildDefinitionId == null && r.TriggerPerson == null);
            if (rule != null)
            {
                rule.InheritAudioSettings = false;
                rule.AudioPattern = firstAudioPattern;
                rule.AudioDuration = audioDuration;
                rule.InheritLedSettings = !setLed;
                rule.LedPattern = setLed ? firstLedPattern : null;
                rule.LightsDuration = null;
            }
        }

        public void InitializeRulesForConnectedSiren(AudioPattern firstAudioPattern, LedPattern firstLedPattern)
        {
            //if (!SirenOfShameDevice.IsConnected) return;
            TrySetDefaultRule(TriggerType.BuildTriggered, 1, false, firstAudioPattern, firstLedPattern);
            TrySetDefaultRule(TriggerType.InitialFailedBuild, 10, true, firstAudioPattern, firstLedPattern);
            TrySetDefaultRule(TriggerType.SubsequentFailedBuild, 10, true, firstAudioPattern, firstLedPattern);
        }

        public PersonSetting FindAddPerson(string requestedBy, int avatarCount = AVATAR_COUNT)
        {
            if (string.IsNullOrEmpty(requestedBy))
            {
                _log.Warn("Tried to add a person with a null RawName");
                return null;
            }
            var person = FindPersonByRawName(requestedBy);
            if (person != null) return person;
            person = new PersonSetting
            {
                DisplayName = requestedBy,
                RawName = requestedBy,
                FailedBuilds = 0,
                TotalBuilds = 0,
                AvatarId = People.Count % avatarCount
            };
            People.Add(person);
            Dirty();
            return person;
        }

        public PersonSetting FindPersonByRawName(string rawName)
        {
            if (People == null) People = new List<PersonSetting>();
            var person = People.FirstOrDefault(i => SosDb.MakeCsvSafe(i.RawName) == SosDb.MakeCsvSafe(rawName));
            return person;
        }

        public string TryGetDisplayName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return userName;
            var person = People.FirstOrDefault(i => i.RawName != null && i.RawName.EndsWith(userName));
            return person == null ? userName : person.DisplayName;
        }

        public void UpdateNameIfChanged(BuildStatus changedBuildStatus)
        {
            var changedName = CiEntryPointSettings
                .SelectMany(i => i.BuildDefinitionSettings)
                .FirstOrDefault(i => i.Id == changedBuildStatus.BuildDefinitionId && i.Name != changedBuildStatus.Name);
            if (changedName == null) return;
            changedName.Name = changedBuildStatus.Name;
            Dirty();
        }

        public bool BuildExistsAndIsActive(string ciEntryPointName, string buildName)
        {
            var ciEntryPoint = CiEntryPointSettings.FirstOrDefault(i => i.Name == ciEntryPointName);
            if (ciEntryPoint != null)
            {
                return ciEntryPoint.BuildDefinitionSettings.Any(i => i.Name == buildName && i.Active);
            }
            return false;
        }

        public bool IsMeOrDefault(PersonSetting person, bool defaultValue)
        {
            if (string.IsNullOrEmpty(MyRawName)) return defaultValue;
            return person.RawName == MyRawName;
        }

        public IEnumerable<BuildDefinitionSetting> GetAllActiveBuildDefinitions()
        {
            return CiEntryPointSettings.SelectMany(i => i.BuildDefinitionSettings).Where(i => i.Active);
        }

        public void SetSosOnlineProxyPassword(string rawPassword)
        {
            var cryptographyService = ServiceContainer.Resolve<CryptographyServiceBase>();
            SosOnlineProxyPasswordEncrypted = cryptographyService.EncryptString(rawPassword);
        }

        public string GetSosOnlineProxyPassword()
        {
            var cryptographyService = ServiceContainer.Resolve<CryptographyServiceBase>();
            return cryptographyService.DecryptString(SosOnlineProxyPasswordEncrypted);
        }

        public void SetSosOnlinePassword(string rawPassword)
        {
            var cryptographyService = ServiceContainer.Resolve<CryptographyServiceBase>();
            SosOnlinePassword = cryptographyService.EncryptString(rawPassword);
        }

        public string GetSosOnlinePassword()
        {
            var cryptographyService = ServiceContainer.Resolve<CryptographyServiceBase>();
            return cryptographyService.DecryptString(SosOnlinePassword);
        }

        public string ExportNewAchievements()
        {
            if (string.IsNullOrEmpty(MyRawName)) return null;
            DateTime? highWaterMark = GetHighWaterMark();
            var initialExport = highWaterMark == null;
            var currentUser = GetCurrentUser();
            if (currentUser == null) return null;
            var currentUsersAchievements = currentUser.Achievements;
            var achievementsAfterHighWaterMark = initialExport ? currentUsersAchievements : currentUsersAchievements.Where(i => i.DateAchieved > highWaterMark);
            var buildsAsExport = achievementsAfterHighWaterMark
                .Select(i => i.AsSosOnlineExport());
            var result = string.Join("\r\n", buildsAsExport);
            return string.IsNullOrEmpty(result) ? null : result;
        }

        private PersonSetting GetCurrentUser()
        {
            return People.FirstOrDefault(i => i.RawName == MyRawName);
        }

        public DateTime? GetHighWaterMark()
        {
            return SosOnlineHighWaterMark == null ? (DateTime?)null : new DateTime(SosOnlineHighWaterMark.Value);
        }

        //public void SaveUserIAm(ComboBox userIAm)
        //{
        //    string myRawName = string.IsNullOrEmpty(userIAm.Text) ? null : userIAm.Text;
        //    MyRawName = myRawName;
        //}

        //public void InitializeUserIAm(ComboBox userIAm)
        //{
        //    // Filter out mapped names
        //    var mappedPeople = from m in UserMappings select m.WhenISee;
        //    var people = from p in People
        //                 where !mappedPeople.Contains(p.ToString())
        //                 select p;

        //    userIAm.Items.Add("");
        //    foreach (var personInProject in people)
        //    {
        //        userIAm.Items.Add(personInProject.RawName);
        //    }
        //    userIAm.Text = MyRawName;
        //    if (!string.IsNullOrEmpty(MyRawName))
        //    {
        //        foreach (var item in userIAm.Items)
        //        {
        //            var personSetting = item as string;
        //            if (personSetting != null && personSetting == MyRawName)
        //            {
        //                userIAm.SelectedItem = item;
        //            }
        //        }
        //    }
        //}

        public bool GetSosOnlineContent()
        {
            if (SosOnlineAlwaysOffline) return false;
            // if someone doesn't want to check for the lastest software, they probably are on a private network and don't want random connections to SoS Online
            if (UpdateLocation != UpdateLocation.Auto) return false;
            return true;
        }

        //public IWebProxy GetSosOnlineProxy()
        //{
        //    if (string.IsNullOrEmpty(SosOnlineProxyUrl)) return null;
        //    if (string.IsNullOrEmpty(SosOnlineProxyUsername))
        //    {
        //        return new WebProxy(SosOnlineProxyUrl);
        //    }
        //    return new WebProxy(
        //        SosOnlineProxyUrl,
        //        false,
        //        new string[] { },
        //        new NetworkCredential(SosOnlineProxyUsername, GetSosOnlineProxyPassword())
        //        );
        //}

        public BuildDefinitionSetting FindBuildDefinitionById(string buildId)
        {
            return CiEntryPointSettings
                .SelectMany(i => i.BuildDefinitionSettings)
                .FirstOrDefault(bds => bds.Id == buildId);
        }

        public bool IsGettingStarted()
        {
            if (NeverShowGettingStarted) return false;
            bool anyServers = CiEntryPointSettings.Any();
            if (!anyServers) return true;
            bool connected = !string.IsNullOrEmpty(SosOnlineUsername);
            bool alwaysOffline = SosOnlineAlwaysOffline;
            return !connected && !alwaysOffline;
        }

        public IList<string> AllUsersMinusMappedOnes()
        {
            return CiEntryPointSettings
                .SelectMany(i => i.BuildDefinitionSettings)
                .SelectMany(bds => bds.PeopleMinusUserMappings(this))
                .Distinct()
                .ToList();
        }

        public List<InstanceUserDto> GetUsersContainedInBuildsAsDto(IList<BuildStatus> changedBuildStatuses)
        {
            return People
                .Where(person => changedBuildStatuses.Any(build => build.RequestedBy == person.RawName))
                .Select(i => new InstanceUserDto(i))
                .ToList();
        }

        //public void Backup()
        //{
        //    string fileName = GetConfigFileName();
        //    string backupFileName = string.Format("{0:yyyy-MM-dd-HH-mm-ss}-SirenOfShame.sosdb.bak", DateTime.Now);
        //    string path = GetSosAppDataFolder();
        //    var backupFileNameAndPath = Path.Combine(path, backupFileName);
        //    File.Copy(fileName, backupFileNameAndPath, true);
        //}

        //public static string GetAvatarsFolder()
        //{
        //    var sosAppDataFolder = GetSosAppDataFolder();
        //    var avatarsDir = Path.Combine(sosAppDataFolder, "Avatars");
        //    Directory.CreateDirectory(avatarsDir);
        //    return avatarsDir;
        //}

        //public static string GetAvatarImagePath(string avatarImageName)
        //{
        //    var avatarsFolder = GetAvatarsFolder();
        //    return Path.Combine(avatarsFolder, avatarImageName);
        //}
    }
}
