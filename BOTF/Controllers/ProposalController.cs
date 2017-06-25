using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BOTF.Models;
using BOTF.Infrastructure;
using WebMatrix.WebData;
using System.Net;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using BOTF.Filters;
/*This controller takes care of all the proposal related things Proposal Detail Page*/
namespace BOTF.Controllers
{
    public class ProposalController : Controller
    {
        LastFM lastfm = new LastFM(Settings.Settings.LastFMKey, "http://ws.audioscrobbler.com/2.0/");
        ContextDb _db = new ContextDb();
        //
        // GET: /Proposal/
        //checking here to see if the person has a referring code
        public ActionResult Index(int Id, string ccf = "")
        {
            if (ccf != "")
            {
                Chance chance = _db.Chance.FirstOrDefault(c => c.Code == ccf);
                if (chance != null)
                {
                    User user = chance.user;
                    if (user != null)
                    {
                        Session["ChanceReferrer"] = user.Id;
                        _db.Chance.Remove(chance); //expire code
                        _db.SaveChanges();
                    }
                   
                }
            }
            Proposal proposal = _db.Proposal.FirstOrDefault(c=>c.Id == Id);
            if (proposal != null)
            {
                return View(proposal);
            }
            return RedirectToAction("Index","Home");
        }

        public JsonResult GetSimilarProposals(string Artistname)
        {
            List<string> ArtistNames = lastfm.GetSimilarArtists(Artistname);
            List<Proposal> output = new List<Proposal>();
           
                foreach (var item in ArtistNames)
                {

                    var proposal = _db.Proposal.Where(c => c.Artist.Contains(item)).FirstOrDefault();
                    if (proposal != null)
                    {
                        output.Add(proposal);
                    }
                }

                return Json(output, JsonRequestBehavior.AllowGet);
   
        }
        /*A function that creates codes for referrals*/
        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /*A function that creates a new Code and registers in the database*/
        [InitializeSimpleMembership]
        public JsonResult GenerateCode() {
         
            string Key = "";
            if (OAuthWebSecurity.IsAuthenticatedWithOAuth)
            {
             
                Chance chance = new Chance();
                bool state = false;
                while (state == false)//keep on checking until a unique key found
                {
                    Key = RandomString(15);
                    chance = _db.Chance.FirstOrDefault(c => c.Code == Key);
                    if (chance == null)
                    {
                       
                       
                      _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Chance.Add(new Chance { Code = Key, DateCreated = DateTime.Now, });;
                   
                       _db.SaveChanges();
                     
                        
                   
                        state = true;
                    }
                }
             


            
            }
        
         
         
           return Json(Key, JsonRequestBehavior.AllowGet);
        }


   

    }
}
