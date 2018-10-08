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

namespace FamiliaXamarin.Medicatie.Entities
{
    public class Hour
    {
        public string Nume { get; set; }
        public string Id { get; set; }

        public Hour(string nume, string id)
        {
            this.Nume = nume;
            this.Id = id;
        }
        public Hour() { }
    }
}