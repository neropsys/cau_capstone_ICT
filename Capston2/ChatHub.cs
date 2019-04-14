﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

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
            Users fromUser = UserContainer.gUserList.Where(x => x.username.Equals(fromUserID)).FirstOrDefault();
            if (fromUser != null)
            {
                int roomID = 0;
                if(fromUser.roomIDByTargetUser.TryGetValue(targetUserID, out roomID) == true)
                {

                    var chatList = UserContainer.gChatList[roomID].Item2;
                    string json = JsonConvert.SerializeObject(chatList); 
                    Clients.Client(fromUser.connectionID).getMessageByID(json);

                }
            }
        }


        public void CreateChat(string fromUser, string toUser)
        {
            var currentUser = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            var targetUser = UserContainer.gUserList.Find(x => x.username.Equals(toUser));
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

            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            var targetUser = UserContainer.gUserList.Find(x => x.username.Equals(toUser));

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

            UserContainer.gChatList[chatRoomID].Item2.Add(chatLog);

            foreach (var userInfo in UserContainer.gChatList[chatRoomID].Item1)
            {
                Clients.Client(userInfo.connectionID).sendMessage(message, fromUserInfo.username);
            }

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
            UserContainer.gUserList.Add(user); //add the connection user to the list
            string json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
            Clients.All.getUserList(json);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionID = Context.ConnectionId;
            Users user = UserContainer.gUserList.Where(x => x.connectionID.Equals(connectionID)).FirstOrDefault();
            if (user != null)
            {
                UserContainer.gUserList.Remove(user); //in the case of connection termination we removed the user from the list
                string json = JsonConvert.SerializeObject(UserContainer.gUserList); //send to client
                Clients.All.getUserList(json);
            }
            return base.OnDisconnected(stopCalled);
        }
       
    }
}