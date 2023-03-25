using Microsoft.AspNetCore.Mvc;
using RickAndMortyPersonInfoWebAPI.Models;
using RickAndMortyPersonInfoWebAPI.Services;
using System.Text.RegularExpressions;


namespace RickAndMortyPersonInfoWebAPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    {
        private APIManager api;

        public HomeController(IConfiguration config)
        {
            api = new APIManager(config);
        }

        [HttpGet("person")]
        public async Task<ActionResult<List<Character>>> Get(string name)
        {
            List<Character> characters = CacheManager.Get<List<Character>>(api.GetApi(APIs.CharacterAPI, name)) as List<Character>;

            if (characters == null)
            {
                CharactersData charactersData = await api.GetCharacters(name);

                if (charactersData.Characters == null || charactersData.Characters.Any(c => c.Name != name))
                    return NotFound();

                characters = charactersData.Characters.Where(c => c.Name == name).ToList();

                CacheManager.Put(api.GetApi(APIs.CharacterAPI, name), characters);
            }

            Regex regex = new Regex(@"\((.*?)\)");
            foreach (var character in characters)
            {
                character.Origin.Dimension = regex.Match(character.Origin.Name).Groups[1].Value;
                character.Origin.Name = regex.Replace(character.Origin.Name, "");
            }

            var charactersWithOutID = characters.Select(c => new
            {
                c.Name,
                c.Status,
                c.Species,
                c.Type,
                c.Gender,
                Origin = new Origin()
                {
                    Name = regex.Replace(c.Origin.Name, ""),
                    Dimension = regex.Match(c.Origin.Name).Groups[1].Value,
                    Type = c.Origin.Type,
                }
            });

            return Ok(charactersWithOutID);
        }

        [HttpPost("check-person")]
        public async Task<ActionResult<bool>> Post(string personName, string episodeName)
        {

            List<Character> characters = CacheManager.Get<List<Character>>(api.GetApi(APIs.CharacterAPI, personName)) as List<Character>;

            if (characters == null)
            {
                CharactersData charactersData = await api.GetCharacters(personName);

                if (charactersData.Characters == null || charactersData.Characters.Any(c => c.Name != personName))
                    return NotFound();

                characters = charactersData.Characters.Where(c => c.Name == personName).ToList();

                CacheManager.Put(api.GetApi(APIs.CharacterAPI, personName), characters);
            }

            Episode episode = CacheManager.Get<Episode>(api.GetApi(APIs.EpisodeAPI, episodeName)) as Episode;

            if (episode == null)
            {
                EpisodesData episodeData = await api.GetEpisodes(episodeName);

                if (episodeData.Episodes == null || episodeData.Episodes.Any(e => e.Name != episodeName))
                    return NotFound();             

                episode = episodeData.Episodes.Where(c => c.Name == episodeName).First();

                CacheManager.Put(api.GetApi(APIs.EpisodeAPI, episodeName), episode);
            }


            List<int> charactersIDInEpisode = new List<int>();
            foreach (var character in episode.Characters)
            {
                charactersIDInEpisode.Add(int.Parse(character.Split('/').Last()));
            }

            if (charactersIDInEpisode.Intersect(characters.Select(c => c.ID)).Any())
                return Ok(true);

            return Ok(false);
        }
    }
}