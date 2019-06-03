using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Capston2
{
    public class BopPartyHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public void CheckGroupchatExists(string fromUser, int id)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            bool ret = UserContainer.gChatList.ContainsKey(id);

            Clients.Client(fromUserInfo.connectionID).onGroupchatCheck(ret);

        }
        //call this function when posting bopParty article to RESIDENCE_POST is successful
        public void CreateBopChat(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            var tuple = new Tuple<List<Users>, List<CHAT_LOG>>(new List<Users>(), new List<CHAT_LOG>());
            tuple.Item1.Add(fromUserInfo);//creator of groupchat == master of groupchat
            int chatRoomID = RoomID.globalChatRoomId++;
            fromUserInfo.bopPartyId = chatRoomID;

            UserContainer.gChatList[chatRoomID] = tuple;
            Clients.Client(fromUserInfo.connectionID).onBopChatCreated(chatRoomID);
        }
        public void JoinBopParty(string fromUser, string toUserNick)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;

            var toUserInfo = UserContainer.gUserList.Find(x => x.userNick.Equals(toUserNick));
            if (toUserInfo == null)
                return;


            if (UserContainer.gChatList.TryGetValue(toUserInfo.bopPartyId, out var value))
            {
                fromUserInfo.bopPartyId = toUserInfo.bopPartyId;
                value.Item1.Add(fromUserInfo);
                Clients.Client(fromUserInfo.connectionID).onJoinBopParty(toUserInfo.bopPartyId);
            }
            else
            {
                Clients.Client(fromUserInfo.connectionID).onJoinBopPartyFail(toUserInfo.bopPartyId);
            }
        }
        public void LeaveBopParty(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            int bopPartyId = fromUserInfo.bopPartyId;
            if (UserContainer.gChatList.TryGetValue(fromUserInfo.bopPartyId, out var value))
            {
                //master requesting to leave == destroy room
                if(value.Item1[0] == fromUserInfo)
                {
                    _DeleteBopParty(fromUserInfo);
                    return;
                }
                fromUserInfo.bopPartyId = -1;
                value.Item1.Remove(fromUserInfo);
                Clients.Client(fromUserInfo.connectionID).onBopPartyLeave(bopPartyId);
            }
            else
            {
                Clients.Client(fromUserInfo.connectionID).onBopPartyLeaveFail(bopPartyId);
            }
        }
        public void KickUser(string fromUser, string targetUserId)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            var targetUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(targetUserId));
            if (targetUserInfo == null)
            {
                Clients.Client(fromUserInfo.connectionID).onFailKickUser("USER_NOT_EXIST");
                return;
            }
            //prevent suicide
            if(fromUserInfo == targetUserInfo)
            {
                return;
            }
            if (UserContainer.gChatList.TryGetValue(fromUserInfo.bopPartyId, out var value) && value.Item1[0] == fromUserInfo)
            {
                if(value.Item1.Contains(targetUserInfo))
                {
                    foreach (var user in value.Item1)
                    {
                        //notify all other user that user has been kicked.
                        //delete groupchat UI from kicked user if targetUserID == myID
                        Clients.Client(targetUserInfo.connectionID).onKickedNotify(targetUserId);
                    }
                    value.Item1.Remove(targetUserInfo);
                    targetUserInfo.bopPartyId = -1;
                }
                else
                {
                    Clients.Client(fromUserInfo.connectionID).onFailKickUser("USER_NOT_IN_CHAT");
                }
            }
        }
        public void DeleteBopParty(string fromUser)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            _DeleteBopParty(fromUserInfo);
        }
        protected void _DeleteBopParty(Users fromUserInfo)
        {
            //room exists && requesting user is the creator of chatroom
            if (UserContainer.gChatList.TryGetValue(fromUserInfo.bopPartyId, out var value) && value.Item1[0] == fromUserInfo)
            {
                foreach (var user in value.Item1)
                {
                    user.bopPartyId = -1;
                    Clients.Client(user.connectionID).onBopPartyDeleted(fromUserInfo.bopPartyId);
                }
                UserContainer.gChatList.Remove(fromUserInfo.bopPartyId);
            }
            else if (value == null)
            {
                Clients.Client(fromUserInfo.connectionID).onBopPartyDeleteFailed(fromUserInfo.bopPartyId);
            }
            //else there is chatroom but requesting user is not the master of the room
        }
        public void SendGroupMsg(string fromUser, string msg)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            if (UserContainer.gChatList.TryGetValue(fromUserInfo.bopPartyId, out var value))
            {
                var chatLog = new CHAT_LOG
                {
                    index = value.Item2.Count,
                    text = msg,
                    time = DateTime.Now.AddHours(9).ToString("s"),
                    userId = fromUserInfo.userId,
                    userNick = fromUserInfo.userNick
                };
                value.Item2.Add(chatLog);
                string strChatLog = JsonConvert.SerializeObject(chatLog);
                foreach (var user in value.Item1)
                {
                    Clients.Client(user.connectionID).onBopPartyMsgReceived(strChatLog);
                }
            }
            else
            {
                Clients.Client(fromUserInfo.connectionID).onBopPartyGroupMsgFailed();
            }
        }
        public void GetMsgByIndex(string fromUser, int index)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUser));
            if (fromUserInfo == null)
                return;
            if (UserContainer.gChatList.TryGetValue(fromUserInfo.bopPartyId, out var value))
            {
                var chatLog = value.Item2;
                var slicedChatList = chatLog.Skip(index).ToList();
                string json = JsonConvert.SerializeObject(slicedChatList);
                Clients.Client(fromUserInfo.connectionID).updateGroupChatByIndex(json);
            }
        }
        public void GetMyBopPartyID(string fromUserId)
        {
            var fromUserInfo = UserContainer.gUserList.Find(x => x.userId.Equals(fromUserId));
            if (fromUserInfo == null)
                return;
            Clients.Client(fromUserInfo.connectionID).onMyBopParty(fromUserInfo.bopPartyId);
        }
    }
    
}