using Capston2DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace Capston2.Controllers
{
    public class CalenderController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();


        public class PostCalenderFormat
        {
            public string title { get; set; }
            public string content { get; set; }
            public string date { get; set; }
        }

        [HttpPost]
        [Route("api/{userId}/calender")]
        public HttpResponseMessage PostCalender(string userId, [FromBody]PostCalenderFormat format)
        {

            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {

                var senderInfo = userDataEntities.USER_INFO.Where(x => x.id.Equals(userId)).FirstOrDefault();
                if(senderInfo.id == "")
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                string userNick = senderInfo.nickname;

                using(CalenderEntities calenderEntites = new CalenderEntities())
                {
                    calenderEntites.CALENDER.Add(new CALENDER
                    {
                        userId = userId,
                        content = format.content,
                        title = format.title,//2013-05-01
                        date = DateTime.Parse(format.date)
                    });
                    calenderEntites.SaveChanges();
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
               // using calende
                // using 

            }
        }

        public class CalenderListFormat
        {
            public string nickname { get; set; }
            public string title { get; set; }
            public string content { get; set; }
            public DateTime date { get; set; }
        }
        [HttpGet]
        [Route("api/{userId}/calender/{year}/{month}")]
        public string GetCalender(string userId, int year, int month)
        {
            DateTime selectedDate = new DateTime(year, month, 1);
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                var senderInfo = userDataEntities.USER_INFO.Where(x => x.id.Equals(userId)).FirstOrDefault();
                if (senderInfo.id == "")
                {
                    return "wrong id";
                }

                using(FriendTableEntities friendTableEntities = new FriendTableEntities())
                {
                    var friends = friendTableEntities.FRIEND.Where(x => x.friend1.Equals(userId) || x.friend2.Equals(userId)).ToArray();

                    Dictionary<string, string> friendIdNickList = new Dictionary<string, string>();//id, nickdictionary

                    foreach(var friend in friends)
                    {
                        if(friend.friend1 == userId)
                        {
                            var friendInfo = userDataEntities.USER_INFO.Where(x => x.id.Equals(friend.friend2)).First();
                            if(friendInfo.id != "")
                            {
                                friendIdNickList[friend.friend2] = friendInfo.nickname;
                            }
                        }
                        else
                        {
                            var friendInfo = userDataEntities.USER_INFO.Where(x => x.id.Equals(friend.friend1)).First();
                            if (friendInfo.id != "")
                            {
                                friendIdNickList[friend.friend1] = friendInfo.nickname;
                            }
                        }
                    }
                    if(friendIdNickList.Count == 0)
                    {
                        return "";
                    }

                    using(CalenderEntities calenderEntities = new CalenderEntities())
                    {
                        List<CalenderListFormat> ret = new List<CalenderListFormat>();
                        foreach(var friend in friendIdNickList)
                        {

                            var query = from calender in calenderEntities.CALENDER
                                        where (calender.userId == friend.Key && calender.date.Year == year && calender.date.Month == month)
                                        select new CalenderListFormat
                                        {
                                            content = calender.content,
                                            date = calender.date,
                                            nickname = friend.Value,
                                            title = calender.title
                                        };
                            ret.AddRange(query.ToList());
                        }
                        return JsonConvert.SerializeObject(ret);
                    }


                }
                
            }
        }
    }
}
