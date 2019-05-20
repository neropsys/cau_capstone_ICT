using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Capston2DataAccess;
using Newtonsoft.Json;

namespace Capston2.Controllers
{

    public class FriendController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();

        [JsonObject(MemberSerialization.OptIn)]
        public class FriendResponseFormat
        {
            [JsonProperty]
            public object nick { get; set; }
            [JsonProperty]
            public object id { get; set; }

        }
        //get friend list
        [HttpGet]
        [Route("api/{userId}/friends")]
        public string GetFriend(string userId)
        {
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                var userDataTable = userDataEntities.USER_INFO.ToList();
                using (FriendTableEntities entities = new FriendTableEntities())
                {
                    var tableValue = entities.FRIEND;
                    var selectedTable = tableValue.ToList();
                    selectedTable = selectedTable.FindAll(x =>
                    (x.friend1.Equals(userId) || x.friend2.Equals(userId)) &&
                    x.type != null
                    );
                    List<FriendResponseFormat> ret = new List<FriendResponseFormat>();
                    foreach (var friendData in selectedTable)
                    {
                        string targetFriendID = friendData.friend1.Equals(userId) ? friendData.friend2 : friendData.friend1;

                        if (userDataTable.Exists(x => x.id.Equals(targetFriendID)))
                        {
                            var friendNickname = userDataTable.Find(x => x.id.Equals(targetFriendID));
                            ret.Add(new FriendResponseFormat { nick = friendNickname.nickname, id = targetFriendID });
                        }
                    }

                    var serializer = new JsonSerializer();
                    var stringWriter = new StringWriter();
                    using (var writer = new JsonTextWriter(stringWriter))
                    {
                        writer.QuoteName = false;
                        serializer.Serialize(writer, ret);
                    }
                    var json = stringWriter.ToString();
                    return json;
                }
            }
        }

        //get pending requests
        [HttpGet]
        [Route("api/{userId}/friend/requests")]
        public string GetRequests(string userId)
        {
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                var userDataTable = userDataEntities.USER_INFO.ToList();

                using (FriendTableEntities entities = new FriendTableEntities())
                {
                    var tableValue = entities.FRIEND;
                    var selectedTable = tableValue.ToList();
                    selectedTable = selectedTable.FindAll(x => x.type.HasValue == false && x.friend2.Equals(userId));
                    List<string> ret = new List<string>();
                    foreach (var friendData in selectedTable)
                    {
                        if (userDataTable.Exists(x => x.id.Equals(friendData.friend1)))
                        {
                            var senderInfo = userDataTable.Find(x => x.id.Equals(friendData.friend1));
                            ret.Add(senderInfo.nickname);
                        }
                    }
                    string json = JsonConvert.SerializeObject(ret);
                    return json;
                }
            }
        }

        public class UpdateFormat
        {
            public string senderId { get; set; }
            public string receiverNick { get; set; }
            public int type { get; set; }
        }

        public class AddFriendFormat
        {
            public string senderId { get; set; }
            public string receiverNick { get; set; }
            public string friendType { get; set; }
        }
        class ResponseFormat
        {
            public bool ret { get; set; }
            public string reason { get; set; }
        }
        //add nfc friend
        //         [HttpPost]
        //         [Route("api/{userId}/friend/nfc")]
        //         public HttpResponseMessage AcceptFriend(string userId, [FromBody]AcceptFormat response)
        //         {
        //             return new HttpResponseMessage();
        //         }


        //accept/decline/delete friend
        [HttpPost]
        [Route("api/{userId}/friend/update")]
        public HttpResponseMessage UpdateFriend(string userId, [FromBody]UpdateFormat response)
        {
            //1:온라인 친구, 2:NFC태그한 친구, 0:친삭
            if (response.type < 0 || response.type > 2)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                var userDataTable = userDataEntities.USER_INFO.ToList();
                if(userDataTable.Exists(x => x.nickname.Equals(response.receiverNick)) == false)
                {
                    ResponseFormat responseFormat = new ResponseFormat();
                    responseFormat.ret = false;
                    responseFormat.reason = "RECV_ID_NOT_FOUND";
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                    string jsonContent = JsonConvert.SerializeObject(responseFormat);
                    responseMessage.Content = new StringContent(jsonContent,
                         System.Text.Encoding.UTF8,
                         "application/json");
                    return responseMessage;
                }
            
                string receiverID = userDataTable.Find(x => x.nickname.Equals(response.receiverNick)).id;
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_update_friend", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@friend1Id", SqlDbType.VarChar, 50).Value = response.senderId;
                        cmd.Parameters.Add("@friend2Id", SqlDbType.VarChar).Value = receiverID;
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = response.type;
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();


                        if (retValue == -1)
                        {
                            return new HttpResponseMessage(HttpStatusCode.BadRequest);
                            //sender id not found. return
                        }
                        ResponseFormat responseFormat = new ResponseFormat();
                        switch (retValue)
                        {
                            case 0://successful
                                responseFormat.ret = true;
                                //TODO: push notification to receiver
                                break;
                            case -2://receiver id not found. 
                                responseFormat.ret = false;
                                responseFormat.reason = "RECV_ID_NOT_FOUND";
                                break;
                        }
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                        string jsonContent = JsonConvert.SerializeObject(responseFormat);
                        responseMessage.Content = new StringContent(jsonContent,
                             System.Text.Encoding.UTF8,
                             "application/json");
                        return responseMessage;
                    }
                }

            }

        }

        //send friend request
        [HttpPost]
        [Route("api/{userId}/friend/add")]
        public HttpResponseMessage AddFriend(string userId, [FromBody]AddFriendFormat value)
        {
            ResponseFormat responseFormat = new ResponseFormat();
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                var userDataTable = userDataEntities.USER_INFO.ToList();
                if (userDataTable.Exists(x => x.nickname.Equals(value.receiverNick)) == false)
                {

                    responseFormat.ret = false;
                    responseFormat.reason = "RECV_ID_NOT_FOUND";
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                    string jsonContent = JsonConvert.SerializeObject(responseFormat);
                    responseMessage.Content = new StringContent(jsonContent,
                         System.Text.Encoding.UTF8,
                         "application/json");
                    return responseMessage;
                }
                string receiverID = userDataTable.Find(x => x.nickname.Equals(value.receiverNick)).id;

                using(FriendTableEntities friendTableEntities = new FriendTableEntities())
                {
                    var friendTable = friendTableEntities.FRIEND.ToList();

                    if(friendTable.Exists(x => x.friend1.Equals(userId) && x.friend2.Equals(receiverID)))
                    {
                        responseFormat.ret = false;
                        responseFormat.reason = "DUPLICATE_REQUEST";
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                        string jsonContent = JsonConvert.SerializeObject(responseFormat);
                        responseMessage.Content = new StringContent(jsonContent,
                             System.Text.Encoding.UTF8,
                             "application/json");
                        return responseMessage;
                    }
                }

                using (SqlConnection connection = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_add_friend", connection))
                    {
                        try
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@friend1Id", SqlDbType.VarChar, 50).Value = value.senderId;
                            cmd.Parameters.Add("@friend2Id", SqlDbType.VarChar).Value = receiverID;
                            if (value.friendType == "NFC")
                            {
                                cmd.Parameters.Add("@type", SqlDbType.Int).Value = 2;
                            }
                            var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                            cmd.Parameters["@result"].Direction = ParameterDirection.Output;

                            connection.Open();
                            cmd.ExecuteNonQuery();
                            int retValue = (int)returnParameter.Value;
                            connection.Close();

                            if (retValue == -1)
                            {
                                return new HttpResponseMessage(HttpStatusCode.BadRequest);
                                //sender id not found. return
                            }
                            switch (retValue)
                            {
                                case 0://successful
                                    responseFormat.ret = true;
                                    //TODO: push notification to receiver
                                    break;
                                case -2://receiver id not found. 
                                    responseFormat.ret = false;
                                    responseFormat.reason = "RECV_ID_NOT_FOUND";
                                    break;
                            }
                            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                            string jsonContent = JsonConvert.SerializeObject(responseFormat);
                            responseMessage.Content = new StringContent(jsonContent,
                                 System.Text.Encoding.UTF8,
                                 "application/json");
                            return responseMessage;

                        }
                        catch (Exception ex)
                        {
                            string exceptionValue = ex.Message;
                            connection.Close();
                            var retMsg = new HttpResponseMessage(HttpStatusCode.OK);
                            retMsg.Content = new StringContent(ex.Message);
                            return retMsg;
                        }
                    }
                }
            }
        }
    }
}
