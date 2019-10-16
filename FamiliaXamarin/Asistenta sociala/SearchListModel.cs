using System;
using System.Collections;
using Android.OS;
using Android.Runtime;
using Java.Interop;

namespace FamiliaXamarin
{ 
    public class SearchListModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsSelected { get; set; }
    }

}