using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.ModelView;
using BOTF.Models;
using BOTF.Infrastructure;
using WebMatrix.WebData;
using BOTF.Filters;
using System.Web;
using Microsoft.Web.WebPages.OAuth;
using System.Web.Services;
using DotNetOpenAuth.AspNet;
/*This controller takes care of the facebook proposals posts*/
namespace BOTF.Controllers
{
   [InitializeSimpleMembership]
    
    public class FacebookProposalController : ApiController
    {
        ContextDb _db = new ContextDb();
        DatabaseCallsApi _api = new DatabaseCallsApi();
        // GET api/facebookproposal

       /*List of Facebook friends*/
       [Authorize]
        public IEnumerable<Models.User> Get()
        {
            List<Models.User> friends = new List<Models.User>();
            
                var token = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "facebook").Token;
                if (token != null)
                {
                    FacebookAPI facebook = new FacebookAPI(token);
                    dynamic status = facebook.GetFacebookUsersFriends();
                    if (!(status is int) && status.ContainsKey("data"))
                    {
                        foreach (var friend in status.data)
                        {
                            //do something here
                            string id = friend.uid2;
                            var username = OAuthWebSecurity.GetUserName("facebook", id);
                            if (username != null)
                            {
                                var local_id = WebSecurity.GetUserId(username);
                                Models.User user = _db.User.FirstOrDefault(c => c.UserId == local_id);
                                if (user != null)
                                {
                                    friends.Add(user);
                                   
                                }
                            }
                        }
                    }
                }
        
          
           return friends;
        }

        // GET api/facebookproposal/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/facebookproposal
       /*This function will either post a proposal to an artist page or to the user's wall depending on the value of ArtistPost*/
       [Authorize]
        public HttpResponseMessage Post(int Id, bool ArtistPost)
        {

        
               Proposal currProposal =  _db.Proposal.FirstOrDefault(c=>c.Id == Id);
               Models.Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c=>c.Provider == "facebook");

               if (service != null )
               {
                   FacebookAPI facebook = new FacebookAPI(service.Token);
                  
                   if (service != null && currProposal != null)
                   {
                       dynamic status;
                       if (ArtistPost)
                       {
                           status = facebook.InsertToArtistFeed(currProposal, WebSecurity.CurrentUserId, System.Web.HttpContext.Current.Request.UrlReferrer.ToString());
                       }
                       else
                       {
                           status = facebook.InsertToFeed(currProposal, WebSecurity.CurrentUserId,System.Web.HttpContext.Current.Request.UrlReferrer.ToString());
                       }
                     
                       if (status is int && status == 1)
                       {
                           return Request.CreateResponse(HttpStatusCode.NotFound);
                       }
                       else if (status is int && status == 2)
                       {
                           return Request.CreateResponse(HttpStatusCode.BadGateway);
                       }
                       else if (status is int && status == 3)
                       {
                           return Request.CreateResponse(HttpStatusCode.BadRequest);
                       }
                       else if (status is int && status == 4)
                       {
                           return Request.CreateResponse(HttpStatusCode.Created);
                       }
                       else
                       {
                           if (ArtistPost)
                           {
                               _api.AddOrUpdateFacebookArtistPost(currProposal.Id, WebSecurity.CurrentUserId, status["id"].ToString()); //saving post id from facebook
                           }
                           else
                           {
                               _api.AddOrUpdateFacebookPost(currProposal.Id, WebSecurity.CurrentUserId, status["id"].ToString());
                           }
                        

                           return Request.CreateResponse(HttpStatusCode.Created);
                       }
                   }
                  
                  
               }


               return Request.CreateResponse(HttpStatusCode.BadRequest);
           

          
        }

        // PUT api/facebookproposal/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/facebookproposal/5
        public void Delete(int id)
        {
        }
    }
}
