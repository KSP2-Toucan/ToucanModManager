﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToucanServices.SpacedockAPI.Models
{
    public struct SearchModModelVersion
    {
        [JsonProperty("friendly_version")]
        public string ModVersion { get; set; }

        [JsonProperty("game_version")]
        public string GameVersion { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created;

        [JsonProperty("download_path")]
        public string DownloadUrl { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("downloads")]
        public UInt32 Downloads { get; set; }
    }

    public struct SpacedockAPISearchModModel
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("id")] public UInt32 Id { get; set; }
        [JsonProperty("game")] public string Game { get; set; }
        [JsonProperty("game_id")] public UInt32 GameId { get; set; }
        [JsonProperty("short_description")] public string ShortDescription { get; set; }
        [JsonProperty("downloads")] public UInt32 Downloads { get; set; }
        [JsonProperty("followers")] public UInt32 Followers { get; set; }
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("license")] public string License { get; set; }
        [JsonProperty("website")] public string Website { get; set; }
        [JsonProperty("donations")] public string Donations { get; set; }
        [JsonProperty("source_code")] public string SourceCode { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("versions")] public List<SearchModModelVersion> Versions { get; set; }
    }
}
