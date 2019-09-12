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
    class PersonalData
    {
        public List<PersonalDisease> listOfPersonalDiseases { get; set; }
        public string Base64Image { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ImageName { get; set; }
        public string ImageExtension { get; set; }
        
        public PersonalData(List<PersonalDisease> listOfPersonalDiseases, string base64Image, string dateOfBirth, string gender, string imageName, string imageExtension)
        {
            this.listOfPersonalDiseases = listOfPersonalDiseases;
            Base64Image = base64Image;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            ImageName = imageName;
            ImageExtension = imageExtension;
        }

        public PersonalData()
        {
        }
    }
}