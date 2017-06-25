using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.Models;
namespace BOTF.Controllers
{
    public class VenueController : ApiController
    {
        // GET api/venue
        LastFM lastfm = new LastFM(Settings.Settings.LastFMKey, "http://ws.audioscrobbler.com/2.0/");
        public IEnumerable<string> Get()
        {
        
            return new string[] { "value1", "value2" };
        }

        // GET api/venue/5
        public List<string> Get([FromUri]string Venue)
        {
            List<string> output = new List<string>();
            output =  lastfm.SearchVenues(Venue);
            return output;
        }

        // POST api/venue
        public void Post([FromBody]string value)
        {
        }

        // PUT api/venue/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/venue/5
        public void Delete(int id)
        {
        }
    }
}
