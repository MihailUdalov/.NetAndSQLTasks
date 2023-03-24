using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RickAndMortyPersonInfoWebAPI.Models;
using System.Net;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RickAndMortyPersonInfoWebAPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    {
        IConfiguration configuration;
        private static ObjectCache cache = MemoryCache.Default;
        public HomeController(IConfiguration iConfiguration)
        {
            configuration = iConfiguration;
        }

        [HttpGet("person")]
        public async Task<ActionResult<List<Character>>> Get(string name)
        {
            string CharacterAPI = configuration.GetConnectionString("CharacterAPI");

            CharactersData characterData = new CharactersData();
            List<Character> characters = cache.Get(name) as List<Character>;

            if (characters == null)
            {
                characters = new List<Character>();
                
                characterData = await GetAPIData<CharactersData>(string.Format(CharacterAPI, name));
                characters.AddRange(characterData.Characters.Where(x => x.Name == name).Select(x => x).ToList());

                for (int i = 1; i < characterData.Info.Pages; i++)
                {
                    characterData = await GetAPIData<CharactersData>(characterData.Info.Next);
                    characters.AddRange(characterData.Characters.Where(x => x.Name == name).Select(x => x).ToList());
                }

                if (characters.Count == 0)
                    return NotFound();

                Regex regex = new Regex(@"\((.*?)\)");
                foreach (var character in characters)
                {

                    character.Origin.Dimension = regex.Match(character.Origin.Name).Groups[1].Value;
                    character.Origin.Name = regex.Replace(character.Origin.Name, "");
                }

                cache.Set(name, characters, DateTimeOffset.Now.AddMinutes(10));
            }


            return new ObjectResult(characters);
        }


        [HttpPost("check-person")]
        public async Task<ActionResult<bool>> Post(string personName, string episodeName)
        {

            string CharacterAPI = configuration.GetConnectionString("CharacterAPI");


            Data characterData = new Data();
            List<int> charactersID = cache.Get(string.Format(CharacterAPI, personName)) as List<int>;
            
            if (charactersID == null)
            {
                charactersID = new List<int>();

                characterData = await GetAPIData<Data>(string.Format(CharacterAPI, personName));
                charactersID.AddRange(characterData.DataAboutCharaters.Where(x => x.Name == personName).Select(x => x.ID).ToList());

                for (int i = 1; i < characterData.Info.Pages; i++)
                {
                    characterData = await GetAPIData<Data>(characterData.Info.Next);
                    charactersID.AddRange(characterData.DataAboutCharaters.Where(x => x.Name == personName).Select(x => x.ID).ToList());
                }

                if (charactersID.Count == 0)
                    return NotFound();

                cache.Set(string.Format(CharacterAPI, personName), charactersID, DateTimeOffset.Now.AddMinutes(10));
            }


            string EpisodeAPI = configuration.GetConnectionString("EpisodeAPI");

            Data episodeData = new Data();
            RequestInformation episode = new RequestInformation();
            List<int> charactersIDInEpisode = cache.Get(string.Format(EpisodeAPI, episodeName)) as List<int>;
            
            if (charactersIDInEpisode == null)
            {
                charactersIDInEpisode = new List<int>();

                episodeData = await GetAPIData<Data>(string.Format(EpisodeAPI, episodeName));
                episode = episodeData.DataAboutCharaters.Where(x => x.Name == episodeName).FirstOrDefault();

                if (episode == null)
                    return NotFound();


                foreach (var character in episode.Characters)
                {
                    charactersIDInEpisode.Add(int.Parse(character.Split('/').Last()));
                }

                cache.Set(string.Format(EpisodeAPI, episodeName), charactersIDInEpisode, DateTimeOffset.Now.AddMinutes(10));
            }

            if (charactersIDInEpisode.Intersect(charactersID).Any())
                return Ok(true);

            return Ok(false);
        }


        async static Task<T> GetAPIData<T>(string APIURL)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(APIURL);

            HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = await streamReader.ReadToEndAsync();
            }

            T data = JsonConvert.DeserializeObject<T>(response);

            return data;
        }
    }
}