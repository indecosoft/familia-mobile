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
using Familia;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class CustomDialogDeleteMedicament :Dialog, View.IOnClickListener
    {
        private Button btnDa;
        private Button btnNu;
        private ICustomDialogDeleteMedicamentListener listener;
        private Medicine medicament;

        public CustomDialogDeleteMedicament(Context context) : base(context)
        {
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.custom_dialog_delete_medicament);

            btnDa = FindViewById<Button>(Resource.Id.btn_da_delete_medicament);
            btnNu = FindViewById<Button>(Resource.Id.btn_nu_delete_medicament);
            btnDa.SetOnClickListener(this);
            btnNu.SetOnClickListener(this);
        }
        public void setListener(ICustomDialogDeleteMedicamentListener listener)
        {
            this.listener = listener;
        }

        public void setMedicament(Medicine medicament)
        {
            this.medicament = medicament;
        }

        public interface ICustomDialogDeleteMedicamentListener
        {
            void onYesClicked(string result, Medicine medicament);
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_da_delete_medicament:
                    listener.onYesClicked("yes", this.medicament);
                    Dismiss();
                    break;
                case Resource.Id.btn_nu_delete_medicament:
                    Dismiss();
                    break;
            }
        }
    }
}