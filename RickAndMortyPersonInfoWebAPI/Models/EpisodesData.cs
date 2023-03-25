using Newtonsoft.Json;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class EpisodesData
    {
        public Info Info { get; set; }

        [JsonProperty("results")]
        public List<Episode> Episodes { get; set; }

        public bool IsEmpty()
        {
            return Info == null || Episodes == null;
        }
    }
}
