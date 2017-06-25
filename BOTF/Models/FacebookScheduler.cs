using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BOTF.Infrastructure;
using BOTF.Models;
/*This class was used to carryout jobs that were not half finished.*/
namespace BOTF.Models
{
    public  class FacebookScheduler
    {
      

        public  void RunScheduler(string NewToken, int CurrentLoggedInUser)
        {
            ContextDb _db = new ContextDb();
            DatabaseCallsApi _api = new DatabaseCallsApi();
            FacebookAPI facebook = new FacebookAPI(NewToken);
            _api.AddOrUpdateService(CurrentLoggedInUser, "facebook", NewToken); //update user's token

            var Proposals_lists = _db.FacebookPostSchedule.Where(c => c.UserId == CurrentLoggedInUser && c.ErrorCode != 900).ToList() ;


            var Comments_lists = _db.FacebookCommentSchedule.Where(c => c.UserId == CurrentLoggedInUser && c.ErrorCode != 900).ToList() ;
            if (Proposals_lists.Count >0)
            {
                
               //


                foreach (var post in Proposals_lists)
                {

                    Proposal proposal = _db.Proposal.FirstOrDefault(c => c.Id == post.ProposalID);
                    if (proposal != null)
                    {
                        dynamic status;
                        if (post.ArtistPost)
                        {
                            status = facebook.InsertToArtistFeed(proposal, CurrentLoggedInUser, System.Web.HttpContext.Current.Request.UrlReferrer.ToString());
                            if (!(status is int))
                            {
                                proposal.FacebookPostIdArtist = status.id;
                                _db.FacebookPostSchedule.Remove(post);
                                _db.SaveChanges();
                            }
                        }
                        else
                        {
                            status = facebook.InsertToFeed(proposal, CurrentLoggedInUser, System.Web.HttpContext.Current.Request.UrlReferrer.ToString());
                            if (!(status is int))
                            {
                                proposal.FacebookPostId = status.id;
                                _db.FacebookPostSchedule.Remove(post);
                                _db.SaveChanges();
                            }
                        }
                        //do something
                    }
                    else
                    {
                        _db.FacebookPostSchedule.Remove(post);
                        _db.SaveChanges();
                    }
                }
              //
            }
            if (Comments_lists.Count > 0)
            {


                foreach (var comment in Comments_lists)
                {
                    int status = facebook.postCommentToPost(comment.FacebookPostId, comment.Body, comment.UserId);
                    if (status == 0)
                    {
                        _db.FacebookCommentSchedule.Remove(comment);
                        _db.SaveChanges();
                    }


                }
            }


        }
    }
}