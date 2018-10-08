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
    class MedicationSchedule
    {
        public string Uuid { get;}
        public string Timestampstring { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Postpone { get; set; }

        public MedicationSchedule(string uuid, string timestampstring, string title, string content, int postpone)
        {
            Uuid = uuid;
            Timestampstring = timestampstring;
            Title = title;
            Content = content;
            Postpone = postpone;
        }
    }
}