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
using Java.Util;

namespace FamiliaXamarin
{
    class Constants
    {
        public static readonly string BLOOD_PRESSURE_FILE = "blood_pressure_data.txt";
        public static readonly string BLOOD_GLUCOSE_FILE = "blood_glucose_data.txt";


        public static readonly UUID UUID_BLOOD_PRESSURE_SERVICE = UUID.FromString("00001810-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_BLOOD_PRESSURE_MEASUREMENT_CHAR = UUID.FromString("00002a35-0000-1000-8000-00805f9b34fb");
        public static readonly UUID CLIENT_CHARACTERISTIC_CONFIG = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
        //public static final UUID UUID_GLUC_FEATURE = UUID.fromString("00002a51-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_GLUC_MEASUREMENT_CHAR = UUID.FromString("00002a18-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_GLUC_MEASUREMENT_CONTEXT_CHAR = UUID.FromString("00002a34-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_GLUC_RECORD_ACCESS_CONTROL_POINT_CHAR = UUID.FromString("00002a52-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_GLUC_SERV = UUID.FromString("00001808-0000-1000-8000-00805f9b34fb");

        public static readonly UUID TRANSFER_WEARABLE_CONTROL_UUID = UUID.FromString("0a0ae00a-0a00-1000-8000-00805f9b34fb");
        public static readonly UUID TRANSFER_WEARABLE_EKG_UUID = UUID.FromString("0a0ae00c-0a00-1000-8000-00805f9b34fb");
        public static readonly UUID TRANSFER_WEARABLE_ACTIVITY_UUID = UUID.FromString("0a0ae00d-0a00-1000-8000-00805f9b34fb");
        public static readonly UUID TRANSFER_WEARABLE_SLEEPCONTENT_UUID = UUID.FromString("0a0ae00b-0a00-1000-8000-00805f9b34fb");

        public static readonly string DATA_URL = "https://devgis.sigma.team/devices/save-device-measurements";
        public static readonly string SERVER_ADDRESS="http://192.168.101.192:3000/";
        public static readonly string PUBLIC_SERVER_ADDRESS="https://chat.devgis.sigma.team/";
        public static readonly string IMAGE_DIRECTORY = "/demonuts";
        public static readonly int RequestGallery = 2;
        public static readonly int RequestCamera = 1;

        //    public static readonly string SERVER_ADDRESS="http://192.168.101.140:3000/";
    }
}