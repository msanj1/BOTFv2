using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BOTF.Models
{
    public class Pick
    {
        public virtual int Id { get; set; }
        public virtual int ProposalId { get; set; }
        public virtual DateTime DateAdded { get; set; }
    }
}