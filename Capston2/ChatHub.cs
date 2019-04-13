using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Capston2
{

    public class TwoKeyDictionary<K1, K2, V> :
        Dictionary<K1, Dictionary<K2, V>>
    {
        public V this[K1 key1, K2 key2]
        {
            get
            {
                if (!ContainsKey(key1) || !this[key1].ContainsKey(key2))
                    throw new ArgumentOutOfRangeException();
                return base[key1][key2];
            }
            set
            {
                if (!ContainsKey(key1))
                    this[key1] = new Dictionary<K2, V>();
                this[key1][key2] = value;
            }
        }
        public void Add(K1 key1, K2 key2, V value)
        {
            if (!ContainsKey(key1))
                this[key1] = new Dictionary<K2, V>();
            this[key1][key2] = value;
        }
        public bool ContainsKey(K1 key1, K2 key2)
        {
            return base.ContainsKey(key1) && this[key1].ContainsKey(key2);
        }
        public new IEnumerable<V> Values
        {
            get
            {
                return from baseDict in base.Values
                       from baseKey in baseDict.Keys
                       select baseDict[baseKey];
            }
        }
    }
    public class ChatHub : Hub
    {
        const int MIN_MEMBER_FOR_GROUPCHAT = 2;
        struct ChatLog
        {
            string text { get; set; }
            string time { get; set; }
        }
        public class Users
        {
            public string username { get; set; }
            public string connectionID { get; set; }
            public string rightNowStr { get; set; }
            public List<int> belongingChatID { get; set; }//MeRightNow or other group chats
            public Dictionary<string, int> roomIDByTargetUser { get; set; }//key:other user's name, value:chat room ID
        }

        
        public class RightNow
        {
            public List<Users> users { get; set; }
            public string tagName { get; set; }

        }
        public struct CHAT_LOG
        {
            public string text;
            public string user;
        }
        public static Dictionary<int, List<Users>> gChatList = new Dictionary<int, List<Users>>();
        public static Dictionary<int, List<CHAT_LOG>> gChatLog = new Dictionary<int, List<CHAT_LOG>>();

        public static List<Users> gUserList = new List<Users>();
        public void Hello()
        {
            Clients.All.hello();
        }
        public void GetMessageByID(string fromUserID, string targetUserID)
        {
            Users fromUser = gUserList.Where(x => x.username.Equals(fromUserID)).FirstOrDefault();
            if (fromUser != null)
            {
                int roomID = 0;
                if(fromUser.roomIDByTargetUser.TryGetValue(targetUserID, out roomID) == true)
                {

                    var chatList = gChatLog[roomID];
                    string json = JsonConvert.SerializeObject(chatList); 
                    Clients.Client(fromUser.connectionID).getMessageByID(json);

                }
            }
        }

        static int globalChatRoomId = 0;

        public void CreateChat(string fromUser, string toUser)
        {
            var currentUser = gUserList.Find(x => x.username.Equals(fromUser));
            var targetUser = gUserList.Find(x => x.username.Equals(toUser));
            if (currentUser == null || targetUser == null)
            {
                //requesting user does not exist or target user does not exist
                return;
            }

            int chatRoomID = 0;
            if (currentUser.roomIDByTargetUser.TryGetValue(toUser, out chatRoomID) == false)
            {
                //create new chat
                List<Users> userList = new List<Users>();
                userList.Add(currentUser);
                userList.Add(targetUser);

                gChatList.Add(globalChatRoomId, userList);
                currentUser.roomIDByTargetUser.Add(toUser, globalChatRoomId);
                targetUser.roomIDByTargetUser.Add(fromUser, globalChatRoomId);
                gChatLog.Add(globalChatRoomId, new List<CHAT_LOG>());

                globalChatRoomId++;
            }
        }

        public void SendMessage(string message, string fromUser, string toUser)
        //I have defined 2 parameters. These are the parameters to be sent here from the client software
        {
            var sender_connectionID = Context.ConnectionId;

            var fromUserInfo = gUserList.Find(x => x.username.Equals(fromUser));
            var targetUser = gUserList.Find(x => x.username.Equals(toUser));

            if(fromUser == null || targetUser == null)
            {//either user does not exist
                return;
            }
            int chatRoomID = 0;
            if (fromUserInfo.roomIDByTargetUser.TryGetValue(toUser, out chatRoomID) == false)
            {
                CreateChat(fromUser, toUser);
                chatRoomID = fromUserInfo.roomIDByTargetUser[toUser];
            }

            var chatLog = new CHAT_LOG();
            chatLog.user = fromUserInfo.username;
            chatLog.text = message;
            gChatLog[chatRoomID].Add(chatLog);

            foreach (var userInfo in gChatList[chatRoomID])
            {
                Clients.Client(userInfo.connectionID).sendMessage(message, fromUserInfo.username);
            }


//             Users user = gUserList.Where(x => x.connectionID.Equals(sender_connectionID)).FirstOrDefault();
//             if (user != null)
//             {
// 
//                 Clients.Client(target_connectionID).SendMessage(message, user.username);
//             }
        }
        public override Task OnConnected()
        {
            var connectionID = Context.ConnectionId;
            string userName = Context.QueryString["username"];
            string rightNowStr = Context.QueryString["rightNowStr"];
            if (string.IsNullOrEmpty(userName))
            {
                userName = Context.Headers["username"];
            }
            Users user = new Users()
            {
                username = userName,
                connectionID = connectionID,
                rightNowStr = "",
                belongingChatID = new List<int>(),
                roomIDByTargetUser = new Dictionary<string, int>()
            };
            gUserList.Add(user); //add the connection user to the list
            string json = JsonConvert.SerializeObject(gUserList); //send to client
            Clients.All.getUserList(json);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionID = Context.ConnectionId;
            Users user = gUserList.Where(x => x.connectionID.Equals(connectionID)).FirstOrDefault();
            if (user != null)
            {
                gUserList.Remove(user); //in the case of connection termination we removed the user from the list
                string json = JsonConvert.SerializeObject(gUserList); //send to client
                Clients.All.getUserList(json);
            }
            return base.OnDisconnected(stopCalled);
        }
       
    }
}