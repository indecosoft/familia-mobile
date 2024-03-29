﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Familia.Medicatie.Entities;

namespace Familia.Medicatie
{
    class CustomDialogDeleteDisease : Dialog, View.IOnClickListener
    {
        private Button _btnDa;
        private Button _btnNu;
        private ICustomDialogDeleteDiseaseListener _listener;
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
        public void SetListener(ICustomDialogDeleteDiseaseListener listener)
        {
            _listener = listener;
        }

        public void SetBoala(Disease boala)
        {
            _boala = boala;
        }

        public CustomDialogDeleteDisease(Context context) : base(context)
        {
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_da_delete_boala:
                    _listener.OnYesClicked("yes", _boala);
                    Dismiss();
                    break;
                case Resource.Id.btn_nu_delete_boala:
                    Dismiss();
                    break;

            }
        }
        public interface ICustomDialogDeleteDiseaseListener
        {
            void OnYesClicked(string result, Disease boala);
        }
    }
}