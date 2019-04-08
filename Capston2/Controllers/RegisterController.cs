using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Capston2.Controllers
{
    public class RegisterController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        public class RegisterModel
        {
            public string id { get; set; }
            public string password { get; set; }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post(RegisterModel value)
        {
            PasswordHash hash = new PasswordHash(value.password);
            var hashedValue = hash.ToArray();
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_add_account", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 50).Value = value.id;
                        cmd.Parameters.Add("@hash", SqlDbType.Binary, PasswordHash.HashSize + PasswordHash.SaltSize).Value = hashedValue;
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();

                        if (retValue == -1)
                        {
                            //duplicate id
                            return new HttpResponseMessage(HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                            responseMessage.Content = new ObjectContent<RegisterModel>(value, new JsonMediaTypeFormatter(), "application/json");

                            return responseMessage;
                        }
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
