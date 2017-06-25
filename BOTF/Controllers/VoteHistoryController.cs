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
/*All the vote/proposal histories are done here*/
namespace BOTF.Controllers
{
     [InitializeSimpleMembership]
    public class VoteHistoryController : ApiController
    {
         ContextDb _db = new ContextDb();
        // GET api/votehistory
         //get a list of vote history
        public IEnumerable<ViewVoteHistory> Get()
        {
            List<ViewVoteHistory> votehistory = new List<ViewVoteHistory>();
            var history = _db.VoteHistory.Select(c => c).OrderByDescending(c => c.Id).Take(10).ToList();
            foreach (var item in history)
            {
                Proposal proposal = _db.Proposal.FirstOrDefault(c=>c.Id == item.ProposalId);
                Models.User user = _db.User.FirstOrDefault(c=>c.UserId == item.UserId);
                votehistory.Add(new ViewVoteHistory {UserId = user.UserId, UserImage=user.Image, Username=user.Name, Artist=proposal.Artist, Venue=proposal.Venue, Vote=proposal.Votes, ProposalId=proposal.Id });

            }
          

            return votehistory;
        }

        // GET api/votehistory/5
         //return either proposals or vote history for a user
        public IEnumerable<ViewAllHistories> Get(int id, bool voteOrproposal = false)
        {
            List<ViewAllHistories> votehistory = new List<ViewAllHistories>();

            if (voteOrproposal == false)//vote history
            {
                var vhistory =  _db.VoteHistory.Where(c=>c.UserId == id).ToList();
                foreach (var item in vhistory)
                {
        
                    var proposal = _db.Proposal.FirstOrDefault(c=>c.Id == item.ProposalId);
                    if (proposal != null)
                    {
                        votehistory.Add(new ViewAllHistories { VoteId = item.Id, Artist = proposal.Artist, Venue = proposal.Venue, Image = proposal.Image, ProposalId=proposal.Id});

                    }
                }
            }
            else //proposal history
            {
                var phistory = _db.Proposal.Where(c => c.ProposedBy == id).ToList();
                foreach (var item in phistory)
                {
                    votehistory.Add(new ViewAllHistories { Artist = item.Artist, Venue = item.Venue, Image = item.Image, ProposalId=item.Id });
                }
            }



          

            return votehistory;
           
        }

        // POST api/votehistory
        public void Post([FromBody]string value)
        {
        }

        // PUT api/votehistory/5
        public void Put(int id, [FromBody]string value)
        {
        }
         //unvote a proposal
        // DELETE api/votehistory/5
        public HttpResponseMessage Delete(ViewUnvote unvote)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                VoteHistory history = _db.VoteHistory.Find(unvote.Id);
                if (history != null)
                {
                    _db.VoteHistory.Remove(history); //remove vote history
                    Models.User user = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId);
                    user.RemainingVotes++;
                    Models.Proposal proposal = _db.Proposal.FirstOrDefault(c=>c.Id == unvote.ProposalId);
                    proposal.Votes--;
                    _db.SaveChanges();
                    return response;
                }
                return response = Request.CreateResponse(HttpStatusCode.NotModified);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

           


        }
    }
}
