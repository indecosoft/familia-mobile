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

namespace FamiliaXamarin.JsonModels
{
    class SharingModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public int Id { get; set; }
        public string Imei { get; set; }

    }
}