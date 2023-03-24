using Newtonsoft.Json;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class CharactersData
    {
        public Info Info { get; set; }

        [JsonProperty("results")]
        public Character[] Characters { get; set; }
    }
}
