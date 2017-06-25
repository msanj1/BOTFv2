using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TweetSharp;
using BOTF.Models;
using BOTF.Infrastructure;
using BOTF.Filters;
using WebMatrix.WebData;
using Microsoft.Web.WebPages.OAuth;
using BOTF.ModelView;
using System.Web;
/*a controller for the twitter parts of the website*/
namespace BOTF.Controllers
{

    public class TwitterProposalController : ApiController
    {
        ContextDb _db = new ContextDb();
        // GET api/twitterproposal

        [Authorize]
        [InitializeSimpleMembership]
        //this function either returns the list of friends not using botf or just return a list of twitter freinds who have a local account
        public List<Models.User> Get(bool invite = false)
        {
            List<Models.User> output = new List<Models.User>();
            Models.Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "twitter");
            if (service != null)
            {
                TwitterService twitter = new TwitterService(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, service.Token, service.TokenSecret);

                var friends = twitter.ListFriends().ToList();
                
                foreach (var friend in friends)
                {
                    
                    var username = OAuthWebSecurity.GetUserName("twitter", friend.Id.ToString());
                    if (username != null && invite == false)
                    {
                        int Id = WebSecurity.GetUserId(username);
                        Models.User user = _db.User.FirstOrDefault(c => c.UserId == Id);
                        output.Add(user);
                     
                    }
                    else if(username == null && invite == true)
                    {
                        output.Add(new Models.User { Id = friend.Id, Image=friend.ProfileImageUrl, Name=friend.ScreenName});

                       
                     
                    }

                    

                }
               
            }
            return output;
        }
         

        // GET api/twitterproposal/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/twitterproposal
        //post a proposal to tweeter
        [Authorize]
        [InitializeSimpleMembership]
        public HttpResponseMessage Post([FromUri]int Id, [FromUri]bool ArtistPost)//check for artist or wall posts
        {
               Proposal currProposal =  _db.Proposal.FirstOrDefault(c=>c.Id == Id);
               Models.Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c=>c.Provider == "twitter");

               if (service != null)
               {

                   TwitterService twitter = new TwitterService(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, service.Token, service.TokenSecret);
                   try
                   {
                       if (ArtistPost == true)
                       {
                           //string message = "test message";
                           var result = twitter.SearchForUser(currProposal.Artist).ToList();
                           
                           string message = "@" + result[0].Name.Replace(" ",string.Empty) + " It was suggested tha you play an event at " + currProposal.Venue + " for more info go to www.botf.azurewebsites.net";
                           twitter.SendTweet(message);
                         
                       }
                       else
                       {
                          
                           string message = "just suggested the event " + "->"
                              + currProposal.Artist + "<-" + " at " + "->" + currProposal.Venue + "<-" + "." + " Like this? Get on board! at " + System.Web.HttpContext.Current.Request.UrlReferrer.ToString();



                        twitter.SendTweet(message);

                          
                           
                       }
                     
                     
                       return Request.CreateResponse(HttpStatusCode.OK);
                   }
                   catch (Exception)
                   {

                       return Request.CreateResponse(HttpStatusCode.Conflict);
                   }
               }
               else
               {
                   return Request.CreateResponse(HttpStatusCode.BadRequest);

               }
              
        }

        // PUT api/twitterproposal/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/twitterproposal/5
        public void Delete(int id)
        {
        }
    }
}
