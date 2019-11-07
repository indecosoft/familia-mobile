
using System;
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
using Com.Goodiebag.Pinview;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;

namespace Familia.LoginSystem
{
    [Activity(Label = "PinActivity")]
    public class PinActivity : AppCompatActivity, Pinview.IPinViewEventListener
    {
        Pinview pin;
        private TextView _textViewError;
        public void OnDataEntered(Pinview p0, bool p1)
        {

            if(p0.Value == Utils.GetDefaults("UserPin"))
            {
                StartActivity(typeof(MainActivity));
                Utils.HideKeyboard(this);
                Finish();
            }
            else
            {
                _textViewError.Text = "Pin incorect";
                ClearPin();
            }
        }

        private void ClearPin()
        {
            for (int i = 0; i < pin.PinLength; i++)
            {
                pin.OnKey(pin.FocusedChild, Keycode.Del, new KeyEvent(KeyEventActions.Up, Keycode.Del));
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pin_lockscreen);
            pin = new Pinview(this);
            pin = FindViewById<Pinview>(Resource.Id.pinview);
            _textViewError = FindViewById<TextView>(Resource.Id.textview_error);
            pin.SetPinViewEventListener(this);
            //pin.SetPinBackgroundRes(R.drawable.sample_background);
            //pin.PinHeight = 40;
            //pin.PinWidth = 40;
            //pin.SetInputType(Pinview.InputType.Number);
            //pin.Value = "1234";
            //myLayout.addView(pin);
            // Create your application here
        }
    }
}
