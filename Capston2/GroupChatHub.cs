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

        public static Dictionary<string, int?> gRightNowRoomID = new Dictionary<string, int?>();//key:RightNow tag string, value:room ID
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
            int? roomID = 0;
            if (gRightNowRoomID.TryGetValue(roomName, out roomID))
            {
                var roomInfo = UserContainer.gChatList[roomID.Value];
                string chatLog = JsonConvert.SerializeObject(roomInfo.Item2);
                Clients.Client(fromUserInfo.connectionID).foundRoom(chatLog);

            }
        }

        public void GetRightnowRooms(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            var keys = gRightNowRoomID.Keys.ToList();
            string json = JsonConvert.SerializeObject(keys);
            Clients.Client(fromUserInfo.connectionID).updateRightNowChatList(json);
        }

        //1. decrement tag count on tag change or delete if count is zero because no one uses it
        //2. update user's tag
        //4. check if tag count is eligible for group chat(number of users using same tag is above threshold)
        //4.1 if tag count is below threshold, destroy room
        //4.2 if tag count is above threshold, create room
        //5. join user if room exists
        public void UploadTag(string fromUser, string keyword)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null || fromUserInfo.rightNowStr == keyword)
                return;


            //delete if tag was only used by this user
            var userListWithPreviousTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(fromUserInfo.rightNowStr));
            if(userListWithPreviousTag.Count == 1 && gRightNowRoomID.ContainsKey(fromUserInfo.rightNowStr))
            {
                gRightNowRoomID.Remove(fromUserInfo.rightNowStr);
            }
            

            //remove user from previous tag room
            int? roomId = 0;
            if(gRightNowRoomID.TryGetValue(fromUserInfo.rightNowStr, out roomId) && 
                UserContainer.gChatList.ContainsKey(roomId.Value))
            {
                UserContainer.gChatList[roomId.Value].Item1.Remove(fromUserInfo);
            }

            //2. update tag
            fromUserInfo.rightNowStr = keyword;
            var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(keyword));

            //4. check if tag is eligible for group chat(number of users using same tag is above threshold)
            if (userListWithSameTag.Count < MIN_MEMBER_FOR_GROUPCHAT)
            {
                //4.1 deleting room since tag count is below required threshold
                int? _chatID = 0;
                if (gRightNowRoomID.TryGetValue(keyword, out _chatID))
                {
                    UserContainer.gChatList.Remove(_chatID.Value);

                    //deleting room info from users
                    foreach (var user in userListWithSameTag)
                    {
                        user.belongingChatID.Remove(_chatID.Value);
                    }
                }
                else
                {
                    gRightNowRoomID.Add(keyword, null);
                }
                BroadcastTagList();
                return;
            }

            //4.2 if tag count is above threshold, but no room has been created,
            int? chatID = gRightNowRoomID[keyword];

            if (chatID.HasValue == false)
            {
                gRightNowRoomID[keyword] = RoomID.globalChatRoomId;
                foreach (var userInfo in userListWithSameTag)
                {
                    userInfo.belongingChatID.Add(RoomID.globalChatRoomId);
                }
                UserContainer.gChatList.Add(RoomID.globalChatRoomId, Tuple.Create(userListWithSameTag, new List<CHAT_LOG>()));

                RoomID.globalChatRoomId++;

                BroadcastTagList();
                BroadcastRoomList();
                return;
            }
            //5. join user if room exists
            var roomInfo = UserContainer.gChatList[chatID.Value];
            roomInfo.Item1.Add(fromUserInfo);
            fromUserInfo.belongingChatID.Add(RoomID.globalChatRoomId);

        }
        void BroadcastRoomList()
        {
            var tagList = gRightNowRoomID.Keys.ToArray();
            List<string> roomList = new List<string>();


            //could have performance problem
            foreach(var tag in tagList)
            {
                var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(tag));

                if(userListWithSameTag.Count >= MIN_MEMBER_FOR_GROUPCHAT)
                {
                    roomList.Add(tag);
                }
            }
            string json = JsonConvert.SerializeObject(roomList.ToArray());
            Clients.All.updateRightNowChatList(json);
        }
        void BroadcastTagList()
        {
            string json = JsonConvert.SerializeObject(gRightNowRoomID.Keys.ToArray());
            Clients.All.updateAvailableTags(json);
        }
        public void SendGroupMsg(string fromUser, string msg)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            int? roomId = 0;
            if (gRightNowRoomID.TryGetValue(fromUserInfo.rightNowStr, out roomId) == false)
            {
                return;
            }
            var roomInfo = UserContainer.gChatList[roomId.Value];
            roomInfo.Item2.Add(new CHAT_LOG { text = msg, user = fromUserInfo.username });
            foreach (var userInfo in roomInfo.Item1)
            {
                Clients.Client(userInfo.connectionID).onGroupChat(fromUserInfo.username, msg);
            }
        }

    }
}