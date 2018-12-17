using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Sharing
{
    class CustomDialogProfileSharingData : Dialog
    {
        private Activity activity;
       
        public CustomDialogProfileSharingData(Context context) : base(context)
        {
            this.activity = (Activity)context;
        }

       

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.profile_sharing_data);
            
            Window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
            
    
            // setupViews();

        }


       
    }
}