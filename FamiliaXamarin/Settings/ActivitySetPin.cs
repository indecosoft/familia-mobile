
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Goodiebag.Pinview;
using Familia.Helpers;

namespace Familia.Settings
{
    [Activity(Label = "ActivitySetPin")]
    public class ActivitySetPin : AppCompatActivity, Pinview.IPinViewEventListener
    {
        Pinview pin;
        private string _newPin;
        private TextView _textViewError;

        public void OnDataEntered(Pinview p0, bool p1)
        {
            if (string.IsNullOrEmpty(_newPin))
            {
                _newPin = p0.Value;
                ClearPin();
            }

            else
            {
                if (_newPin == p0.Value)
                {
                    Utils.SetDefaults("UserPin", p0.Value);
                    ClearPin();
                    Utils.HideKeyboard(this);
                    Finish();
                }
                else
                {
                    _textViewError.Text = "Pin incorect";
                    ClearPin();
                }
            }
        }

        private void ClearPin()
        {
            for (var i = 0; i < pin.PinLength; i++)
            {
                pin.OnKey(pin.FocusedChild, Keycode.Del, new KeyEvent(KeyEventActions.Up, Keycode.Del));
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
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
