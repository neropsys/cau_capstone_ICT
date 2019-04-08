﻿using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Capston2.Controllers
{
    public class AddProfileController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        public class ProfileModel
        {
            public string email { get; set; }
            public string nickname { get; set; }
            public string dateofbirth { get; set; }
            public string residence { get; set; }
            public string major { get; set; }
            public string hobby { get; set; }
            public string id { get; set; }
        }

        class ResponseFormat
        {
            public bool ret { get; set; }
            public string reason { get; set; }
        }
        // POST api/values
        [HttpPost]
        public HttpResponseMessage Post(ProfileModel value)
        {
            DateTime date;
            if (DateTime.TryParseExact(
                value.dateofbirth,
                "yyyy-MM-dd",
                CultureInfo.CurrentCulture,
                DateTimeStyles.None, out date) == false)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //TODO:check credential
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_add_user_info", connection))
                {
                    try
                    {
                        
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = value.email;
                        cmd.Parameters.Add("@nickname", SqlDbType.VarChar, 50).Value = value.nickname;
                        cmd.Parameters.Add("@dateofbirth", SqlDbType.Date).Value = date;
                        cmd.Parameters.Add("@residence", SqlDbType.VarChar, 50).Value = value.residence;
                        cmd.Parameters.Add("@major", SqlDbType.VarChar, 25).Value = value.major;
                        cmd.Parameters.Add("@hobby", SqlDbType.VarChar, 50).Value = value.hobby;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 50).Value = value.id;
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();


                        ResponseFormat responseFormat = new ResponseFormat();
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        if (retValue == -1)
                        {
                            //duplicate id
                            responseFormat.ret = false;
                            responseFormat.reason = "BAD_ID";
                        }
                        else if(retValue == -2)
                        {
                            //duplicate nickname
                            responseFormat.ret = false;
                            responseFormat.reason = "BAD_NICKNAME";
                        }
                        else
                        {
                            responseFormat.ret = true;
                            responseFormat.reason = "";
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