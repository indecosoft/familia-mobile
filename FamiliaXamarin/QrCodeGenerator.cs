using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using ZXing.Common;

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
//            BitMatrix bitMatrix = Utils.GenQrCode(Activity);
//                int height = bitMatrix.Height;
//            int width = bitMatrix.Width;
//            Bitmap bmp = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
//            for (int x = 0; x < width; x++)
//            {
//                for (int y = 0; y < height; y++)
//                {
//                    bmp.SetPixel(x, y, bitMatrix[x, y] ? Color.Black : Color.White);
//                }
//            }
            imageViewQrCode.SetImageBitmap(Utils.GenQrCode(Activity));


            btnReset.Click += delegate
            {
                imageViewQrCode.SetImageBitmap(Utils.GenQrCode(Activity));
            };

            return view;
        }
    }
}