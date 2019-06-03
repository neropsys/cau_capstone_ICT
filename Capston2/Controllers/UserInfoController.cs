using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Capston2DataAccess;
using Newtonsoft.Json;

namespace Capston2.Controllers
{
    public class UserInfoController : ApiController
    {

        public class ResponseFormat
        {
            public string dateofbirth { get; set; }
            public string residence { get; set; }
            public string major { get; set; }
            public string hobby { get; set; }
        }
        public class RequestFormat
        {
            public bool dateofbirth { get; set; }
            public bool residence { get; set; }
            public bool major { get; set; }
            public bool hobby { get; set; }
        }
        [HttpPost]
        [Route("api/UserInfo/{userId}")]
        public HttpResponseMessage SetUserInfo(RequestFormat value, string userId)
        {
            using (capston_databaseEntities userDataEntites = new capston_databaseEntities())
            {
                var userInfo = userDataEntites.USER_INFO.FirstOrDefault(x => x.id == userId);

                if (userInfo != null)
                {
                    using (capston_database_PrivacyEntities privacyEntities = new capston_database_PrivacyEntities())
                    {
                        var privacySetting = privacyEntities.USER_INFO_PRIVACY.SingleOrDefault(x => x.id == userId);

                        if (privacySetting != null)
                        {
                            privacySetting.dateofbirth = value.dateofbirth ? 1 : (byte?)null;
                            privacySetting.hobby = value.hobby ? 1 : (byte?)null;
                            privacySetting.major = value.major ? 1 : (byte?)null;
                            privacySetting.residence = value.residence ? 1 : (byte?)null;
                        }
                        else
                        {

                            privacyEntities.USER_INFO_PRIVACY.Add(new USER_INFO_PRIVACY
                            {
                                dateofbirth = value.dateofbirth ? 1 : (byte?)null,
                                hobby = value.hobby ? 1 : (byte?)null,
                                id = userId,
                                major = value.major ? 1 : (byte?)null,
                                residence = value.residence ? 1 : (byte?)null
                            });
                        }
                        privacyEntities.SaveChanges();
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
                else
                {
                    var retMsg = new HttpResponseMessage(HttpStatusCode.OK);
                    retMsg.Content = new StringContent("Userinfo not found");
                    return retMsg;
                }
            }
        }
        public class RequestModel
        {
            public string id { get; set; }
        }
        [HttpGet]
        [Route("api/UserInfo/{userNick}")]
        public HttpResponseMessage GetUserInfo(string userNick)
        {
            using(capston_databaseEntities userDataEntites = new capston_databaseEntities())
            {
                var userInfo = userDataEntites.USER_INFO.FirstOrDefault(x => x.nickname == userNick);
                
                if(userInfo != null)
                {
                    using (capston_database_PrivacyEntities privacyEntities = new capston_database_PrivacyEntities())
                    {
                        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                        ResponseFormat retValue = new ResponseFormat();
                        var privacySetting = privacyEntities.USER_INFO_PRIVACY.FirstOrDefault(x => x.id == userInfo.id);

                        if(privacySetting != null)
                        {
                            //1:public. 2:friend only. 0 or null:hidden. only 1 or null for now
                           if(privacySetting.dateofbirth.HasValue)
                            {
                                retValue.dateofbirth = userInfo.dateofbirth;
                            }
                            if (privacySetting.hobby.HasValue)
                            {
                                retValue.hobby = userInfo.hobby;
                            }
                            if (privacySetting.major.HasValue)
                            {
                                retValue.major = userInfo.major;
                            }
                            if (privacySetting.residence.HasValue)
                            {
                                retValue.residence = userInfo.residence;
                            }
                        }

                        string jsonContent = JsonConvert.SerializeObject(retValue);

                        response.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                        return response;
                    }
                }
                else
                {
                    var retMsg = new HttpResponseMessage(HttpStatusCode.OK);
                    retMsg.Content = new StringContent("Userinfo not found");
                    return retMsg;
                }
            }
        }
    }
}
