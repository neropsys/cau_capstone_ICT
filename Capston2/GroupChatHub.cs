using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Capston2
{
    public class GroupChatHub : Hub
    {
        const int MIN_MEMBER_FOR_GROUPCHAT = 2;

        public static Dictionary<string, int> gRightNowRoomID = new Dictionary<string, int>();//key:RightNow tag string, value:room ID

        public void Hello()
        {
            Clients.All.hello();
        }

        void JoinRightNowRoom(string fromUser, string roomName)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            fromUserInfo.rightNowStr = roomName;
            //room has already been created
            int roomID = 0;
            if (gRightNowRoomID.TryGetValue(roomName, out roomID))
            {
                var roomInfo = UserContainer.gChatList[roomID];
                string chatLog = JsonConvert.SerializeObject(roomInfo.Item2);
                Clients.Client(fromUserInfo.connectionID).foundRoom(chatLog);

            }
        }

        public void GetRightnowTags(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            var keys = gRightNowRoomID.Keys.ToList();
            string json = JsonConvert.SerializeObject(keys);
            Clients.Client(fromUserInfo.connectionID).rightNowTags(json);
        }
        public void UploadTag(string fromUser, string keyword)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            fromUserInfo.rightNowStr = keyword;

            var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(keyword));

            if (userListWithSameTag.Count < MIN_MEMBER_FOR_GROUPCHAT)
            {
                //show only chat rooms


                //Clients.All.rightNowTags
                return;
            }

            int chatID = 0;
            if (gRightNowRoomID.TryGetValue(keyword, out chatID) == false)
            {
                //crate room
                UserContainer.gChatList.Add(RoomID.globalChatRoomId, Tuple.Create(userListWithSameTag, new List<CHAT_LOG>()));
                gRightNowRoomID.Add(keyword, RoomID.globalChatRoomId);
                foreach (var userInfo in userListWithSameTag)
                {
                    userInfo.belongingChatID.Add(RoomID.globalChatRoomId);
                }
                RoomID.globalChatRoomId++;

                //notify all people that the room has been created
                foreach (var userInfo in userListWithSameTag)
                {
                    string json = JsonConvert.SerializeObject(gRightNowRoomID.Keys.ToArray());
                    Clients.Client(userInfo.connectionID).rightNowTags(json);
                }
                return;
            }
            chatID = gRightNowRoomID[keyword];

            var roomInfo = UserContainer.gChatList[chatID];
            roomInfo.Item1.Add(fromUserInfo);

            fromUserInfo.belongingChatID.Add(RoomID.globalChatRoomId);

            //let user join room & send all log in the room
            string chatLog = JsonConvert.SerializeObject(roomInfo.Item2);
            Clients.Client(fromUserInfo.connectionID).foundRoom(chatLog);
        }

        public void SendGroupMsg(string fromUser, string msg)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            var roomId = 0;
            if (gRightNowRoomID.TryGetValue(fromUserInfo.rightNowStr, out roomId) == false)
            {
                return;
            }
            var roomInfo = UserContainer.gChatList[roomId];
            roomInfo.Item2.Add(new CHAT_LOG { text = msg, user = fromUserInfo.username });
            foreach (var userInfo in roomInfo.Item1)
            {
                Clients.Client(userInfo.connectionID).onGroupChat(fromUserInfo.username, msg);
            }
        }

    }
}