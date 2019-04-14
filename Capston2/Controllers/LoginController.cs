using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Capston2DataAccess;
namespace Capston2.Controllers
{
    public class LoginController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }
        public class LoginModel
        {
            public string id { get; set; }
            public string password { get; set; }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post(LoginModel value)
        {
            string id = value.id;
            string password = value.password;

            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_get_hash", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 50).Value = id;

                        var hashParam = cmd.Parameters.Add("@hash", SqlDbType.Binary, PasswordHash.HashSize + PasswordHash.SaltSize);
                        hashParam.Direction = ParameterDirection.Output;

                        var retParam = cmd.Parameters.Add("@result", SqlDbType.Int);
                        retParam.Direction = ParameterDirection.Output;

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)retParam.Value;
                        if (retValue == -1)
                        {
                            connection.Close();
                            //id or pswd is wrong
                            return new HttpResponseMessage(HttpStatusCode.BadRequest);
                        }
                        byte[] hash = (byte[])hashParam.Value;
                        connection.Close();
                        PasswordHash pswdHash = new PasswordHash(hash);
                        if (pswdHash.Verify(password))
                        {
                            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            using(capston_databaseEntities entities = new capston_databaseEntities())
                            {
                                var tableValue = entities.USER_INFO.ToList();
                                var userInfo = tableValue.Find(x => x.id.Equals(id));
                                if(userInfo != null)
                                {
                                    responseMessage.Content = new ObjectContent<USER_INFO>(userInfo, new JsonMediaTypeFormatter(), "application/json");
                                }
                            }
                            
                            return responseMessage;
                        }
                        else
                        {
                            return new HttpResponseMessage(HttpStatusCode.BadRequest); 
                        }


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

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
