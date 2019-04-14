using Newtonsoft.Json;
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


        public class PostModel
        {
            public string residence { get; set; }
            public string nickname { get; set; }
            public string text { get; set; }
            public string title { get; set; }
        }
        public class GetModel
        {
            public string residenceName { get; set; }
        }
        [HttpGet]
        public HttpResponseMessage Get([FromUri]GetModel value)
        {
            using (Entities entities = new Entities())
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
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PostModel value)
        {
            //TODO:check credential
            //TODO:check session state
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
                        exceptionValue += "zz";
                        connection.Close();
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);

                    }
                }
            }

        }
    }
}
