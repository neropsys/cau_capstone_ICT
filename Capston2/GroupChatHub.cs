using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Capston2DataAccess;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Capston2
{
    public class GroupChatHub : Hub
    {

        const int MIN_MEMBER_FOR_GROUPCHAT = 2;


        public static Dictionary<string, int?> gRightNowRoomID = new Dictionary<string, int?>();//key:RightNow tag string, value:room ID,
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
            if (gRightNowRoomID.TryGetValue(roomName, out int? roomID) && roomID != null)
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

        public void GetRightnowMessageByIndex(string fromUser, string roomTag, int chatIndex)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            if (gRightNowRoomID.TryGetValue(roomTag, out int? roomID) && roomID != null)
            {
                var chatLog = UserContainer.gChatList[roomID.Value].Item2;

                var slicedChatList = chatLog.Skip(chatIndex).ToList();
                string json = JsonConvert.SerializeObject(slicedChatList);

                Clients.Client(fromUserInfo.connectionID).updateGroupChatByIndex(roomTag, json);
            }

        }

        public class UserInfoFormat
        {
            public string id { get; set; }
            public string nickname { get; set; }
        }
        public void GetRightnowTagUserInfo(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            string userTag = fromUserInfo.rightNowStr;

            var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(userTag));
            List<UserInfoFormat> ret = new List<UserInfoFormat>();
            using (capston_databaseEntities userDataEntities = new capston_databaseEntities())
            {
                
                foreach (var userInfo in userListWithSameTag)
                {
                    var userProfile = userDataEntities.USER_INFO
                        .Where(x => x.id == userInfo.username)
                        .Select(f => new UserInfoFormat { id = f.id, nickname = f.nickname }).ToList();
                    ret.AddRange(userProfile);
                }
            }
            string retJson = JsonConvert.SerializeObject(ret);

            Clients.Client(fromUserInfo.connectionID).getRightNowTagUserInfo(retJson);
            
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
            if (userListWithPreviousTag.Count == 1 && gRightNowRoomID.ContainsKey(fromUserInfo.rightNowStr))
            {
                gRightNowRoomID.Remove(fromUserInfo.rightNowStr);
            }


            //remove user from previous tag room and delete room if it is below threshold
            if (gRightNowRoomID.TryGetValue(fromUserInfo.rightNowStr, out int? roomId) &&
                roomId != null &&
                UserContainer.gChatList.ContainsKey(roomId.Value))
            {
                var userList = UserContainer.gChatList[roomId.Value].Item1;
                userList.Remove(fromUserInfo);
                if (userList.Count < MIN_MEMBER_FOR_GROUPCHAT)
                {
                    DeleteGroupChat(roomId.Value, ref userList);

                }

            }

            //2. update tag
            fromUserInfo.rightNowStr = keyword;


            //return if tag is set to null
            if (keyword == "" || gRightNowRoomID.ContainsKey(keyword))
            {
                //broadcast
                if (gRightNowRoomID.ContainsKey(keyword))
                {
                    BroadcastTagList();
                }

                return;
            }
            else
            {
                gRightNowRoomID.Add(keyword, null);
            }
            var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(keyword));

            //4. check if tag is eligible for group chat(number of users using same tag is above threshold)
            if (userListWithSameTag.Count < MIN_MEMBER_FOR_GROUPCHAT)
            {
                //4.1 deleting room since tag count is below required threshold
                if (gRightNowRoomID.TryGetValue(keyword, out int? _chatID) && _chatID != null)
                {
                    DeleteGroupChat(_chatID.Value, ref userListWithSameTag);
                }
                BroadcastTagList();
                return;
            }

            //4.2 if tag count is above threshold, but no room has been created,
            int? chatID = gRightNowRoomID[keyword];

            if (chatID.HasValue == false && 
                userListWithSameTag.Count >= MIN_MEMBER_FOR_GROUPCHAT)
            {
                gRightNowRoomID.Add(keyword, RoomID.globalChatRoomId);
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
            foreach (var tag in tagList)
            {
                var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(tag));

                if (userListWithSameTag.Count >= MIN_MEMBER_FOR_GROUPCHAT)
                {
                    roomList.Add(tag);
                }
            }
            string json = JsonConvert.SerializeObject(roomList.ToArray());
            Clients.All.updateRightNowChatList(json);
        }
        public void BroadcastTagList()
        {
            Dictionary<string, int> tagUserNum = new Dictionary<string, int>();
            foreach(var roominfo in gRightNowRoomID)
            {
                var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(roominfo.Key));

                tagUserNum.Add(roominfo.Key, userListWithSameTag.Count);
                
            }
            
            string json = JsonConvert.SerializeObject(tagUserNum.ToArray());

            Clients.All.updateAvailableTags(json);
            
        }

        public void GetTag(string fromuser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromuser));
            if (fromUserInfo == null)
                return;
            Dictionary<string, int> tagUserNum = new Dictionary<string, int>();
            foreach (var roominfo in gRightNowRoomID)
            {
                var userListWithSameTag = UserContainer.gUserList.FindAll(x => x.rightNowStr.Equals(roominfo.Key));

                tagUserNum.Add(roominfo.Key, userListWithSameTag.Count);

            }

            string json = JsonConvert.SerializeObject(tagUserNum.ToArray());

            Clients.Client(fromUserInfo.connectionID).updateAvailableTags(json);

        }
        public void SendGroupMsg(string fromUser, string msg)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.username.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            if (gRightNowRoomID.TryGetValue(fromUserInfo.rightNowStr, out int? roomId) == false || roomId == null)
            {
                return;
            }

            var roomInfo = UserContainer.gChatList[roomId.Value];
            int _index = roomInfo.Item2.Count;
            roomInfo.Item2.Add(new CHAT_LOG { text = msg, user = fromUserInfo.username, index = _index });
            foreach (var userInfo in roomInfo.Item1)
            {
                Clients.Client(userInfo.connectionID).onGroupChat(fromUserInfo.username, msg);
            }
        }

        protected void DeleteGroupChat(int roomID, ref List<Users> userList)
        {
            UserContainer.gChatList.Remove(roomID);

            //deleting room info from users
            foreach (var user in userList)
            {
                user.belongingChatID.Remove(roomID);
            }

            BroadcastRoomList();
        }

    }
}