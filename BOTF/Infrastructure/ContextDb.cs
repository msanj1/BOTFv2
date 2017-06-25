using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using BOTF.Models;
namespace BOTF.Infrastructure
{
    public class ContextDb:DbContext
    {
     

        public ContextDb()//use default connection
            : base("DefaultConnection")
        {
           
        }
        public DbSet<Pick> StaffPick { get; set; }
        public DbSet<Proposal> Proposal { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<VoteHistory> VoteHistory { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<FacebookPostSchedule> FacebookPostSchedule { get; set; }
        public DbSet<FacebookCommentSchedule> FacebookCommentSchedule { get; set; }
        public DbSet<Slider> Slider { get; set; }
        public DbSet<SliderList> SliderList { get; set; }
        public DbSet<Chance> Chance { get; set; }
    }
}