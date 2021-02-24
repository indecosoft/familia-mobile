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

namespace Familia.OngBenefits.ShowBenefits
{
    class Benefit
    {
        public string Name {get;}
        public string Details { get; }
        public string DateTimeReceived { get; }

        public string Observations { get; }

        public Benefit(string name, string details, string dateTimeReceived, string observations) {
            this.Name = name;
            this.Details = details;
            this.DateTimeReceived = dateTimeReceived;
            this.Observations = observations;
        }

    }
}