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
using Fragment = Android.Support.V4.App.Fragment;

namespace Familia.OngBenefits.GenerateCardQR
{
    class GenerateCardQRFragment : Fragment
    {
        public static string KEY_GENERATE_CARD_QR_BENEFITS= "key generate card qr benefits";
        private View containerView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            containerView = inflater.Inflate(Resource.Layout.fragment_show_benefits, container, false);

            // TODO implement Scanare Buletine
            return containerView;
        }
    }
}