using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BOTF.Models
{
    public class LastFM
    {

     string apiKey;
     string host;
        public LastFM(string API, string Host )
        {
             apiKey = API;
             host = Host;
        
        }

        class SampleData
        {
            [JsonProperty(PropertyName = "items")]
            public System.Data.DataTable Items { get; set; }
        }

        static List<string> _venues = new List<string>() { "Goma, Brisbane", "The Tivoli. Fortitude Valley, Brisbane, Queensland", "Brisbane Entertainment Centre", "Rod Laver Arena, Melbourne", "Eatons Hill Hotel & Function Centre", "Eatons Hill Hotel & Function Centre" };
        public List<string> SearchVenues(string searchTerm)
        {
            List<string> venues = new List<string>();
            foreach (var venue in _venues.Where(v => v.IndexOf(searchTerm,StringComparison.CurrentCultureIgnoreCase) > 0))
            {
                venues.Add(venue);
            }
            return venues;
        }

        public List<string> GetSimilarArtists(string artistName) 
        {
            string URL = host + "?method=artist.getinfo&artist=" + artistName + "&api_key=" + apiKey + "&autocorrect=1";
            var client = new WebClient();
            string xml = client.DownloadString(URL);
            List<string> tags = new List<string>();
            var xdoc = XDocument.Load(new StringReader(xml));
            var names = xdoc.Descendants("artist").Elements("similar").Elements("artist").Elements("name").Select(r => r.Value).Take(5).ToList();
            return names;
        }

        public dynamic GetArtistInfo( string artistName)
        {
          
            string URL = host + "?method=artist.getinfo&artist=" + artistName + "&api_key=" + apiKey + "&autocorrect=1";
            var client = new WebClient();
            string xml = client.DownloadString(URL);


            
            List<string> tags = new List<string>();

            var xdoc = XDocument.Load(new StringReader(xml));
            var data = from item in xdoc.Descendants("artist")
                       select new
                       {
                           artist = item.Element("name").Value,
                           image = item.Element("image").Attribute("small").Value,
                       };
            var correctname = xdoc.Descendants("artist").Elements("name").Select(r => r.Value).ToArray();
            var tagdata = xdoc.Descendants("artist").Elements("tags").Elements("tag").Elements("name").Select(r => r.Value).ToArray();
            var imgs = xdoc.Descendants("artist").Elements("image")
               .Where(node => (string)node.Attribute("size") == "mega")
               .Select(node => node.Value.ToString())
               .ToList();
            var artistbio = xdoc.Descendants("artist").Elements("bio").Elements("summary").Select(r => r.Value).Take(1).ToArray();
            var genre = xdoc.Descendants("artist").Elements("tags").Elements("tag").Elements("name").Select(r => r.Value).Take(1).ToArray();
            string image = "";
            string bio = "";
            string Genre = "";

           
           
            image = imgs[0];
            bio = artistbio[0];
            Genre = genre[0];

            return new { PictureURL = image, ArtistBio = bio, MusicGenre = Genre, ArtistName = correctname[0] };
        }

        public List<string> SearchArtist(string artist)
        {
            string url = host + "?method=artist.search&artist=" + artist + "&api_key=" + apiKey + "&limit=5";
            var client = new WebClient();
            string xml = client.DownloadString(url);
            xml = xml.Replace("opensearch:", "");

            List<string> names = new List<string>();
            XDocument doc = XDocument.Parse(xml);
       
            //XmlTextReader reader = new XmlTextReader(new StringReader(xml));
            //reader.MoveToContent();
            foreach (var artistName in doc.Descendants("artist").Select(art=>(string)art.Element("name")))
            {
                names.Add(artistName);
            }
            return names;
        }
    }
}