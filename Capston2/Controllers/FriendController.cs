using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        //get friend list
        [HttpGet]
        [Route("api/{userId}/friends")]
        public string GetFriend(string userId)
        {
            using (FriendTableEntities entities = new FriendTableEntities())
            {
                var tableValue = entities.FRIEND;
                var selectedTable = tableValue.ToList();
                selectedTable = selectedTable.FindAll(x => 
                (x.friend1.Equals(userId) || x.friend2.Equals(userId)) &&
                x.type != null
                );
                List<string> ret = new List<string>();
                foreach (var friendData in selectedTable)
                {
                    ret.Add(friendData.friend1.Equals(userId) ? friendData.friend2 : friendData.friend1);
                }
                string json = JsonConvert.SerializeObject(selectedTable.ToArray());
                return json;
            }
        }

        //get pending requests
        [HttpGet]
        [Route("api/{userId}/friend/requests")]
        public string GetRequests(string userId)
        {

            using (FriendTableEntities entities = new FriendTableEntities())
            {
                var tableValue = entities.FRIEND;
                var selectedTable = tableValue.ToList();
                selectedTable = selectedTable.FindAll(x => x.type.HasValue == false && x.friend2.Equals(userId));
                List<string> ret = new List<string>();
                foreach(var friendData in selectedTable)
                {
                    ret.Add(friendData.friend1);
                }
                string json = JsonConvert.SerializeObject(ret);
                return json;
            }
        }

        public class UpdateFormat
        {
            public string senderId { get; set; }
            public string receiverId { get; set; }
            public int type { get; set; }
        }

        public class AddFriendFormat
        {
            public string senderId { get; set; }
            public string receiverId { get; set; }
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
            
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_update_friend", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@friend1Id", SqlDbType.VarChar, 50).Value = response.senderId;
                    cmd.Parameters.Add("@friend2Id", SqlDbType.VarChar).Value = response.receiverId;
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

        //send friend request
        [HttpPost]
        [Route("api/{userId}/friend/add")]
        public HttpResponseMessage AddFriend(string userId, [FromBody]AddFriendFormat value)
        {
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_add_friend", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@friend1Id", SqlDbType.VarChar, 50).Value = value.senderId;
                        cmd.Parameters.Add("@friend2Id", SqlDbType.VarChar).Value = value.receiverId;
                        if(value.friendType == "NFC")
                        {
                            cmd.Parameters.Add("@type", SqlDbType.Int).Value = 2;
                        }
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();

                        if(retValue == -1)
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
