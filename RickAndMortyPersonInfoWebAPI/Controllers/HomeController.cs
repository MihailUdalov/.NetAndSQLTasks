using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RickAndMortyPersonInfoWebAPI.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace RickAndMortyPersonInfoWebAPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    {
        IConfiguration configuration;
        public HomeController(IConfiguration iConfiguration)
        {
            configuration = iConfiguration;
        }

        [HttpGet("person")]
        public async Task<ActionResult<List<Character>>> Get(string name)
        {
            string CharacterAPI = configuration.GetConnectionString("CharacterAPI");

            CharactersData characterData = new CharactersData();
            List<Character> characters = new List<Character>();

            characterData = GetAPIData<CharactersData>(string.Format(CharacterAPI, name));
            characters.AddRange(characterData.Results.Where(x => x.Name == name).Select(x => x).ToList());

            for (int i = 1; i < characterData.Info.Pages; i++)
            {
                characterData = GetAPIData<CharactersData>(characterData.Info.Next);
                characters.AddRange(characterData.Results.Where(x => x.Name == name).Select(x => x).ToList());
            }

            if (characters.Count == 0)
                return NotFound();

            Regex regex = new Regex(@"\((.*?)\)");
            foreach (var character in characters)
            {

                character.Origin.Dimension = regex.Match(character.Origin.Name).Groups[1].Value;
                character.Origin.Name = regex.Replace(character.Origin.Name, "");
            }


            return new ObjectResult(characters);
        }


        [HttpPost("check-person")]
        public async Task<ActionResult<bool>> Post(string personName, string episodeName)
        {

            string CharacterAPI = configuration.GetConnectionString("CharacterAPI");

            Data characterData = new Data();
            List<int> charactersID = new List<int>();

            characterData = GetAPIData<Data>(string.Format(CharacterAPI, personName));
            charactersID.AddRange(characterData.Results.Where(x => x.Name == personName).Select(x => x.ID).ToList());

            for (int i = 1; i < characterData.Info.Pages; i++)
            {
                characterData = GetAPIData<Data>(characterData.Info.Next);
                charactersID.AddRange(characterData.Results.Where(x => x.Name == personName).Select(x => x.ID).ToList());
            }

            if (charactersID.Count == 0)
                return NotFound();


            string EpisodeAPI = configuration.GetConnectionString("EpisodeAPI");

            Data episodeData = new Data();
            RequestInformation episode = new RequestInformation();

            episodeData = GetAPIData<Data>(string.Format(EpisodeAPI, episodeName));
            episode = episodeData.Results.Where(x => x.Name == episodeName).FirstOrDefault();

            if (episode is null)
                return NotFound();

            List<int> charactersIDInEpisode = new List<int>();
            foreach (var character in episode.Characters)
            {
                charactersIDInEpisode.Add(int.Parse(character.Split('/').Last()));
            }


            if (charactersIDInEpisode.Intersect(charactersID).Any())
                return Ok(true);

            return Ok(false);
        }


        static T GetAPIData<T>(string APIURL)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(APIURL);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            T data = JsonConvert.DeserializeObject<T>(response);

            return data;
        }
    }
}