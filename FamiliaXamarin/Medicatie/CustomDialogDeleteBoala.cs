using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class CustomDialogDeleteBoala : Dialog, View.IOnClickListener
    {
        private Button _btnDa;
        private Button _btnNu;
        private ICustomDialogDeleteBoalaListener _listener;
        private Disease _boala;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.custom_dialog_delete_boala);

            _btnDa = FindViewById<Button>(Resource.Id.btn_da_delete_boala);
            _btnNu = FindViewById<Button>(Resource.Id.btn_nu_delete_boala);
            _btnDa.SetOnClickListener(this);
            _btnNu.SetOnClickListener(this);
        }
        public void SetListener(ICustomDialogDeleteBoalaListener listener)
        {
            _listener = listener;
        }

        public void SetBoala(Disease boala)
        {
            _boala = boala;
        }

        public CustomDialogDeleteBoala(Context context) : base(context)
        {
        
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_da_delete_boala:
                    _listener.OnYesClicked("yes", this._boala);
                    Dismiss();
                    break;
                case Resource.Id.btn_nu_delete_boala:
                    Dismiss();
                    break;

            }
        }
        public interface ICustomDialogDeleteBoalaListener
        {
            void OnYesClicked(string result, Disease boala);
        }
    }
}