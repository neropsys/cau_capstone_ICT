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

        public class EncodeUtf16ToUtf8
        {
            public static string Utf16ToUtf8(string utf16String)
            {

                string utf8String = String.Empty;



                //UTF-16 바이트를 배열로 얻어온다.

                byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);

                //UTF-16 바이트를 배열을 UTF-8로 변환한다.

                byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);



                // UTF8 bytes 배열 내부에 UTF8 문자를 추가한다.

                for (int i = 0; i < utf8Bytes.Length; i++)

                {

                    // Because char always saves 2 bytes, fill char with 0

                    byte[] utf8Container = new byte[2] { utf8Bytes[i], 0 };

                    utf8String += BitConverter.ToChar(utf8Container, 0);

                }



                // UTF8을 리턴한다.

                return utf8String;

            }
        }
        public class Users  //User Model
        {
            public string userId { get; set; }
            public string userNick { get; set; }
            public string connectionID { get; set; }
            public string rightNowStr { get; set; }
            public List<int> belongingChatID { get; set; }//groupchat id that this user belongs(include, MeRightNow groupchat)
            //public Dictionary<string, int> roomIDByTargetUser { get; set; }//key:other user's name, value:chat room ID
        }
        public struct CHAT_LOG
        {
            public string text;
            public string user;
            public int index;
        }
        public 
        HubConnection connection; // Connection defination
        IHubProxy chat; // chat proxy defination
        IHubProxy groupChat; // chat proxy defination
        List<Users> users = null; // user list
        Users sender_message = null; // sender message in user
        public Form1()
        {
            InitializeComponent();
            string fds = DateTime.Now.ToString("s");

        }


        bool connect(string userName, string rightNowStr, string userNick)
        {
            connection = new HubConnection("http://localhost:13458/signalr");
            userName = EncodeUtf16ToUtf8.Utf16ToUtf8(userName);
            rightNowStr = EncodeUtf16ToUtf8.Utf16ToUtf8(rightNowStr);
            connection.Headers.Add("userId", userName);
            connection.Headers.Add("rightNowStr", rightNowStr);
            connection.Headers.Add("userNick", userNick);
            chat = connection.CreateHubProxy("ChatHub");
            groupChat = connection.CreateHubProxy("GroupChatHub");
            try
            {
                //called by server
                chat.On<string>("getUserList", (message) =>
                { // getUserList is ChatHub function
                    var json_serialize = new JavaScriptSerializer();
                    users = json_serialize.Deserialize<List<Users>>(message);
                    List<string> user_names = users.Select(x => x.userId).ToList();

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
                            chatLogBox.Text += chat.user + ":" + chat.text + "/ index:" + chat.index + "\n"; // writing username and message on richTextBox1 
                        }
                       
                    }));
                });
                groupChat.On<string>("foundRoom", (message) =>
                {
                    var json_serialize = new JavaScriptSerializer();
                    List<CHAT_LOG> chatLog = json_serialize.Deserialize<List<CHAT_LOG>>(message);
                    BeginInvoke(new Action(() =>
                    {
                        groupChatLog.Clear();
                        foreach (var chat in chatLog)
                        {
                            groupChatLog.Text += chat.user + ":" + chat.text + "/ index:" + chat.index + "\n"; // writing username and message on richTextBox1 
                        }

                    }));
                });
                groupChat.On<string, string>("updateGroupChatByIndex", (roomTag, message) =>
                {
                    var json_serialize = new JavaScriptSerializer();
                    List<CHAT_LOG> chatLog = json_serialize.Deserialize<List<CHAT_LOG>>(message);
                    BeginInvoke(new Action(() =>
                    {
                        groupChatLog.Clear();
                        foreach (var chat in chatLog)
                        {
                            groupChatLog.Text += chat.user + ":" + chat.text + "/ index:" + chat.index + "\n"; // writing username and message on richTextBox1 
                        }

                    }));
                });
                groupChat.On<string>("updateRightNowChatList", (message) =>
                {
                    var json_serialize = new JavaScriptSerializer();
                    List<string> tagList = json_serialize.Deserialize<List<string>>(message);
                    BeginInvoke(new Action(() =>
                    {
                        rightNowList.Items.Clear();
                        rightNowList.Items.AddRange(tagList.ToArray());
                    }));
                });
                groupChat.On<string, string>("onGroupChat", (user, message) =>
                {
                    BeginInvoke(new Action(() =>
                    {
                        groupChatLog.Text += user + ":" + message + "\n"; // writing username and message on richTextBox1 
                    }));
                });

                groupChat.On<string>("updateAvailableTags", (tagListJson) =>
                {
                    var json_serialize = new JavaScriptSerializer();
                    List<string> tagList = json_serialize.Deserialize<List<string>>(tagListJson);

                    BeginInvoke(new Action(() =>
                    {
                        this.tagList.Items.Clear();
                        this.tagList.Items.AddRange(tagList.ToArray());
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
            if (connect(user.Text.Trim(), rightNowStr.Text.Trim(), userNickInput.Text.Trim()))
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
                chat.Invoke("SendMessage", user.Text.Trim(),  sender_message.userId, textbox1.Text.Trim()); //Send message to user
                textbox1.Text = "";
            }
        }
        

        private void getTextLogBtn_Click(object sender, EventArgs e)
        {
            chat.Invoke("GetMessageByID", user.Text.Trim(), sender_message.userId);
        }

        private void createRoomBtn_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("UploadTag", user.Text.Trim(), rightNowStr.Text.Trim());
        }

        private void rightNowList_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupChat.Invoke("JoinRightNowRoom", user.Text.Trim(), rightNowList.SelectedItem.ToString());
        }
        private void getRoomTagBtn_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("GetRightnowRooms", user.Text.Trim());
        }

        private void sendToGroupBtn_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("SendGroupMsg", user.Text.Trim(), groupChatTextBox.Text);
            groupChatTextBox.Text = "";
        }

        private void tagList_SelectedIndexChanged(object sender, EventArgs e)
        {
            rightNowStr.Text = tagList.SelectedItem.ToString();
        }

        private void getChatFrom0_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("GetRightnowMessageByIndex", user.Text.Trim(), rightNowList.SelectedItem.ToString() , 0);
        }

        private void getTagButton_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("BroadcastTagList");
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            groupChat.Invoke("GetRightnowTagUserInfo", user.Text.Trim());
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
