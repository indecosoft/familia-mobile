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
using SQLite;

namespace Familia.Profile
{
    class ProfileDataModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Base64Image { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ImageName { get; set; }
        public string ImageExtension { get; set; }
    }
}