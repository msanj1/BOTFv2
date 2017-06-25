using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using System.Dynamic;
using BOTF.Infrastructure;
using System.Diagnostics;

namespace BOTF.Models
{

    public class FacebookPostSchedule
    {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual int ProposalID { get; set; }
        public virtual int ErrorCode { get; set; }
        public virtual bool ArtistPost { get; set; }
    }

    public class FacebookCommentSchedule
    {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual string FacebookPostId { get; set; }
                
        public virtual string Body { get; set; }
        public virtual int ErrorCode { get; set; }
    }


   
  

    public class FacebookAPI
    {
        FacebookClient client;
        ContextDb _db = new ContextDb();

       

        public FacebookAPI(string Token)
        {
            client = new FacebookClient(Token);
        }

      

        public dynamic GetUsersData()
        {
         
            try
            {
                 dynamic data=  client.Get("me", new { fields = "name, email,picture" });
                 return data;

            }
            catch (FacebookOAuthException)
            {
                return null;
            }
        }

       
        public dynamic InsertToFeed(Proposal proposal, int CurrentUser, string URL)
        {



            dynamic parameters = new ExpandoObject();

            parameters.message = "just suggested the event " + "->"
             + proposal.Artist + "<-" + " at " + "->" + proposal.Venue + "<-" + " for Battleof the Fans." + " Like this? Get on board and vote for it! "
             + "Awesome prizes to be won!" + " Battle of the Fans would like to thank you for dedication to the cause!";


            parameters.name = "We all love music! Who do you want to see?";
            parameters.link = URL;
            parameters.picture = proposal.Image;
            parameters.privacy = new { value = "EVERYONE", };

            try
            {
                return client.Post("/me/feed", parameters);
                
            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {
                  
                    proposal.FacebookPostId = "";
                    _db.SaveChanges();
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    _db.FacebookPostSchedule.Add(new FacebookPostSchedule { DateCreated = DateTime.Now, ErrorCode = 190, ProposalID = proposal.Id, UserId = CurrentUser, ArtistPost=false });
                    _db.SaveChanges();
                    return 2;
                }

                return 3;//another error has happenned 

            }
         
         
        
        }

        public static string GetLongtermFbToken(string existingToken)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Get("oauth/access_token",
                                    new
                                    {
                                        client_id = Settings.Settings.FacebookAppId,
                                        client_secret = Settings.Settings.FacebookAppSecret,
                                       
                                        grant_type = "fb_exchange_token",
                                        fb_exchange_token = existingToken
                                    });

            return result.access_token;
        }

        public dynamic getComments(string PostID)
        {
            try
            {
                dynamic post = client.Get(PostID + "/comments"); //comments
                //if (post is Boolean)
                //{

                //    return false;
                //}
                return post;
            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {
                    Proposal proposal = _db.Proposal.FirstOrDefault(c => c.FacebookPostId == PostID);
                    proposal.FacebookPostId = "";
                    _db.SaveChanges();
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    return 2;
                }

                return 3;//another error has happenned 

            }
        }

        public int postCommentToPost(string postID, string body, int CurrentUser)
        {
          
            dynamic parameters = new ExpandoObject();
            parameters.message = body;
            try
            {
                client.Post(postID + "/comments", parameters);
                return 0; //post was successfull
            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {
                    Proposal proposal = _db.Proposal.FirstOrDefault(c => c.FacebookPostId == postID);
                    proposal.FacebookPostId = "" ;
                    _db.SaveChanges();
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    _db.FacebookCommentSchedule.Add(new FacebookCommentSchedule { Body = body, DateCreated = DateTime.Now, ErrorCode = 190, FacebookPostId = postID, UserId = CurrentUser });
                    _db.SaveChanges();
                    return 2;
                }

                return 3;//another error has happenned 
               
            }
           

           
           
        }

        public int deleteComment(string commentID,string postID)
        {
            try
            {
                client.Delete(commentID);
                return 0;
            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {
                    Proposal proposal = _db.Proposal.FirstOrDefault(c => c.FacebookPostId == postID);
                    proposal.FacebookPostId = "";
                    _db.SaveChanges();
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    return 2;
                }

                return 3;//another error has happenned 

            }
           

        }

        public dynamic GetFacebookUsersFriends()
        {
          
            try
            {
                
               var friends = client.Get("fql", new { q = "SELECT uid2 FROM friend WHERE uid1 = me()" });
                var data = (IDictionary<string, object>)friends;
             
                return data;
            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {
                 
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    return 2;
                }

                return 3;//another error has happenned 

            }
          
          
        }
        public dynamic InsertToArtistFeed(Proposal proposal, int CurrentUser, string URL="")
        {

            



            dynamic parameters = new ExpandoObject();

            parameters.message = "just suggested you play an event at " + proposal.Venue + " on Battle of the Fans. It has already received " + proposal.Votes + " votes!. Check it out and let your fans know what you think";
        
            parameters.name = "We all love music! Who do you want to see?";
            parameters.link = URL;
            parameters.privacy = new { value = "EVERYONE", };
            string FacebookID = getArtistFacebookID(proposal.Artist,proposal,CurrentUser);
          

          
            try
            {
                if (FacebookID == "")
                {
                    return 4; //could not find the artist page 
                }
                else
                {
                  
                    var id = client.Post(FacebookID + "/feed", parameters);
                    return id;
                }
              

            }
            catch (FacebookApiException ex)
            {
                if (ex.ErrorCode == 100) //postId does not exist
                {

                    proposal.FacebookPostIdArtist = "";
                    _db.SaveChanges();
                    return 1;
                }
                if (ex.ErrorCode == 190) //Token Expired
                {
                    _db.FacebookPostSchedule.Add(new FacebookPostSchedule { DateCreated = DateTime.Now, ErrorCode = 190, ProposalID = proposal.Id, UserId = CurrentUser, ArtistPost=true });
                    _db.SaveChanges();
                    return 2;
                }

                return 3;//another error has happenned 

            }

        }

        public string getArtistFacebookID(string ArtistName, Proposal proposal, int Current_User)
        {
            string artistName = "BOTF " + ArtistName;
            string ArtistID = "";
            try
            {
                var output = client.Get("fql", new { q = "SELECT page_id FROM page WHERE contains('" + artistName + "') limit 1" });
                var data = (IDictionary<string, object>)output;
                if (data.ContainsKey("data"))
                {
                    var temp = (List<object>)data["data"];
                    foreach (IDictionary<string, object> item in temp)
                    {
                        ArtistID = item["page_id"].ToString();

                    }
                }
                return ArtistID;
            }
            catch (FacebookApiException ex)
            {
                _db.FacebookPostSchedule.Add(new FacebookPostSchedule { DateCreated = DateTime.Now, ErrorCode = 190, ProposalID = proposal.Id, UserId = Current_User, ArtistPost = true });
                _db.SaveChanges();
                return "";
                
            }
          
          

        }

    }

   
}