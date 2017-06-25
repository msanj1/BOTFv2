using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.Models;
using BOTF.Infrastructure;
using BOTF.ModelView;
/*controller for the staff picks section*/
namespace BOTF.Controllers
{
    public class StaffPicksController : ApiController
    {
        ContextDb _db = new ContextDb();
        // GET api/staffpicks
        //function that returns all the staff picks
        public List<ViewProposals> Get([FromUri]string Filter = null)
        {
            List<ViewProposals> proposals = new List<ViewProposals>();

             List<Pick> picks =  _db.StaffPick.ToList();
             foreach (var pick in picks)
             {
                 Proposal proposal;
                 if (Filter != null)
                 {
                    
                     proposal = _db.Proposal.Where(c => c.Genre == Filter).FirstOrDefault(c => c.Id == pick.ProposalId);
                 }
                 else
                 {
                     proposal = _db.Proposal.FirstOrDefault(c => c.Id == pick.ProposalId);
                 }

                
                 if (proposal != null)
                 {
                     Models.User user = _db.User.FirstOrDefault(c => c.UserId == proposal.ProposedBy);
                     if (user != null)
                     {
                         proposals.Add(new ViewProposals { Artist=proposal.Artist, Genre=proposal.Genre, Image=proposal.Image, Owner=user.Name, ProposedBy=proposal.ProposedBy, Venue=proposal.Venue, Votes=proposal.Votes, Id=proposal.Id, PickId=pick.Id, DateCreated=pick.DateAdded });
                     }
                 }
             }
             return proposals;
        }

        // GET api/staffpicks/5
        //same function but with a count variable
        public List<ViewProposals> Get([FromUri]int count, [FromUri]int skip, [FromUri]string Filter)
        {
            List<ViewProposals> output = new List<ViewProposals>();
            var picks =  _db.StaffPick.OrderByDescending(c => c.DateAdded).Skip(skip).Take(count).ToList();
            
            foreach (var pick in picks)
            {
                Proposal proposal =  _db.Proposal.FirstOrDefault(c => c.Id == pick.ProposalId);
                if (proposal != null)
                {
                    Models.User user = _db.User.FirstOrDefault(c=>c.Id == proposal.ProposedBy);
                    if (user != null)
                    {
                        output.Add(new ViewProposals { Artist = proposal.Artist, Genre = proposal.Genre, Image = proposal.Image, Id = proposal.Id, ProposedBy = proposal.ProposedBy, Venue = proposal.Venue, Votes = proposal.Votes, Owner=user.Name });
                    }
                   
                }
                
            }
            return output;
        }

        // POST api/staffpicks
        public void Post([FromBody]string value)
        {

        }

        // PUT api/staffpicks/5
        //add a new staff pick to the list
        public HttpResponseMessage Put(ViewPicks Pick)
        {
            if (ModelState.IsValid)
            {
                Proposal proposal = _db.Proposal.FirstOrDefault(c => c.Id == Pick.Id);
                if (proposal != null)
                {
                    _db.StaffPick.Add(new Pick { ProposalId=proposal.Id, DateAdded=DateTime.Now});
                    _db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/staffpicks/5
        //delete a staff pick from the list
        public HttpResponseMessage Delete(ViewPicks Pick)
        {
            if (ModelState.IsValid)
            {
                Pick pick =  _db.StaffPick.FirstOrDefault(c=>c.Id == Pick.Id);
                if (pick !=null)
                {
                    _db.StaffPick.Remove(pick);
                    _db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
