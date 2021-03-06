﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using System.Web;
using Capston2DataAccess;
namespace Capston2.Controllers
{
    public class ResidencePostsController : ApiController
    {
        class ResponseFormat
        {
            public bool ret { get; set; }
            public string reason { get; set; }
        }

        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();




        public class ReplyModel
        {
            public string nickName { get; set; }
            public string userId { get; set; }
            public int replyIndex { get; set; }
            public string text { get; set; }
        }
        public class ReplyFormat
        {
            public string userId { get; set; }
            public string text { get; set; }
        }
        public class ReplyDeleteFormat
        {
            public int replyId;
        }
        [HttpPost]
        [Route("api/posts/replies/delete/{postId}")]
        public HttpResponseMessage DeleteReply(int postId, [FromBody]ReplyDeleteFormat format)
        {
            using (capston_databaseEntities userInfoTable = new capston_databaseEntities())
            {
                try
                {
                    using (capston_postreplyconn replyEntities = new capston_postreplyconn())
                    {
                        var replyTable = replyEntities.POST_REPLIES;
                        var targetReply = replyTable.FirstOrDefault(x => x.postid == postId && x.replyindex == format.replyId);
                        if (targetReply != null)
                        {
                            replyTable.Remove(targetReply);

                            replyEntities.SaveChanges();
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage(HttpStatusCode.BadRequest);
                        }
                    }
                }
                catch (Exception e)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    responseMessage.Content = new StringContent(e.Message,
                               System.Text.Encoding.UTF8,
                               "application/json");
                    return responseMessage;
                }
            }
        }
        [HttpPost]
        [Route("api/posts/replies/{postId}")]
        public HttpResponseMessage PostReply(int postId, [FromBody] ReplyFormat format)
        {
            using (capston_databaseEntities userInfoTable = new capston_databaseEntities())
            {
                var userInfo = userInfoTable.USER_INFO.FirstOrDefault(x => x.id == format.userId);
                if (userInfo != null)
                {
                    try
                    {
                        using (capston_postreplyconn replyEntities = new capston_postreplyconn())
                        {
                            var replyTable = replyEntities.POST_REPLIES;
                            var lastReply = replyTable.Where(x => x.postid == postId).OrderByDescending(x => x.replyindex).FirstOrDefault();
                            replyTable.Add(new POST_REPLIES
                            {
                                nickname = userInfo.nickname,
                                postid = postId,
                                replyindex = lastReply != null ? lastReply.replyindex + 1 : 0,
                                text = format.text,
                                userid = format.userId,
                                time = DateTime.Now.AddHours(9)
                            });

                            replyEntities.SaveChanges();
                        }
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    catch (Exception e)
                    {
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        responseMessage.Content = new StringContent(e.Message,
                                   System.Text.Encoding.UTF8,
                                   "application/json");
                        return responseMessage;
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }
        [HttpGet]
        [Route("api/posts/replies/{postId}")]
        public HttpResponseMessage GetReplies(int postId)
        {
            //format
            /*
             {
             "postid":44,
             "nickname":"장혁재닉",
             "userid":"chang_hyuk_jae",
             "replyindex":0,
             "text":"ㅎㅇㅎㅇ",
             "time":"2019-05-29T05:13:35.297"
             }
             */
            using (capston_postreplyconn replyEntities = new capston_postreplyconn())
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                var replyList = replyEntities.POST_REPLIES.Where(x => x.postid == postId).OrderBy(x => x.replyindex).ToArray();
                string replyListJson = JsonConvert.SerializeObject(replyList);
                responseMessage.Content = new StringContent(replyListJson,
                                   System.Text.Encoding.UTF8,
                                   "application/json");
                return responseMessage;
            }
        }
        public class GetModel
        {
            public string residenceName { get; set; }
        }
        [HttpGet]
        public HttpResponseMessage Get([FromUri]GetModel value)
        {
            using (ResidenceEntities entities = new ResidenceEntities())
            {
                var tableValue = entities.RESIDENCE_POSTS;
                var selectedTable = tableValue.ToList();
                selectedTable = selectedTable.FindAll(x => x.residence.Equals(value.residenceName));

                string json = JsonConvert.SerializeObject(selectedTable.ToArray());

                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                responseMessage.Content = new StringContent(json,
                           System.Text.Encoding.UTF8,
                           "application/json");
                return responseMessage;
            }
        }

        public class PostModel
        {
            public string residence { get; set; }
            public string nickname { get; set; }
            public string text { get; set; }
            public string title { get; set; }
            public int type { get; set; }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PostModel value)
        {
            //TODO:check credential
            //TODO:check session state

            if (value.type == 1)//bopParty type = 1
            {
                using (ResidenceEntities postTableEntity = new ResidenceEntities())
                {
                    var getPost = postTableEntity.RESIDENCE_POSTS.FirstOrDefault(x => x.nickname == value.nickname && x.type == 1);
                    if (getPost != null)
                    {
                        var diff = DateTime.Now.AddHours(9).Subtract(getPost.date);
                        if (diff.Minutes < 2)//2 minute
                        {

                            ResponseFormat responseFormat = new ResponseFormat();
                            responseFormat.ret = true;
                            responseFormat.reason = "DUPLICATE REQUEST";
                            string jsonContent = JsonConvert.SerializeObject(responseFormat);


                            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = new StringContent(jsonContent,
                                System.Text.Encoding.UTF8,
                                "application/json");
                            return responseMessage;
                        }
                        else
                        {
                            postTableEntity.RESIDENCE_POSTS.Remove(getPost);
                            postTableEntity.SaveChanges();
                        }

                    }
                }
            }
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_post_to_residence_board", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@residence", SqlDbType.VarChar, 50).Value = value.residence;
                        cmd.Parameters.Add("@nickname", SqlDbType.VarChar, 50).Value = value.nickname;
                        cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = value.text;
                        cmd.Parameters.Add("@title", SqlDbType.VarChar, 50).Value = value.title;
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = value.type;
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();


                        ResponseFormat responseFormat = new ResponseFormat();
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        responseFormat.ret = true;
                        if (retValue == -1)
                        {
                            responseFormat.ret = false;
                            responseFormat.reason = "UNKNOWN_NICKNAME";
                        }

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

        public class ModifyModel
        {
            public string title { get; set; }
            public string text { get; set; }
            public int id { get; set; }
        }

        [HttpPost]
        [Route("api/posts/edit")]
        public HttpResponseMessage EditPost([FromBody] ModifyModel value)
        {
            using (ResidenceEntities postTableEntity = new ResidenceEntities())
            {
                var getPost = postTableEntity.RESIDENCE_POSTS.FirstOrDefault(x => x.id == value.id);
                if (getPost != null)
                {
                    getPost.title = value.title;
                    getPost.text = value.text;
                    postTableEntity.SaveChanges();
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    ResponseFormat responseFormat = new ResponseFormat();
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFormat.ret = false;
                    responseFormat.reason = "POST_ID_NOT_FOUND";
                    return responseMessage;
                }

            }
        }
        public class DeleteModel
        {
            public string  userId { get; set; }
            public int postId { get; set; }
        }
        [HttpPost]
        [Route("api/posts/delete")]
        public HttpResponseMessage DeletePost([FromBody] DeleteModel value)
        {
            using (ResidenceEntities postTableEntity = new ResidenceEntities())
            {
                var getPost = postTableEntity.RESIDENCE_POSTS.FirstOrDefault(x => x.id == value.postId);
                if (getPost != null)
                {
                    postTableEntity.RESIDENCE_POSTS.Remove(getPost);
                    postTableEntity.SaveChanges();
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    ResponseFormat responseFormat = new ResponseFormat();
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFormat.ret = false;
                    responseFormat.reason = "POST_ID_NOT_FOUND";
                    return responseMessage;
                }

            }

        }
    }
}
