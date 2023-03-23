using Microsoft.AspNetCore.Mvc;
using System.Net;
using System;
using Newtonsoft.Json;
using RickAndMortyPersonInfoWebAPI.Models;

namespace RickAndMortyPersonInfoWebAPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    { 
        public HomeController()
        {
            
        }

        [HttpGet ("person")]
        public async Task<ActionResult<bool>> Get(string name)
        {
            return Ok(true);
        }

        [HttpPost("check-person")]
        public async Task<ActionResult<bool>> Post(string personName, string episodeName)
        {
            return Ok(true);
        }


        static Data GetAPIData(string APIURL)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(APIURL);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            Data data = JsonConvert.DeserializeObject<Data>(response);

            return data;
        }
    }
}