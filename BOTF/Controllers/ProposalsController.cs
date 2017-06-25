using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.ModelView;
using BOTF.Models;
using BOTF.Infrastructure;
using System.Threading;
using WebMatrix.WebData;
using BOTF.Filters;
/*This controller takes care of the propopsals functionality*/
namespace BOTF.Controllers
{
   [InitializeSimpleMembership]
    public class ProposalsController : ApiController
    {
        ContextDb _db = new ContextDb();
        LastFM lastfm = new LastFM(Settings.Settings.LastFMKey, "http://ws.audioscrobbler.com/2.0/");

        // GET api/proposal

   /*get the users ranked with respect to a proposal*/
       [Authorize]
        public IEnumerable<ViewTopFiveVoters> Get(int Id)
        {
            List<ViewTopFiveVoters> voters = new List<ViewTopFiveVoters>();
            var output = from p in _db.VoteHistory
                         where p.ProposalId == Id
                         group p by p.UserId into g
                         
                         select new { UserId = g.Key, NoVote = g.Count() };


            var output2 = output.OrderByDescending(c => c.NoVote).Take(5).ToList();
           int Rank = 1;
            foreach (var item in output2)
            {
                Models.User user = _db.User.FirstOrDefault(c=>c.UserId == item.UserId);

                voters.Add(new ViewTopFiveVoters { Id = item.UserId, Email = user.Email, Image = user.Image, Name = user.Name, Rank = Rank++ });
            }
            var allvoters = output.OrderByDescending(c=>c.NoVote).ToList();
            int CurentRank = allvoters.AsQueryable().Select((user, index) => new { user.UserId, index }).Where(user => user.UserId == WebSecurity.CurrentUserId).Select(c=>c.index).FirstOrDefault();//finding ranking number
            CurentRank++;
            ViewTopFiveVoters check = voters.SingleOrDefault(c => c.Id == WebSecurity.CurrentUserId);

            if (CurentRank >= 5 && check == null)
            {
                voters.RemoveAt(4);
                Models.User me = _db.User.FirstOrDefault(c=>c.UserId == WebSecurity.CurrentUserId);
                voters.Insert(4, new ViewTopFiveVoters { Id = WebSecurity.CurrentUserId, Rank = CurentRank, Email = me.Email, Image = me.Image, Name = me.Name });
            }
        


            return voters;
        }
       /*This function is used to display the top12 and the rest part of the homepage*/
        // GET api/proposal/5
       public List<ViewProposals> Get([FromUri]int count, [FromUri]int skip, [FromUri]string Filter, [FromUri]bool State)
        {
          
          List<ViewProposals> output = new List<ViewProposals>();
          List<Proposal> proposals;
          if (Filter != null)
          {
              if (State == true)
              {
                  
                  proposals = _db.Proposal.Select(c => c).Where(c => c.Genre == Filter).OrderBy(c => c.Id).Skip(skip).Take(count).ToList();//filter
              }
              else
              {
                  proposals = _db.Proposal.Select(c => c).Where(c => c.Genre == Filter).OrderByDescending(c => c.Votes).Skip(skip).Take(count).ToList(); //top12

              }
          }
          else
          {
              if (State == true)
              {
                   proposals = _db.Proposal.Select(c => c).OrderBy(c=>c.Id).Skip(skip).Take(count).ToList();
              }
              else
              {
                  proposals = _db.Proposal.Select(c => c).OrderByDescending(c => c.Votes).Skip(skip).Take(count).ToList();

              }
          }
             
            foreach (var proposal in proposals)
            {
                Models.User user = _db.User.FirstOrDefault(c=>c.UserId == proposal.ProposedBy);
                output.Add(new ViewProposals { Id=proposal.Id, Artist=proposal.Artist, Genre=proposal.Genre, Image=proposal.Image, Venue=proposal.Venue, Votes=proposal.Votes, Owner=user.Name, ProposedBy=proposal.ProposedBy});
            }
            return output;
        
        }
       /*This function is used to post a new proposal*/
        // POST api/proposal
        [Authorize]
        public HttpResponseMessage PostProposal(ViewProposal proposal)
        {
            if (ModelState.IsValid)
            {
                
                 try
                 {
                     
                     dynamic ArtistData = lastfm.GetArtistInfo(proposal.artist); //grab data from LastFM
                     Models.User u = _db.User.FirstOrDefault(c=>c.UserId == WebSecurity.CurrentUserId);
                     if (u.RemainingProposals <= 0)
                     {
                             return  Request.CreateResponse(HttpStatusCode.Forbidden); //check if user can propose a new proposal or not
                     }

                  
                     
                     Proposal temp = new Proposal { Artist = ArtistData.ArtistName, Biography = ArtistData.ArtistBio, Genre = ArtistData.MusicGenre, Image = ArtistData.PictureURL, Venue = proposal.venue, Votes = 0, ProposedBy = WebSecurity.CurrentUserId };
                     var DatabaseIntegrityCHK = _db.Proposal.FirstOrDefault(c => c.Venue == temp.Venue && c.Artist == temp.Artist);
                     if (DatabaseIntegrityCHK != null) //check if a similar proposal exists
                     {
                         return Request.CreateResponse(HttpStatusCode.Conflict);
                     }
                     u.Proposals.Add(temp);
                     
                     //_db.Proposal.Add(temp);
                     
                     _db.SaveChanges();
                      Models.User user =  _db.User.First(c=> c.UserId == WebSecurity.CurrentUserId);
                     user.RemainingProposals--; //reduce remaining proposal

                     //_db.VoteHistory.Add(new Domain.VoteHistory { ProposalId =temp.Id, UserId=WebSecurity.CurrentUserId });
                     _db.SaveChanges();
                 
                     //Proposal list = new Proposal { Id = propId, id = propId.ToString(), UserId = (ObjectId)userId, Artist = artist, Venue = venue, Votes = 0, Image = image, UsersName = userName, Biography = ArtistData.ArtistBio, Genre = ArtistData.MusicGenre};
                     var response = new HttpResponseMessage(HttpStatusCode.Created);
                     response.Headers.Location = new Uri(Request.RequestUri, "/api/FacebookProposal?Id=" + temp.Id + "&ArtistPost=" + false); //header for background post to feed and tweeter page
                  
                     //response.Headers = temp;

                     
                     return response;
                 }
                 catch (Exception)
                 {
                     return Request.CreateResponse(HttpStatusCode.BadRequest);
                    
                 }
               
       
            }else
	        {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
	        }
           
        }

       

       /*This function takes care of the vote button*/
        // PUT api/proposal/5
       [Authorize]
        public HttpResponseMessage Put(int Id)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            Models.User user = _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId);
            if (user.RemainingVotes <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden); //check remaining votes
            }
                Proposal proposal = _db.Proposal.FirstOrDefault(c=>c.Id == Id);

            proposal.Votes++;
       
            _db.SaveChanges();
            _db.VoteHistory.Add(new VoteHistory { ProposalId = proposal.Id, UserId = WebSecurity.CurrentUserId });
           
            user.RemainingVotes--;
            _db.SaveChanges();
            int currentVotes = proposal.Votes;

            if (currentVotes != 0 && ((currentVotes % 10) == 0))
            {
                response.Headers.Location = new Uri(Request.RequestUri, "/api/FacebookProposal?Id=" + proposal.Id + "&ArtistPost=" + true); //artist post if a vote reaches 10
            }
            return response;

        }

        // DELETE api/proposal/5
        public void Delete(int id)
        {
        }
    }
}
