﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SirenOfShame.Uwp.Watcher.Watcher;

namespace SirenOfShame.Uwp.Watcher.Settings
{
    // todo: figure out settings serialization
    //[Serializable]
    public class BuildDefinitionSetting
    {
        public BuildDefinitionSetting()
        {
            People = new List<string>();
        }

        public BuildDefinitionSetting(MyBuildDefinition buildDefinition, string buildServer) : this()
        {
            Active = false;
            Id = buildDefinition.Id;
            Name = buildDefinition.Name;
            AffectsTrayIcon = true;
            BuildServer = buildServer;
            Parent = buildDefinition.Parent;
        }

        public bool Active { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool AffectsTrayIcon { get; set; }
        public List<string> People { get; set; }
        public string BuildServer { get; set; }

        /// <summary>
        /// For TFS this is the project collection that a build definition lives in, but it could be used for containing information for other CI build servers
        /// </summary>
        public string Parent { get; set; }

        public async Task<BuildStatus> AsUnknownBuildStatus(SosDb sosDb)
        {
            var readAll = await sosDb.ReadAll(this);
            var lastKnownBuild = readAll.LastOrDefault();
            var comment = lastKnownBuild == null ? null : lastKnownBuild.Comment;
            var startedTime = lastKnownBuild == null ? null : lastKnownBuild.StartedTime;
            // SosDb doesn't store local start time, so use the server's start time
            var localStartTime = lastKnownBuild == null ? DateTime.MinValue : lastKnownBuild.StartedTime ?? DateTime.MinValue;
            var buildId = lastKnownBuild == null ? null : lastKnownBuild.BuildId;
            var requestedBy = lastKnownBuild == null ? null : lastKnownBuild.RequestedBy;
            var finishedTime = lastKnownBuild == null ? null : lastKnownBuild.FinishedTime;
            var url = lastKnownBuild == null ? null : lastKnownBuild.Url;

            return new BuildStatus
            {
                BuildStatusEnum = BuildStatusEnum.Unknown,
                BuildDefinitionId = Id,
                Name = Name,
                StartedTime = startedTime,
                Comment = comment,
                LocalStartTime = localStartTime,
                BuildId = buildId,
                RequestedBy = requestedBy,
                FinishedTime = finishedTime,
                Url = url
            };
        }

        public bool ContainsPerson(BuildStatus buildStatus)
        {
            if (buildStatus == null) return false;
            return People.Any(p => p == buildStatus.RequestedBy);
        }

        public IEnumerable<string> PeopleMinusUserMappings(SirenOfShameSettings settings)
        {
            return People.Where(person => UserMappingsDoNotContain(settings, person));
        }

        private static bool UserMappingsDoNotContain(SirenOfShameSettings settings, string person)
        {
            return settings.UserMappings.All(um => um.WhenISee != person);
        }
    }
}
