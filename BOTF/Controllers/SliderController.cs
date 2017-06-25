using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BOTF.Models;
using BOTF.Infrastructure;
using BOTF.ModelView;
using System.IO;
/*controller for the slider*/
namespace BOTF.Controllers
{
    public class SliderController : ApiController
    {
        ContextDb _db = new ContextDb();
        // GET api/slider
        //get slider contents
        public List<Slider> Get()
        {
            List<Slider> output = new List<Slider>();
            var slider_contents =  _db.SliderList.ToList();
            if (slider_contents.Count > 0)
            {
                foreach (var content in slider_contents)
                {
                    Slider slider = _db.Slider.FirstOrDefault(c=>c.Id == content.SliderId);
                    if (slider != null)
                    {
                        output.Add(new Slider { Id = content.Id, Description = slider.Description, Image = slider.Image, Link = slider.Link, Title = slider.Title });
                    }
                }
            }
            return output;
        }

        // GET api/slider/5
        public string Get(int id)
        {
            return "value";
        }
        //post a new slider content
        // POST api/slider
        public HttpResponseMessage Post(int Id)
        {
            Slider slider =  _db.Slider.FirstOrDefault(c=>c.Id == Id);
            if (slider != null)
            {
                //do something
                _db.SliderList.Add(new SliderList{SliderId=Id});
                _db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            else
            {
               return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // PUT api/slider/5
        public void Put(int id, [FromBody]string value)
        {

        }
        //delete a slider content
        // DELETE api/slider/5
        public HttpResponseMessage Delete(ViewDeleteSliderContent model)
        {

            if (model.List == false)
            {
                var listContent = _db.SliderList.FirstOrDefault(c => c.Id == model.Id);
                if (listContent != null)
                {
                    _db.SliderList.Remove(listContent);
                    _db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                //var listContent = _db.SliderList.FirstOrDefault(c => c.Id == model.Id);
                var list = _db.Slider.FirstOrDefault(c=>c.Id==model.Id);
                if (list != null)
                {
                   
                   var slider_list= _db.SliderList.Where(c => c.SliderId == list.Id);

                   
                   File.Delete(System.Web.HttpContext.Current.Server.MapPath("~" + list.Image));
           
                    _db.Slider.Remove(list);
                 
            
                    
                    foreach (var item in slider_list)//delete all traces of a list content
                    {

                          _db.SliderList.Remove(item);
                    }
                    _db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }

            
            
        }
    }
}
