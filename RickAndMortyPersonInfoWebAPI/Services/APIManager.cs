using Newtonsoft.Json;
using RickAndMortyPersonInfoWebAPI.Models;
using System.Net;

namespace RickAndMortyPersonInfoWebAPI.Services
{
    public enum APIs
    {
        CharacterAPI,
        EpisodeAPI
    }


    public class APIManager
    {
        private IConfiguration config;

        public APIManager(IConfiguration config)
        {
            this.config = config;
        }

        public string GetApi(APIs api, string key)
        {
            return string.Format(config.GetConnectionString(APIs.CharacterAPI.ToString()), key);

        }

        public async Task<CharactersData> GetCharacters(string key)
        {
            CharactersData charactersData = await Get<CharactersData>(string.Format(config.GetConnectionString(APIs.CharacterAPI.ToString()), key));
            if (charactersData.Info != null)
            {
                for (int i = 1; i < charactersData.Info.Pages; i++)
                {
                    CharactersData pageData = await Get<CharactersData>(charactersData.Info.Next);
                    charactersData.Characters.ToList().AddRange(pageData.Characters);
                }
            }

            return charactersData;
        }

        public async Task<EpisodesData> GetEpisodes(string key)
        {
            EpisodesData episodesData = await Get<EpisodesData>(string.Format(config.GetConnectionString(APIs.EpisodeAPI.ToString()), key));
            if (episodesData.Info != null)
            {
                for (int i = 1; i < episodesData.Info.Pages; i++)
                {
                    EpisodesData pageData = await Get<EpisodesData>(episodesData.Info.Next);
                    episodesData.Episodes.ToList().AddRange(pageData.Episodes);
                }
            }

            return episodesData;
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
                //TODO: wrtie log...
                return new T();
            }
        }
    }
}
