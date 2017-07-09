using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using BOTF.Models;

namespace BOTF
{
    public class userService
    {
        public string Token { get; set; }
        public string Provider { get; set; }
    }

    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            
         

            /*Tweeter Configuration*/
            Settings.Settings.TwitterConsumerKey = "LKLeLT9sw38CdVTnWb8IIw";
            Settings.Settings.TwitterConsumerSecret = "uKr3F9GKjNkBGuE7DCbZvR4xcRcsPPU1ibLpPg0U";

            /*Facebook Configuration*/
            Settings.Settings.FacebookAppId = "499187306800509";
            Settings.Settings.FacebookAppSecret = "a1dfe19ee3d9f05f48f36b88522c0eda";
            

            /*Facebook/Twitter callback Configuration*/
            Settings.Settings.TwitterCallbackURL = "http://botf.azurewebsites.net/Account/TwitterCallback";
            Settings.Settings.FacebookCallbackURL = "http://msartists.azurewebsites.net/Account/FBAuth";

            /*LastFM Configuration*/
            Settings.Settings.LastFMKey = "5a1d769dafee0a5d5cd83aef7bfa54c7";
           
//            pplication name	Battle of the Artists
//API key	5a1d769dafee0a5d5cd83aef7bfa54c7
//Shared secret	7ae798d8bfe1d886c505390e2d85339b
//Registered to	mohsensanjari

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: Settings.Settings.TwitterConsumerKey,
                consumerSecret: Settings.Settings.TwitterConsumerSecret);


            OAuthWebSecurity.RegisterFacebookClient(
                appId: Settings.Settings.FacebookAppId,
                appSecret: Settings.Settings.FacebookAppSecret
               
             );
             
           

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
