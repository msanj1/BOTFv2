using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetSharp;
namespace BOTF.Models
{
   
    public class TwitterApi
    {
      
        TwitterService twitter;
        string callbackURL;
        public TwitterApi(string ConsumerKey, string ConsumerSecret, string callback) {

            twitter = new TwitterService(ConsumerKey, ConsumerSecret);
            callbackURL = callback;
        }

        public Uri GetRequestURI() {
            OAuthRequestToken requestToken = twitter.GetRequestToken(callbackURL);
            Uri uri = twitter.GetAuthorizationUri(requestToken);
            return uri;
        }

        public bool Authenticate(OAuthRequestToken requestToken, string oauth_verifier, ref string Token, ref string tokenSecret)
        {

            OAuthAccessToken accessToken= twitter.GetAccessToken(requestToken, oauth_verifier);
            if (accessToken != null)
            {
                twitter.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

                Token = accessToken.Token;
                tokenSecret = accessToken.TokenSecret;
                return true;
            }
            return false;
        }

        public TwitterUser userInfo() {
           return twitter.VerifyCredentials();
        }


    }
}