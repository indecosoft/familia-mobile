using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Refractored.Controls;
using String = System.String;

namespace FamiliaXamarin.SmartBand
{
    [Activity(Label = "AddSmartBandDeviceActivity")]
    public class AddSmartBandDeviceActivity : AppCompatActivity
    {
        

        private string _url;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Create your application here
        }
        
    }
}