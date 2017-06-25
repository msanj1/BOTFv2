using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using BOTF.Models;
using BOTF.Filters;
using BOTF.ModelView;
using TweetSharp;
using BOTF.Infrastructure;
using Microsoft.Web.WebPages.OAuth;
namespace BOTF.Controllers
{
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        DatabaseCallsApi api = new DatabaseCallsApi();
        LastFM lastfm = new LastFM(Settings.Settings.LastFMKey, "http://ws.audioscrobbler.com/2.0/"); //setting up last fm

        public ActionResult Index(string Filter = "")
        {
           
            ModelView.User user = new ModelView.User();
            if (  WebSecurity.IsAuthenticated == true)
            {
               int id= WebSecurity.CurrentUserId;
               var userinfo = api.GetUserInfo(id);
                //get user's info
               user = new ModelView.User { Id = userinfo.Id, Email = userinfo.Email, Name = userinfo.Name, Image = userinfo.Image, RemainingProposals = userinfo.RemainingProposals, RemainingVotes = userinfo.RemainingVotes };
            }

            ViewBag.Filter = Filter; //viewbag for filtering
            return View(user);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /*This function send a message to friends who are not using BOTF*/
        [Authorize]
        [HttpPost]
        public ActionResult TweetRequest(int[] Id)
        {
            if (OAuthWebSecurity.IsAuthenticatedWithOAuth)
            {
                ContextDb _db = new ContextDb();
                Models.Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "twitter");
                if (service != null)
                {
                    TwitterService twitter = new TwitterService(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, service.Token, service.TokenSecret);
                    foreach (var item in Id)
                    {
                        twitter.SendDirectMessage(item, "Join us at www.botf.azurewebsites.net");
                    }


                }
            }
         
            return RedirectToAction("Index");

        }

        /*type ahead functions*/
        [HttpGet]
        public JsonResult GetVenuesApi(string venue)
        {
            if (venue.Length > 3)
            {
              
                List<string> names;

                names = lastfm.SearchVenues(venue);
                return Json(names, JsonRequestBehavior.AllowGet);
            }
            else return Json(true, JsonRequestBehavior.AllowGet);
        }
        /*type ahead functions*/
        [HttpGet]
        public JsonResult GetArtistsApi(string artist)
        {

            if (artist.Length > 3)
            {
              
                List<string> names;

                names = lastfm.SearchArtist(artist);
                return Json(names, JsonRequestBehavior.AllowGet);
            }
            else return Json(true, JsonRequestBehavior.AllowGet);
        }
        /*Used to load the genres for filtering*/
        public JsonResult GetGenres() {
            ContextDb _db = new ContextDb();
            List<string> Genres =  _db.Proposal.Select(c => c.Genre).Distinct().ToList();
            return Json(Genres,JsonRequestBehavior.AllowGet);
        }
    }


}
