using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

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
            containerView = inflater.Inflate(Resource.Layout.fragment_generate_qr_card, container, false);

            // TODO implement Scanare Buletine
            return containerView;
        }
    }
}