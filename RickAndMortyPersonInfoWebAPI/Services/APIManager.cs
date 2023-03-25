using Newtonsoft.Json;
using RickAndMortyPersonInfoWebAPI.Models;
using System.Net;

namespace RickAndMortyPersonInfoWebAPI.Services
{
    public class APIManager
    {
        private IConfiguration config;

        public APIManager(IConfiguration config)
        {
            this.config = config;
        }

        public async Task<List<Character>> GetCharactersByName(string name)
        {
            string url = string.Format(config.GetConnectionString(APIs.CharacterAPI.ToString()), name);
            CharactersData charactersData = await Get<CharactersData>(url);

            List<Character> characters = new List<Character>();
            if (charactersData.IsEmpty())
                return characters;

            //add characters from the first page
            characters.AddRange(charactersData.Characters);
            for (int i = 1; i < charactersData.Info.Pages; i++)
            {
                charactersData = await Get<CharactersData>(charactersData.Info.Next);
                characters.AddRange(charactersData.Characters);
            }

            return characters.Where(c => c.Name == name).ToList();
        }

        public async Task<Episode> GetEpisode(string name)
        {
            string url = string.Format(config.GetConnectionString(APIs.EpisodeAPI.ToString()), name);
            EpisodesData episodesData = await Get<EpisodesData>(url);

            List<Episode> episodes = new List<Episode>();
            if (episodesData.IsEmpty())
                return new Episode();

            episodes.AddRange(episodesData.Episodes);
            for (int i = 1; i < episodesData.Info.Pages; i++)
                {
                    episodesData = await Get<EpisodesData>(episodesData.Info.Next);
                    episodes.AddRange(episodesData.Episodes);
                }      

            return episodes.Where(e => e.Name == name).FirstOrDefault();
        }

        private async Task<T> Get<T>(string url) where T : new()
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();


                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string response = await streamReader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<T>(response);
                }
            }
            catch (Exception ex)
            {
                //TODO: wrtie a log...
                return new T();
            }
        }
    }
}
