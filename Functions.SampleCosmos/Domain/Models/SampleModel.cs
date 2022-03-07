using Newtonsoft.Json;
using RauchTech.Extensions.Data.Cosmos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Models
{
    public class SampleModel : EntityRoot
    {
        [JsonPropertyName("lastUpdate")] //For Swagger usage (this version does not understand Newtonsoft yet)
        [JsonProperty("lastUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdate { get; set; }
    }
}
