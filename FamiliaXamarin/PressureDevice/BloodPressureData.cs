using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Util;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;
using Object = Java.Lang.Object;


namespace FamiliaXamarin.PressureDevice
{
    public class BloodPressureData : Object
    {

        public float Sys { get; set; }
        public float Dia { get; set; }
        public float Pul { get; set; }
        public Calendar Data { get; set; }

        public BloodPressureData(float sys, float dia, float pul, Calendar data)
        {
            Sys = sys;
            Dia = dia;
            Pul = pul;
            Data = data;
        }

//        public int getSYS()
//        {
//            return Sys;
//        }
//
//        public int getDIA()
//        {
//            return Dia;
//        }
//
//        public int getPUL()
//        {
//            return Pul;
//        }
//
//        public Data getCAL()
//        {
//            return Data;
//        }

    }

}