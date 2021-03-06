﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Capston2DataAccess;
namespace Capston2
{
    public class ChatHub : Hub
    {

        public class RightNow
        {
            public List<Users> users { get; set; }
            public string tagName { get; set; }

        }

        public void Hello()
        {
            Clients.All.hello();
        }
        public void GetMessageByID(string fromUserID, string targetUserID)
        {
            Users fromUser = UserContainer.gUserList.Where(x => x.userId.Equals(fromUserID)).FirstOrDefault();
            if (fromUser != null)
            {
                int roomID = 0;
                if(fromUser.roomIDByTargetUser.TryGetValue(targetUserID, out roomID) == true)
                {

                    var chatList = UserContainer.gChatList[roomID].Item2;
                    string json = JsonConvert.SerializeObject(chatList);
                    //json = EncodeUtf16ToUtf8.Utf16ToUtf8(json)
                    Clients.Client(fromUser.connectionID).getMessageByID(json);

                }
            }
        }

        public void GetMessageByIndex(string fromUser, string targetUserID, int chatIndex)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            if (fromUserInfo.roomIDByTargetUser.TryGetValue(targetUserID, out int roomID) == true)
            {
                var chatLog = UserContainer.gChatList[roomID].Item2;

                var slicedChatList = chatLog.Skip(chatIndex).ToList();
                string json = JsonConvert.SerializeObject(slicedChatList);

                Clients.Client(fromUserInfo.connectionID).updateChatByIndex(json);
            }

        }
        public void GetAllUser(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            string _json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
            Clients.Client(fromUserInfo.connectionID).getUserList(_json);
        }
        public void CreateChat(string fromUser, string toUser)
        {
            var currentUser = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            var targetUser = UserContainer.gUserList.Find(x => x.userId.Equals(toUser));
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

               
                currentUser.roomIDByTargetUser.Add(toUser, RoomID.globalChatRoomId);
                targetUser.roomIDByTargetUser.Add(fromUser, RoomID.globalChatRoomId);

                UserContainer.gChatList.Add(RoomID.globalChatRoomId, Tuple.Create( userList, new List<CHAT_LOG>()));

                RoomID.globalChatRoomId++;
            }
        }

        public void SendMessage(string fromUser, string toUser, string message)
        //I have defined 2 parameters. These are the parameters to be sent here from the client software
        {
            var sender_connectionID = Context.ConnectionId;

            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            var targetUser = UserContainer.gUserList.Find(x => x.userId.Equals(toUser));

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

            var chatLog = new CHAT_LOG
            {
                userId = fromUserInfo.userId,
                text = message,
                index = UserContainer.gChatList[chatRoomID].Item2.Count,
                userNick = fromUserInfo.userNick,
                time = DateTime.Now.AddHours(9).ToString("s")
            };
            UserContainer.gChatList[chatRoomID].Item2.Add(chatLog);

            foreach (var userInfo in UserContainer.gChatList[chatRoomID].Item1)
            {
                Clients.Client(userInfo.connectionID).sendMessage(message, fromUserInfo.userId);
            }

        }

        public override Task OnConnected()
        {
            var connectionID = Context.ConnectionId;
            string userId = Context.Request.Headers["userId"];
            string rightNowStr = Context.Request.Headers["rightNowStr"];
            
            string _bopParty = Context.Request.Headers["bopParty"];
            string userNick = null;
            using (capston_databaseEntities userDataEntites = new capston_databaseEntities())
            {
                userNick = userDataEntites.USER_INFO.First(x => x.id == userId).nickname;
            }
            var userInfo = UserContainer.gUserList.Where(x => x.userId == userId).FirstOrDefault();
            if (userInfo != null)
            {
                userInfo.connectionID = connectionID;
                string _json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
                Clients.All.getUserList(_json);
                return base.OnConnected();
            }
            else
            {
                Users user = new Users()
                {
                    userId = userId,
                    connectionID = connectionID,
                    rightNowStr = "",
                    userNick = userNick,
                    bopPartyId = Int32.Parse(_bopParty),
                    belongingChatID = new List<int>(),
                    roomIDByTargetUser = new Dictionary<string, int>()
                };
                UserContainer.gUserList.Add(user); //add the connection user to the list
            }
            string json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
            Clients.All.getUserList(json);
            return base.OnConnected();
        }
        public override Task OnReconnected()
        {

            string json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
            Clients.All.getUserList(json);
            return base.OnReconnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionID = Context.ConnectionId;
            Users user = UserContainer.gUserList.Where(x => x.connectionID.Equals(connectionID)).FirstOrDefault();
            if (user != null)
            {
                //UserContainer.gUserList.Remove(user); //in the case of connection termination we removed the user from the list
                string json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
                Clients.All.getUserList(json);
            }
            return base.OnDisconnected(stopCalled);
        }
       
    }
}