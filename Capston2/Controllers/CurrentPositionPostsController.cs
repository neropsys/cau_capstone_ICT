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
using System.Web.SessionState;
using System.Xml.Linq;
using System.Web;
using Capston2DataAccess;
namespace Capston2.Controllers
{
    public class CurrentPositionPostsController : ApiController
    {
        class ResponseFormat
        {
            public bool ret { get; set; }
            public string reason { get; set; }
        }

        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        static List<string> buildingNumbers;
        public class PostModel
        {
            public string nickname { get; set; }
            public string text { get; set; }
            public string title { get; set; }
            public int buildingNumber { get; set; }
        }
        public class GetModel
        {
            public string buildingNumber;
        }
        [HttpGet]
        public string Get([FromUri]GetModel value)
        {
            using (BuildingPostsModel entities = new BuildingPostsModel())
            {
                var tableValue = entities.BUILDING_POSTS;
                var selectedTable = tableValue.ToList();
                selectedTable = selectedTable.FindAll(x => x.buildingnumber.Equals(value.buildingNumber));

                string json = JsonConvert.SerializeObject(selectedTable.ToArray());
                return json;
            }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PostModel value)
        {

            if (buildingNumbers == null)
            {
                buildingNumbers = new List<string>();

                var xml = XDocument.Load(HttpContext.Current.Server.MapPath("/Resources/Buildings.xml"));
                var buildingList = xml.Descendants("Building").Attributes("Number").Select(x => x.Value);

                foreach (string building in buildingList)
                {
                    buildingNumbers.Add(building);
                }
            }
            //TODO:check credential
            //TODO:check session state
            if (buildingNumbers.Contains(value.buildingNumber.ToString()))
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_post_to_location_board", connection))
                    {
                        try
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@nickname", SqlDbType.VarChar, 50).Value = value.nickname;
                            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = value.text;
                            cmd.Parameters.Add("@title", SqlDbType.VarChar, 50).Value = value.title;
                            cmd.Parameters.Add("@buildingnumber", SqlDbType.Int).Value = value.buildingNumber;
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
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
