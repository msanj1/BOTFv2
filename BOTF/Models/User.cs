using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOTF.Models
{
    public class User
    {
        
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual string Email { get; set; }
        public virtual string Name { get; set; }
        public virtual int RemainingVotes { get; set; }
        public virtual int RemainingProposals { get; set; }
        public virtual string Image { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        
        public virtual ICollection<Proposal> Proposals { get; set; }
        public virtual ICollection<Chance> Chance { get; set; }
    }

    public class Chance
    {
        public virtual int Id { get; set; }
        public virtual string Code { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual User user { get; set; }
    }
}
