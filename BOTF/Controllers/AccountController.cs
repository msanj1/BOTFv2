using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using BOTF.Filters;
using BOTF.Models;
using BOTF.Infrastructure;
using Facebook;
using System.IO;
using System.Configuration;
using TweetSharp;

namespace BOTF.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //tweetsharp setup
        TwitterApi twitter = new TwitterApi(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, Settings.Settings.TwitterCallbackURL);
        //
        // GET: /Account/Login
        //some variable for holding Facebook's token and name
        public static string FacebookToken { get; set; }
        public static string Provider { get; set; }
      
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login


       

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

     /*This function takes care of managing code referrals via Facebook and Twitter share buttons*/
        private  void CheckChanceState(int id= 0) {
           ContextDb _db = new ContextDb();
           if (Session["ChanceReferrer"] != null && Session["state"] == null) //checking if a session variable for the current visitor exists
            {
            
                int Id = Convert.ToInt32(Session["ChanceReferrer"].ToString());
                Models.User referrer = _db.User.FirstOrDefault(c => c.Id == Id); //get the refferrer's data
                if (referrer != null)
                {
                    referrer.RemainingVotes++;
                    _db.SaveChanges();
                    Session.Remove("ChanceReferrer");
                    Session["state"] = true; //setting this to true later so that the second time this function is called the new registered user's remaining is increased
                }
            }
           else if (id > 0 && Session["state"] != null)//true
            {
                _db.User.FirstOrDefault(c => c.UserId == id).RemainingVotes++ ;
                _db.SaveChanges();
                Session.Remove("state");
            }
            

        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    CheckChanceState();
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    string MongoDirectory = "";
                    if (model.Image != null)
                    {
                        var filename = Path.GetFileName((model.Image.FileName));
                        var path = Path.Combine(Server.MapPath("~/Images"), filename);
                        MongoDirectory = String.Format("{0}{1}", @"\Images\", filename);
//C:\Users\mohsen\Desktop\BOTF\BOTF\Content\images\no-icon.jpg
                        model.Image.SaveAs(path); //save image in images
                    }
                    else
                    {
                        MongoDirectory = Url.Content("~/Content/images/no-icon.jpg"); //if not image is chosen use a default one
                    }
                 
                    ContextDb _db = new ContextDb();
                    UsersContext db = new UsersContext();
                    //set up a new user
                    int Id = db.UserProfiles.FirstOrDefault(c=>c.UserName == model.UserName).UserId;
                    _db.User.Add(new User { UserId = Id, Email = model.Email, Name = model.Name, RemainingProposals = 100, RemainingVotes = 100, Image = MongoDirectory });
                    _db.SaveChanges();
                    CheckChanceState(Id);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            ContextDb _db = new ContextDb();
            Service service =  _db.User.FirstOrDefault(c => c.UserId == WebSecurity.CurrentUserId).Services.FirstOrDefault(c => c.Provider == provider); //remove the user's services 
            _db.Service.Remove(service);
            _db.SaveChanges();
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
       
            
          
             ContextDb _db = new ContextDb();
          

             User user=  _db.User.FirstOrDefault(c=>c.UserId == WebSecurity.CurrentUserId); //the manage page
             if (user != null)
             {
                 ViewBag.user = user;
             }
             else
             {
                 ViewBag.user = new User { Name = "admin", Email = "No Emails set", Image = "", RemainingProposals = 0, RemainingVotes = 0 }; //if logged in as admin set some default properties
             }
          
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {

            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin


        /*This is where facebook and tweetsharp apis branch off*/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            if (provider == "twitter")
            {
              
                return RedirectToAction("TwitterAuth"); 
            }
            else 
            {
                return RedirectToAction("FBLogin");
            }
           

          
        }

        [AllowAnonymous]
        public ActionResult TwitterAuth()
        {
            
            var uri = twitter.GetRequestURI(); //get request token and uri

          
            return new RedirectResult(uri.ToString()); //authenticate
        }
        [AllowAnonymous]
        public ActionResult TwitterCallback(string oauth_token, string oauth_verifier)
        {
            if (oauth_token == null)
            {
                return RedirectToAction("Index","Home");
            }
            var requestToken = new OAuthRequestToken { Token = oauth_token };
            string Token, TokenSecret;
            Token = ""; TokenSecret = "";
            twitter.Authenticate(requestToken,oauth_verifier,ref Token,ref TokenSecret); 
            TwitterUser user = twitter.userInfo(); //get user's info

            if (OAuthWebSecurity.Login("twitter", user.Id.ToString(), createPersistentCookie: false))//if has account then login
            {


                return RedirectToAction("Index","Home"); //go back to the home page
            }
            if (User.Identity.IsAuthenticated)//used when adding an external loggin
            {
                // If the current user is logged in add the new account
                DatabaseCallsApi _api = new DatabaseCallsApi();

                _api.AddOrUpdateService(WebSecurity.CurrentUserId, "twitter", Token, TokenSecret); //set the service for the user

                OAuthWebSecurity.CreateOrUpdateAccount("twitter", user.Id.ToString(), WebSecurity.CurrentUserName);

                return RedirectToAction("Index", "Home"); //go back to the home page
            }
            else
            {
                // User is new, ask for their desired membership name

                CheckChanceState();

                string loginData = OAuthWebSecurity.SerializeProviderUserId("twitter", user.Id.ToString());
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData("twitter").DisplayName;
                if (Token != "" && TokenSecret != "")
                {
                    Session["AccessToken"] = Token;
                    Session["AccessTokenSecret"] = TokenSecret;

                }
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = user.ScreenName, ExternalLoginData = loginData });
            }
            




           
          
        }

        //
        // GET: /Account/ExternalLoginCallback
         [AllowAnonymous]
        public ActionResult FBLogin()
        {
            // Build the Return URI form the Request Url
            var redirectUri = new UriBuilder(Request.Url);
            redirectUri.Path = Url.Action("FBAuth", "Account");

            var client = new FacebookClient();
             /*redirection URI to grant authentication and authorization*/
            var uri = client.GetLoginUrl(new
            {
                client_id = Settings.Settings.FacebookAppId,
                redirect_uri = Settings.Settings.FacebookCallbackURL,
                scope = "email,user_friends,read_stream,publish_actions", //permissions
            });

            return Redirect(uri.ToString());

        }

        [AllowAnonymous]
         public ActionResult FBAuth(string returnUrl)
        {
            var client = new FacebookClient();
            var oauthResult = client.ParseOAuthCallbackUrl(Request.Url);

            // Build the Return URI form the Request Url
            var redirectUri = new UriBuilder(Request.Url);
            redirectUri.Path = Url.Action("FbAuth","Account");
            dynamic result = client.Get("/oauth/access_token", new //get the facebook token
            {
                client_id = Settings.Settings.FacebookAppId,
                redirect_uri = Settings.Settings.FacebookCallbackURL,
                client_secret = Settings.Settings.FacebookAppSecret,
                code = oauthResult.Code,
            });
           

            if (result == null)
            {

                return RedirectToAction("ExternalLoginFailure");
            }
            string accessToken = result.access_token;
            string token = FacebookAPI.GetLongtermFbToken(accessToken);  //get a 2month token
            FacebookToken = accessToken;
            Provider = "facebook";
            dynamic me = client.Get("/me", //get some basic user info
                 new
                 {
                     fields = "first_name,last_name,email",
                     access_token = accessToken
                 });
           
            if (OAuthWebSecurity.Login("facebook",me.id, createPersistentCookie: false))
            {
                string username = OAuthWebSecurity.GetUserName("facebook", me.id);
                int userId = WebSecurity.GetUserId(username);
                FacebookScheduler scheduler = new FacebookScheduler(); //run any undone task
                scheduler.RunScheduler(token, userId);

                return RedirectToLocal(returnUrl);
            }
            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                DatabaseCallsApi _api = new DatabaseCallsApi();

                var username = OAuthWebSecurity.GetUserName("facebook", me.id);
             
                
                _api.AddOrUpdateService(WebSecurity.CurrentUserId, "facebook", token);
                OAuthWebSecurity.CreateOrUpdateAccount("facebook",me.id, WebSecurity.CurrentUserName.ToString());

                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name

                CheckChanceState();

                string loginData = OAuthWebSecurity.SerializeProviderUserId("facebook", me.id);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData("facebook").DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = me.email, ExternalLoginData = loginData, Email = me.email });
            }

        }

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {


 
            var url =  Url.Action("ExternalLoginCallback");



          
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl })); //facebook 2hrs token

           

          
            FacebookToken = result.ExtraData["accesstoken"];
            Provider = result.Provider;
            if (Provider == "facebook")
            {
                string token = FacebookAPI.GetLongtermFbToken(FacebookToken);
                FacebookToken = token;
            }

            if (!result.IsSuccessful)
            {

                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                string username = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId);
                int userId = WebSecurity.GetUserId(username);
                FacebookScheduler scheduler = new FacebookScheduler();
                scheduler.RunScheduler(FacebookToken, userId);
                
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                DatabaseCallsApi _api = new DatabaseCallsApi();
                
                var username = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId);
                int user_id = WebSecurity.GetUserId(username);
                _api.AddOrUpdateService(user_id, result.Provider, FacebookToken);
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
               
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                
           
              
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                       
                       
                        // Insert name into the profile table
                       UserProfile profile = db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();
                  
                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                       //need to check to see if it is facebook or twitter
                        if (provider == "twitter")
                        {
                     

                            if (Session["AccessToken"] != null && Session["AccessTokenSecret"] != null) //used to distinugish between facebook and twitter regisration
	                        {
                                //"9kCMAgidv1NzN8TfMVgZw", "RimlGsenvejdoRlw0NSazYzXJBO6olF2IBMJcw11Uc"
                                //creating new tweetsharp service
                                TwitterService service = new TwitterService(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, Session["AccessToken"].ToString(), Session["AccessTokenSecret"].ToString());
                                TwitterUser me = service.VerifyCredentials();
                                ContextDb _db = new ContextDb();
                                Models.User temp = new Models.User { UserId = profile.UserId, Email = model.Email, Image = me.ProfileImageUrl, Name =me.Name, RemainingProposals = 100, RemainingVotes = 100 };
                                temp = _db.User.Add(temp);
                                _db.SaveChanges();
                                CheckChanceState(temp.UserId);
                                DatabaseCallsApi _api = new DatabaseCallsApi();
                                _api.AddOrUpdateService(temp.UserId, "twitter", Session["AccessToken"].ToString(), Session["AccessTokenSecret"].ToString());
                                Session.Remove("AccessToken");
                                Session.Remove("AccessTokenSecret");
                            }
                           
                        }
                        else
                        {
                            //setting new facebook service
                            FacebookAPI facebook = new FacebookAPI(FacebookToken);
                            dynamic facebookData = facebook.GetUsersData();
                            if (facebookData != null)
                            {

                                ContextDb _db = new ContextDb();

                                Models.User temp = new Models.User { UserId = profile.UserId, Email = facebookData.email.ToString(), Image = facebookData.picture["data"]["url"].ToString(), Name = facebookData.name.ToString(), RemainingProposals = 1, RemainingVotes = 3 };
                                temp = _db.User.Add(temp);
                                _db.SaveChanges();
                                CheckChanceState(temp.UserId);
                                DatabaseCallsApi _api = new DatabaseCallsApi();
                                _api.AddOrUpdateService(temp.UserId, Provider, FacebookToken);

                            }
                        }
                       
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }


        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {

                var parameters = new Dictionary<string, object>();
                parameters["state"] = ReturnUrl;
                parameters["scope"] = "email";
                ReturnUrl += "&scope='email'";
                
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
