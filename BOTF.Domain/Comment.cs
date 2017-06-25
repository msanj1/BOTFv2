using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace BOTF.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }//referencing Proposal ID
        public string Body { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; } //ID of User

    }
}
