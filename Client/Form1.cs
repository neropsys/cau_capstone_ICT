using MetroFramework.Forms;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : MetroForm
    {
        public class Users  //User Model
        {
            public string username { get; set; }
            public string connectionID { get; set; }
            public string rightNowStr { get; set; }
            //public List<int> belongingChatID { get; set; }//groupchat id that this user belongs(include, MeRightNow groupchat)
            //public Dictionary<string, int> roomIDByTargetUser { get; set; }//key:other user's name, value:chat room ID
        }
        public struct CHAT_LOG
        {
            public string text;
            public string user;
        }
        HubConnection connection; // Connection defination
        IHubProxy chat; // chat proxy defination
        List<Users> users = null; // user list
        Users sender_message = null; // sender message in user
        public Form1()
        {
            InitializeComponent();
        }


        bool connect(string userName, string rightNowStr)
        {
            connection = new HubConnection("http://localhost:13458/signalr");
            connection.Headers.Add("username", userName);
            connection.Headers.Add("rightNowStr", rightNowStr);
            chat = connection.CreateHubProxy("ChatHub");
            try
            {
                chat.On<string>("getUserList", (message) =>
                { // getUserList is ChatHub function
                    var json_serialize = new JavaScriptSerializer();
                    users = json_serialize.Deserialize<List<Users>>(message);
                    List<string> user_names = users.Select(x => x.username).ToList();

                    var myIndex = user_names.IndexOf(userName);
                    user_names.Remove(userName);
                    users.RemoveAt(myIndex);
                    BeginInvoke(new Action(() =>
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.AddRange(user_names.ToArray());//User List in ListBox
                    }));
                });
                chat.On<string, string>("sendMessage", (message, user) => //sendMessage is ChatHub function
                {
                    BeginInvoke(new Action(() =>
                    {
                        chatLogBox.Text += user + ":" + message + "\n"; // writing username and message on richTextBox1 
                    }));
                });

                chat.On<string>("getMessageByID", (message) =>
                {
                    var json_serialize = new JavaScriptSerializer();
                    List<CHAT_LOG> chatLog = json_serialize.Deserialize<List<CHAT_LOG>>(message);
                    BeginInvoke(new Action(() =>
                    {
                        chatLogBox.Clear();
                        foreach(var chat in chatLog)
                        {
                            chatLogBox.Text += chat.user + ":" + chat.text+ "\n"; // writing username and message on richTextBox1 
                        }
                       
                    }));
                });
                connection.Start().Wait();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (connect(user.Text.Trim(), rightNowStr.Text.Trim()))
            {
                button1.Enabled = false; // Server Connection
                button2.Enabled = true;
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            if (listBox1.SelectedIndex != -1)
            {
                sender_message = users[listBox1.SelectedIndex]; //to send a message to the selected person
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                sender_message = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            connection.Stop(); // Connection Stop
            button1.Enabled = true;
            button2.Enabled = false;
            listBox1.Items.Clear();
            sender_message = null;
            button3.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!textbox1.Text.Trim().Equals("") && sender_message != null)
            {
                chat.Invoke("SendMessage", textbox1.Text.Trim(), user.Text.Trim(), sender_message.username); //Send message to user
                textbox1.Text = "";
                listBox1.SelectedIndex = -1;
            }
        }
        

        private void getTextLogBtn_Click(object sender, EventArgs e)
        {
            chat.Invoke("GetMessageByID", user.Text.Trim(), sender_message.username);
        }
    }
}
