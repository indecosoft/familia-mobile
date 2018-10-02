using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Org.Json;
using String = System.String;
using Toolbar = Android.Widget.Toolbar;

namespace FamiliaXamarin
{
    [Activity(Label = "ChatActivity")]
    public class ChatActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
                  SetContentView(Resource.Layout.activity_chat);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            if (savedInstanceState == null)
            {
                Bundle extras = Intent.Extras;
                if (extras == null)
                {

                }
                else if (extras.GetBoolean("AcceptClick"))
                {
                    var emailFrom = Utils.GetDefaults("Email", this);
                    string dest = null;
                    try
                    {
                        dest = extras.GetString("EmailFrom");
                        JSONObject mailObject = new JSONObject().Put("dest", dest).Put("from", emailFrom).Put("accepted", true);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                        //finish();
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }


                    //Log.Error("roomsArray", WebSoketService.rooms.toString());
                    //active = true;
                    //setContentView(R.layout.activity_chat);
                    //Toolbar toolbar = findViewById(R.id.toolbar);
                    //setSupportActionBar(toolbar);


                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    toolbar.NavigationClick += delegate { Finish(); };
          
                //finish();
            } else if (extras.GetBoolean("RejectClick")) {
                var EmailFrom = Utils.GetDefaults("Email",this);
                try {

                    JSONObject mailObject = new JSONObject().Put("from", extras.GetString("EmailFrom")).Put("dest", EmailFrom).Put("accepted", false);
    Log.Error("aici", mailObject.ToString());
                    WebSocketClient.Client.Emit("chat accepted", mailObject);
                    //finish();
                } catch (JSONException e) {
                    e.PrintStackTrace();
                }
                Finish();
            } else if (extras.GetBoolean("Conv")) {

                
            }
        }


            // Create your application here
        }
    }
}