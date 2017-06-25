using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using BOTF.Models;
using BOTF.Filters;
using BOTF.ModelView;
using BOTF.Infrastructure;
/*This controller is used for the Profile Page*/
namespace BOTF.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/
        ContextDb _db = new ContextDb();
        UsersContext _usr = new UsersContext();
        public ActionResult Index(int Id)
        {
            Models.User user = _db.User.FirstOrDefault(c => c.UserId == Id);

            if (user != null)
            {
                string name = _usr.UserProfiles.FirstOrDefault(c => c.UserId == user.UserId).UserName;

                ViewBag.username = name;
                return View(user);
            }
            return RedirectToAction("Index", "Home");
        }

    }
}
