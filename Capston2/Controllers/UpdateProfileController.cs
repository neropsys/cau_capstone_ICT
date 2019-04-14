using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Capston2.Controllers
{
    public class UpdateProfileController : ApiController
    {
        public class ProfileModel
        {
            public string residence { get; set; }
        }
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ProfileModel value)
        {

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
