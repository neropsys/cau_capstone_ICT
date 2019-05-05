using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capston2
{
    public class Users
    {
        public string username { get; set; }
        public string connectionID { get; set; }
        public string rightNowStr { get; set; }
        public List<int> belongingChatID { get; set; }//MeRightNow or other group chats
        public Dictionary<string, int> roomIDByTargetUser { get; set; }//key:other user's name, value:chat room ID. 2 users have each other's name as key, and value is same
    }
    public struct CHAT_LOG
    {
        public string text;
        public string user;
        public int index;
    }
    public static class RoomID
    {
        public static int globalChatRoomId = 0;
    }
    public static class UserContainer
    {
       
        public static List<Users> gUserList = new List<Users>();
        //key: id, value: Item1: User List, Item2:Chat log
        public static Dictionary<int, Tuple<List<Users>, List<CHAT_LOG>>> gChatList = new Dictionary<int, Tuple<List<Users>, List<CHAT_LOG>>>();
    }
}