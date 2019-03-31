using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Capston2DataAccess;
namespace Capston2.Controllers
{
    public class UserInfoController : ApiController
    {
        public IEnumerable<USER_TABLE> Get()
        {
            using(UserDataEntities entities = new UserDataEntities())
            {
                return entities.USER_TABLE.ToList();
            }
        }
    }
}
