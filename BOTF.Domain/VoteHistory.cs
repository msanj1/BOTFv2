using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace BOTF.Domain
{
   public class VoteHistory
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual int ProposalId { get; set; }
    }
}
