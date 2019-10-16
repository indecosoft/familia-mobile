﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Familia.Profile.Data
{
    class PersonalDisease
    {
        public int Id { get; set; }
        public int Cod { get; set; }
        public string Name { get; set; }

        public PersonalDisease(int cod, string name)
        {
            Id++;
            Cod = cod;
            Name = name;
        }

       

    }
}