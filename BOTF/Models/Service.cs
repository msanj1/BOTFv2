using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BOTF.Models
{
    public class Service
    {
        public virtual int Id { get; set; }
    
        public virtual string Provider { get; set; }
        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
    }
}