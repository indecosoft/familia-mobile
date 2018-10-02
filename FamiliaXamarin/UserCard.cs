using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

namespace FamiliaXamarin
{
    class UserCard
    {
        public string Name;
        public string Probleme;
        public string Email;
        public string Avatar;
        public int Url;

        public UserCard(string name, string email, string probleme, string avatar, int url)
        {
            Name = name;
            Probleme = probleme;
            Url = url;
            Email = email;
            Avatar = avatar;
        }
    }
}