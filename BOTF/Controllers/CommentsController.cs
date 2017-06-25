using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.ModelView;
using BOTF.Models;
using BOTF.Infrastructure;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using BOTF.Filters;

/*This controller takes care of all of the comments*/
namespace BOTF.Controllers
{
    public class CommentsController : ApiController
    {
     
        ContextDb _db = new ContextDb();
        // GET api/comments
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/comments/5
        /*This function grabs comments for a proposal*/
        [Authorize]
        public IEnumerable<ViewCommentHistory> Get([FromUri]int Id)
        {
            List<Comment> list = _db.Comment.Select(c => c).Where(c => c.ProposalId == Id).ToList();
            List<ViewCommentHistory> output = new List<ViewCommentHistory>();
            foreach (var item in list)
            {
                Models.User user = _db.User.FirstOrDefault(c=>c.UserId == item.CreatedBy);
                if (item.CreatedBy == WebSecurity.CurrentUserId)//check if owner of the comment
                {
                     output.Add(new ViewCommentHistory { Id = item.Id.ToString(), Body = item.Body, CreatedBy = user.Name, DateCreated = item.CreatedDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz"), UserId=user.Id.ToString(), isFacebook=false });
                }
                else
                {
                    output.Add(new ViewCommentHistory { Body = item.Body, CreatedBy = user.Name, DateCreated = item.CreatedDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz"), UserId = user.UserId.ToString(), isFacebook =false });
                }
               
            }
           //get a service facebook or twitter
               Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "facebook");
               if (service != null)
               {
                   FacebookAPI facebook = new FacebookAPI(service.Token);
                   Proposal proposal = _db.Proposal.FirstOrDefault(c => c.Id == Id && c.FacebookPostId != null && c.FacebookPostId != "");
                    
                   if (proposal != null)
                   {
                      
                        dynamic comments = facebook.getComments(proposal.FacebookPostId); //get facebook post comments
                        if ((comments is int) == false && comments != null &&comments.ContainsKey("data"))
                        {
                            foreach (dynamic item in comments.data)
                            {
                                string id = item.id;
                                string body = item.message;
                                string created_date = item.created_time;
                                string created_by = item.from.name;
                                string created_by_facebookID = item.from.id;
                                string userID = OAuthWebSecurity.GetUserName("facebook",created_by_facebookID);
                                var LoggedInUserID = OAuthWebSecurity.GetAccountsFromUserName(userID).FirstOrDefault(c=>c.Provider=="facebook").ProviderUserId;


                                if (LoggedInUserID != null && LoggedInUserID == created_by_facebookID)
                                {
                                    output.Add(new ViewCommentHistory { Id = id, Body = body, DateCreated = created_date, CreatedBy = created_by, UserId = created_by_facebookID, isFacebook = true, Artist = false });
                                }
                                else
                                {
                                    output.Add(new ViewCommentHistory { Body = body, DateCreated = created_date, CreatedBy = created_by, UserId = created_by_facebookID, isFacebook = true, Artist = false });
                                }
                             
                            }
              
                        }
                       output =  output.OrderByDescending(c => DateTime.Parse(c.DateCreated)).ToList(); //reorder the comments
                        if (proposal.FacebookPostIdArtist != "" && proposal.FacebookPostIdArtist != null)//used to identify if an artist post has been made if yes then grab the latest comment from Artist
                        {
                            comments = facebook.getComments(proposal.FacebookPostIdArtist);
                            if ((comments is int) == false && comments != null && comments.ContainsKey("data"))
                            {

                                foreach (dynamic item in comments.data)
                                {
                                    string id = item.id;
                                    string body = item.message;
                                    string created_date = item.created_time;
                                    string created_by = item.from.name;
                                    string created_by_facebookID = item.from.id;
                                 
                                    output.Insert(0,new ViewCommentHistory { Body = body, DateCreated = created_date, CreatedBy = created_by, UserId = created_by_facebookID, isFacebook = true, Artist = true });
                                    break;
                                }

                            }
                        }
                        
                        
                   }
                   proposal = _db.Proposal.FirstOrDefault(c => c.Id == Id && c.FacebookPostId != null && c.FacebookPostId != "");
               }


               output = output.OrderByDescending(c => DateTime.Parse(c.DateCreated)).ToList();
            return output.ToList();
            
        }

        // POST api/comments
        /*function to post a new comment*/
        [Authorize]
        public HttpResponseMessage PostComment(ViewComment comment)
        {
            if (ModelState.IsValid)
            {

               
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                    _db.Comment.Add(new Comment { ProposalId = comment.Id, Body = comment.Body, CreatedBy = WebSecurity.CurrentUserId, CreatedDate = DateTime.UtcNow });
                    _db.SaveChanges();
                   
                        Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "facebook"); //get service for posting to feed
                        Proposal proposal = _db.Proposal.FirstOrDefault(c=>c.Id == comment.Id && c.FacebookPostId != null && c.FacebookPostId != "");
                        if (service != null && proposal != null)
                        {
                            FacebookAPI facebook = new FacebookAPI(service.Token);
                            int status =  facebook.postCommentToPost(proposal.FacebookPostId, comment.Body,WebSecurity.CurrentUserId);
                         
                            if (status == 1)
                            {
                                 return Request.CreateResponse(HttpStatusCode.NotFound);
                            }
                            else if(status == 2)
                            {
                                return Request.CreateResponse(HttpStatusCode.BadGateway);
                            }
                            else if (status == 3)
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest);
                            }
                            //facebook.postCommentToPost();
                        }
                       
                    
                   
                    return response;
              
               


            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // PUT api/comments/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/comments/5
        /*Delete a post*/
        [Authorize]
        public HttpResponseMessage Delete(Viewdelete delete)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
          
          

                if (delete.isFacebook)
                  {
                      Service service = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == "facebook");
                      if (service != null)
                      {
                          FacebookAPI facebook = new FacebookAPI(service.Token);
                          int status = facebook.deleteComment(delete.Id.ToString(),delete.PostId); //delete comment from a post on facebook
                          if (status == 1)
                          {
                              return Request.CreateResponse(HttpStatusCode.NotFound);
                          }
                          else if (status == 2)
                          {
                              return Request.CreateResponse(HttpStatusCode.BadGateway);
                          }
                          else if (status == 3)
                          {
                              return Request.CreateResponse(HttpStatusCode.BadRequest);
                          }
                      }
                  }
                  else
                  {
                      int id = Convert.ToInt32(delete.Id);
                      Comment comment = _db.Comment.FirstOrDefault(c => c.Id == id);
                      _db.Comment.Remove(comment);
                      _db.SaveChanges();
                  }
                  return response;
           
           
               
         
           
            
        }
    }
}
