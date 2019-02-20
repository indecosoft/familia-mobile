using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Views;
using Android.Widget;
using Refractored.Controls;

namespace FamiliaXamarin.Sharing
{
    class CustomDialogProfileSharingData : Dialog
    {
        private Activity _activity;
        public TextView Name;
        public CircleImageView Image;
        public Button ButtonConfirm;
        public Button ButtonCancel;
        private string conflict = "aaa";
       
        public CustomDialogProfileSharingData(Context context) : base(context)
        {
            _activity = (Activity)context;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.profile_sharing_data);
            Name = FindViewById<TextView>(Resource.Id.tv_person_name);
            Image = FindViewById<CircleImageView>(Resource.Id.round_image);
            ButtonConfirm = FindViewById<Button>(Resource.Id.btn_confirm);
            ButtonCancel = FindViewById<Button>(Resource.Id.btn_cancel);
            Window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
            var bg = FindViewById<ConstraintLayout>(Resource.Id.bg);
            bg.Click += delegate(object sender, EventArgs args)
            {
                Dismiss();
            };
        }
    }
}