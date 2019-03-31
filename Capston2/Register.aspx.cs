using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Security.Cryptography;
namespace Capston2
{
    public partial class Register : System.Web.UI.Page
    {

        //testing
        string cs = ConfigurationManager.ConnectionStrings["UserInfoDBString"].ConnectionString;
        SqlCommand cmd = new SqlCommand();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (IDBox.Text == "" || PswdBox.Text == "")
                return;
           


            string userID = IDBox.Text;
            string userPassword = PswdBox.Text;

            PasswordHash hash = new PasswordHash(userPassword);
            var hashedValue = hash.ToArray();
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_add_user", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 50).Value = IDBox.Text;
                        cmd.Parameters.Add("@hash", SqlDbType.Binary, PasswordHash.HashSize + PasswordHash.SaltSize).Value = hashedValue;
                        var returnParameter = cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)returnParameter.Value;
                        connection.Close();

                        if(retValue == -1)
                        {
                            //duplicate id
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionValue = ex.Message;
                        exceptionValue += "zz";
                        connection.Close();
                    }
                }
            }
        }

        protected void LoginBtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_get_hash", connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 50).Value = IDBox.Text;

                        var hashParam = cmd.Parameters.Add("@hash", SqlDbType.Binary, PasswordHash.HashSize + PasswordHash.SaltSize);
                        hashParam.Direction = ParameterDirection.Output;

                        var retParam = cmd.Parameters.Add("@result", SqlDbType.Int);
                        retParam.Direction = ParameterDirection.Output;

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        int retValue = (int)retParam.Value;
                        if(retValue == -1)
                        {
                            connection.Close();
                            //id or pswd is wrong
                            return;
                        }
                        byte[] hash = (byte[])hashParam.Value;
                        connection.Close();
                        PasswordHash pswdHash = new PasswordHash(hash);
                        if (pswdHash.Verify(PswdBox.Text))
                        {
                            int i = 0;
                            i++;
                        }


                    }
                    catch (Exception ex)
                    {
                        string exceptionValue = ex.Message;
                        exceptionValue += "zz";
                        connection.Close();
                    }
                }
            }
        }
    }
}