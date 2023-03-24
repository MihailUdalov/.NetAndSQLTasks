using Newtonsoft.Json;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class Data
    {
        public Info Info { get; set; }

        [JsonProperty("results")]
        public RequestInformation[] DataAboutCharaters { get; set; }
    }
}
