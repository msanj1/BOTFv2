using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOTF.Domain
{
    public class User
    {
        
        public virtual int Id { get; set; }
        
        public virtual string Email { get; set; }
        public virtual string Name { get; set; }
        public virtual int RemainingVotes { get; set; }
        public virtual int RemainingProposals { get; set; }
        public virtual string Image { get; set; }
        public virtual ICollection<Proposal> Proposals { get; set; }
    }
}
