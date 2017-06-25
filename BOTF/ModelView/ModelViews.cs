using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Configuration;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
namespace BOTF.ModelView
{
    public class User
    {
        [Required]
        public  int Id { get; set; }

        public  string Email { get; set; }
        public  string Name { get; set; }
        public  int RemainingVotes { get; set; }
        public  int RemainingProposals { get; set; }
        public  string Image { get; set; }
    
    }

    public class ViewProposal
    {
        [Required]
    
        public string artist { get; set; }
        [Required]
   
        public string venue { get; set; }
    }

    public class ViewVoteHistory
    {
        public int UserId { get; set; }
        public int ProposalId { get; set; }
        public string Username { get; set; }
        public string UserImage { get; set; }
        public string Artist { get; set; }
        public string Venue { get; set; }
        public int Vote { get; set; } 
    }

    [DataContract]
    public class ViewComment
    {
      
     [Required]
     [DataMember(IsRequired = true)]
        public int Id { get; set; }

      [Required]
      public string Body { get; set; }

      public bool isFacebook { get; set; }
      public bool Artist { get; set; }  
    }

    public class Viewdelete
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public bool isFacebook { get; set; }
    }

    public class ViewCommentHistory
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public string DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public string UserId { get; set; }
        public bool isFacebook { get; set; }
        public bool Artist { get; set; }
    }

    public class ViewTopFiveVoters
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Rank { get; set; }
    }
    /*Proposal and Vote History*/
    public class ViewAllHistories
    {
        public int VoteId { get; set; } //ID of vote history
        public int ProposalId { get; set; } //ID of proposal
        public string Artist { get; set; }
        public string Venue { get; set; }
        public string Image { get; set; }
    }
    [DataContract]
    public class ViewUnvote
    {

        [DataMember(IsRequired=true)]
        public int Id { get; set; }
        [DataMember(IsRequired = true)]
        public int ProposalId { get; set; }
    }
     [DataContract]
    public class FacebookProposal
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }
        
    }

     public class ViewProposals
     {
         public int Id { get; set; }
         public string Artist { get; set; }
         public string Venue { get; set; }
         public int Votes { get; set; }
         public string Image { get; set; }
         public string Genre { get; set; }
         public int ProposedBy { get; set; }
         public string Owner { get; set; }
         public int PickId { get; set; }
         public DateTime DateCreated { get; set; }
     }

      [DataContract]
     public class ViewPicks
     {
         [DataMember(IsRequired = true)]
         public int Id { get; set; }
     }

      public class ViewSliderRegistration
      {
         
          public string Id { get; set; }
          [Required]
          public string Title { get; set; }
          [Required]
          public string Description { get; set; }
          [Required]
          public HttpPostedFileBase Image { get; set; }
          public string Link { get; set; }
      }

      public class ViewDeleteSliderContent
      {
          public int Id { get; set; }
          public bool List { get; set; }
      }
    



}