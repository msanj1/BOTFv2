using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BOTF.Models
{
    public class Slider
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual string Image { get; set; }
        public virtual string Link { get; set; }
        public virtual DateTime DateCreated { get; set; }
    }

    public class SliderList
    {
        public int Id { get; set; }
        public int SliderId { get; set; }
    }
}