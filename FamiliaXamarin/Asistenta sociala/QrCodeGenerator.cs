using Familia;
using Android.OS;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin
{
    public class QrCodeGenerator : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.fragment_qr_code_generator, container, false);
            var btnReset = view.FindViewById<Button>(Resource.Id.btnReset);
            var imageViewQrCode = view.FindViewById< ImageView>(Resource.Id.qrCode);
            imageViewQrCode.SetImageBitmap(Utils.GenQrCode(Activity));

            btnReset.Click += delegate
            {
                imageViewQrCode.SetImageBitmap(Utils.GenQrCode(Activity));
            };

            return view;
        }
    }
}