using Newtonsoft.Json;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class EpisodesData
    {
        public Info Info { get; set; }

        [JsonProperty("results")]
        public Episode[] Episodes { get; set; }
    }
}
