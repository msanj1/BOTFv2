using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace BOTF.Domain
{

    public class Proposal
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string Artist { get; set; }
        public virtual string Venue { get; set; }
        public virtual int Votes { get; set; }
        public virtual string Image { get; set; }
        public virtual string Biography { get; set; }
        public virtual string Genre { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual int ProposedBy { get; set; }
    }


}
