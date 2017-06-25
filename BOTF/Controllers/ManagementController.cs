using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BOTF.Infrastructure;
using BOTF.Filters;
using BOTF.ModelView;
using BOTF.Models;
using System.IO;
/*This controller takes care of the Dashboard section of the website*/
namespace BOTF.Controllers
{
    [InitializeSimpleMembership]
    //[Authorize(Roles="Admin")]
    public class ManagementController : Controller
    {
        //
        // GET: /Management/
        //Staff Picks
        ContextDb _db = new ContextDb();
        /*This function is also used to filter the proposals that are available*/
        public ActionResult Index(string Query="")
        {

       
            var proposals =  _db.Proposal.Where(c => c.Artist.Contains(Query) || c.Genre.Contains(Query) || c.Venue.Contains(Query)).ToList();
            if (proposals.Count == 0)
            {
               
               return View(_db.Proposal.ToList());
            }
            else
            {
                return View(proposals);
            }
          
        }
        //Slider
        /*This funciton return the slider's contents*/
        public ActionResult SliderContent(int Id = 0)
        {
            if (Id > 0)
            {
                Slider content = _db.Slider.FirstOrDefault(c=>c.Id == Id);
                ViewBag.Prefil = content;
                ViewBag.fil = true;
            }
            var model = _db.Slider;
         
            return View(model);

        }
        /*This function takes care of registering new slider contents*/
        [HttpPost]
        public ActionResult RegisterSliderContent(ViewSliderRegistration model)
        {
        
            if (ModelState.IsValid)
            {
                int Id = Convert.ToInt32(model.Id);
                if (Id > 0)
                {
                   Slider content = _db.Slider.FirstOrDefault(c=>c.Id == Id);
                   if (content != null)
                   {
                       System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~" + content.Image)); //if an old file exists then delete it(used for editing)
                       string Directory = "";
                       if (model.Image.ContentLength > 0)
                       {
                           var filename = Path.GetFileName((model.Image.FileName));
                           var path = Path.Combine(Server.MapPath("~/Images"), filename);
                           Directory = String.Format("{0}{1}", @"\Images\", filename);

                           model.Image.SaveAs(path); //save new image
                       }
                       content.Description = model.Description;
                       content.DateCreated = DateTime.Now;
                       content.Image = Directory;
                       content.Link = model.Link;
                       content.Title = model.Title;
                  
                       _db.SaveChanges();


                      
                   }
                }
                else//new slider content
                {
                    string Directory = "";
                    if (model.Image.ContentLength > 0)
                    {
                        var filename = Path.GetFileName((model.Image.FileName));
                        var path = Path.Combine(Server.MapPath("~/Images"), filename);
                        Directory = String.Format("{0}{1}", @"\Images\", filename);

                        model.Image.SaveAs(path); //save image in images
                    }
                    _db.Slider.Add(new Slider { Title = model.Title, Description = model.Description, Image = Directory, Link = model.Link, DateCreated = DateTime.Now });
                    _db.SaveChanges();
                }

                
                return RedirectToAction("SliderContent");
            }
            else
            {
         
                return RedirectToAction("SliderContent");
            }
            

        }

    }
}
