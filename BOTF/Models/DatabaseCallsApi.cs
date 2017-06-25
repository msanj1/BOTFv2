using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BOTF.Infrastructure;
namespace BOTF.Models
{

    public class DatabaseCallsApi
    {
         ContextDb _db;
        public DatabaseCallsApi()
        {
            _db = new ContextDb();
        }

        public User GetUserInfo(int Id)
        {
            User user = _db.User.FirstOrDefault(c=>c.UserId == Id);
            if (user != null)
            {
                  return user;
            }
            return new User();
        }
        /*explained before on account. add a facebook or twitter service to a user*/
        public void AddOrUpdateService(int UserId, string Provider, string Token, string TokenSecret = "")
        {
            User user = _db.User.FirstOrDefault(c => c.UserId == UserId);
         
            if (user != null)
            {
                Service service = user.Services.FirstOrDefault(c => c.Provider == Provider);
                if (service == null)
                {
                    user.Services.Add(new Service { Provider = Provider, Token = Token,TokenSecret=TokenSecret });
                    _db.SaveChanges();
                }
                else
                {
                    service.Provider = Provider;
                    service.Token = Token;
                    service.TokenSecret = TokenSecret;
                    _db.SaveChanges();
                }
            }
            
        }
        /*add a facebook post id to database*/
        public void AddOrUpdateFacebookPost( int ProposalId, int UserId,string PostId)
        {
            Service service = _db.User.FirstOrDefault(c => c.UserId == UserId).Services.FirstOrDefault(c=>c.Provider == "facebook");
            Proposal propsal = _db.Proposal.FirstOrDefault(c=>c.Id == ProposalId);

          
                if (service != null && propsal != null)
                {
                    propsal.FacebookPostId = PostId;
                    _db.SaveChanges();
                }

        }
        /*add a facebook post id to database*/
        public void AddOrUpdateFacebookArtistPost(int ProposalId, int UserId, string PostId)
        {
            Service service = _db.User.FirstOrDefault(c => c.UserId == UserId).Services.FirstOrDefault(c => c.Provider == "facebook");
            Proposal propsal = _db.Proposal.FirstOrDefault(c => c.Id == ProposalId);


            if (service != null && propsal != null)
            {
                propsal.FacebookPostIdArtist = PostId;
                _db.SaveChanges();
            }

        }




    }
}