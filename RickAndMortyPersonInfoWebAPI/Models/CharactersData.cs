using Newtonsoft.Json;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class CharactersData
    {
        public Info Info { get; set; }

        [JsonProperty("results")]
        public List<Character> Characters { get; set; }

        public bool IsEmpty()
        {
            return Info == null || Characters == null;
        }
    }
}
