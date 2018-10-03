using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Org.Json;

namespace FamiliaXamarin
{
    internal class ChatModel
    {

        public static readonly int TypeMessage = 0;
        public static readonly int TypeLog = 1;
        public static readonly int TypeAction = 2;
        public static readonly int TypeMyMessage = 3;

        public int mType { get; set; }
    
        public string Message { get; set; }
        public string Username { get; set; }

        public ChatModel()
        {
        }
     
        public class Builder
        {
            private int mType;
            private string mUsername;
            private string mMessage;
            private string mAvatar;

            public Builder(int type)
            {
                mType = type;
            }

            public Builder Username(string username)
            {
                mUsername = username;
                return this;
            }

            public Builder Message(string message)
            {
                mMessage = message;
                return this;
            }
            public Builder Avatar(string avatar)
            {
                mAvatar = avatar;
                return this;
            }

            public ChatModel Build()
            {
                ChatModel message = new ChatModel
                {
                    mType = mType,
                    Username = mUsername,
                    Message = mMessage
                };
                //message.mAvatar = mAvatar;
                return message;
            }
        }
    }
}
