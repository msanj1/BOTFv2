using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BOTF.Settings
{
    public static class Settings
    {
        public static string TwitterConsumerKey { get; set; }
        public static string TwitterConsumerSecret { get; set; }
        public static string FacebookAppId { get; set; }
        public static string FacebookAppSecret { get; set; }
        public static string TwitterCallbackURL { get; set; }
        public static string FacebookCallbackURL { get; set; }
        public static string LastFMKey { get; set; }
    }
}