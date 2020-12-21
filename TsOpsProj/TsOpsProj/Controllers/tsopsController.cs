using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TsOpsProj.Models;

namespace TsOpsProj.Controllers
{
    public class tsopsController : ApiController
    {

        public PIPointOperations PI;

        public tsopsController()
        {
            PI = new PIPointOperations("<MYPISERVER>");
        }


        // GET: api/tsops
        public IEnumerable<string> Get()
        {

            return this.PI.GetPIPoints();
        }

        // GET: api/tsops/sinusoid
        //GET: api/tsops/id=sinusoid
        public PIValueModel Get(string id)
        {
            return PI.GetPIValue(id);
        }

        // POST: api/tsops
        public IHttpActionResult Post([FromBody]PIPointModel value)
        {
            string tagName = value.PointName;
            bool pointCreated = PI.CreatePIPoint(value);
            if (pointCreated)
            {
                //PIValueModel newPoint = Get(value.PointName);
                return Content(HttpStatusCode.OK, Get(tagName));
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, "Point already exists");
            }
        }

        // PUT: api/tsops/sinusoid
        public IHttpActionResult Put(string id, [FromBody] PIPointModel tagAttrib)
        {
            try 
            {
                PI.AddPIValue(id, tagAttrib.datetime,
                                            tagAttrib.value);
                return Content(HttpStatusCode.OK, Get(id));
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // DELETE: api/tsops/sinusoid
        public void Delete(int id)
        {
            // NOT IMPLEMENTED
        }
    }
}
