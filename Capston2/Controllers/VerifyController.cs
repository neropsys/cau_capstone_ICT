using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Capston2.Controllers
{
    //TODO: need throttling
    public class VerifyController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        public class VerifyModel
        {
            public string valueName { get; set; }
            public string value { get; set; }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post(VerifyModel verifyModel)
        {
            if (verifyModel.valueName != "id" && verifyModel.valueName != "nickname")
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_verify_value", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 50).Value = verifyModel.valueName;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@value", SqlDbType.VarChar, 50).Value = verifyModel.value;

                        var retParam = cmd.Parameters.Add("@result", SqlDbType.Int);
                        retParam.Direction = ParameterDirection.Output;

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        int retValue = (int)retParam.Value;

                        responseMessage.Content = new StringContent(
                            retValue == 0 ? "{\"return\":true}" : "{\"return\":false}", 
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
