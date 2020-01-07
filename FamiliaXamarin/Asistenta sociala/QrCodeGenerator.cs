using Familia;
using Android.OS;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using System.Threading.Tasks;

namespace FamiliaXamarin {
    public class QrCodeGenerator : Android.Support.V4.App.Fragment {
        public override void OnCreate(Bundle savedInstanceState) => base.OnCreate(savedInstanceState);

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            var view = inflater.Inflate(Resource.Layout.fragment_qr_code_generator, container, false);

            var imageViewQrCode = view.FindViewById<ImageView>(Resource.Id.qrCode);
            LoadQRCode(imageViewQrCode);

            view.FindViewById<Button>(Resource.Id.btnReset).Click += delegate {
                LoadQRCode(imageViewQrCode);
            };
            return view;
        }

        private void LoadQRCode(ImageView imageViewQrCode) {
            var progressBarDialog =
                            new ProgressBarDialog(
                                "Va rugam asteptati", "Generare QR...", Activity, false);
            progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            progressBarDialog.Show();
            Task.Run(() => {
                var qrBitmap = Utils.GenQrCode();
                if (qrBitmap != null) {
                    Activity.RunOnUiThread(() => imageViewQrCode.SetImageBitmap(qrBitmap));
                }
                Activity.RunOnUiThread(progressBarDialog.Dismiss);
            });
        }
    }
}