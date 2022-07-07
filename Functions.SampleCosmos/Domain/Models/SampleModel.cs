using Newtonsoft.Json;
using RauchTech.Extensions.Data.Cosmos.Models;
using System;

namespace Functions.SampleCosmos.Domain.Models
{
    public class SampleModel : EntityRoot
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("lastUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdate { get; set; }
    }
}
