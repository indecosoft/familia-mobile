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
using FamiliaXamarin.Helpers;

namespace Familia.Profile.Data
{
    class PersonView
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Birthdate { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public List<PersonalDisease> ListOfPersonalDiseases { get; set; }

        public PersonView(string name, string email, string birthdate, string gender, string avatar, List<PersonalDisease> list)
        {
            Name = name;
            Email = email;
            Birthdate = birthdate;
            Gender = gender;
            Avatar = avatar;
            ListOfPersonalDiseases = list;
        }
        
    }
}