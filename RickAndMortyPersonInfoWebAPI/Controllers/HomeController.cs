using Microsoft.AspNetCore.Mvc;
using RickAndMortyPersonInfoWebAPI.Models;
using RickAndMortyPersonInfoWebAPI.Services;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RickAndMortyPersonInfoWebAPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    {
        private readonly APIManager api;

        public HomeController(IConfiguration config)
        {
            api = new APIManager(config);
        }

        [HttpGet("person")]
        public async Task<ActionResult<List<Character>>> Get(string name)
        {
            List<Character> characters = await GetCharacters(name);
            if (characters == null || characters.Count() == 0)
                return NotFound();

            Regex regex = new Regex(@"\((.*?)\)");
            //use anon. to avoid ID property
            var output = characters.Select(c => new
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

            return Ok(output);
        }

        [HttpPost("check-person")]
        public async Task<ActionResult<bool>> Post(string personName, string episodeName)
        {
            List<Character> characters = await GetCharacters(personName);
            if (characters == null || characters.Count() == 0)
                return NotFound();

            string episodeCacheKey = CacheManager.GetKey(APIs.EpisodeAPI, episodeName);
            Episode episode = CacheManager.Get<Episode>(episodeCacheKey);

            if (episode == null)
            {
                episode = await api.GetEpisode(episodeName);

                if (episode == null || episode.Name == null)
                    return NotFound();

                CacheManager.Put(episodeCacheKey, episode);
            }


            List<int> characterIds = new List<int>();
            foreach (string character in episode.Characters)
            {
                if (int.TryParse(character.Split('/').Last(), out int characterId))
                    characterIds.Add(characterId);
            }

            if (characters.Select(c => c.ID)
                .Intersect(characterIds)
                .Any())
            {
                return Ok(true);
            }

            return Ok(false);
        }

        private async Task<List<Character>> GetCharacters(string name)
        {
            string caharactersCacheKey = CacheManager.GetKey(APIs.CharacterAPI, name);
            List<Character> characters = CacheManager.Get<List<Character>>(caharactersCacheKey);

            if (characters == null)
            {
                characters = await api.GetCharactersByName(name);
                if (characters == null || characters.Count() == 0)
                    return characters;

                CacheManager.Put(caharactersCacheKey, characters);
            }

            return characters;
        }
    }
}