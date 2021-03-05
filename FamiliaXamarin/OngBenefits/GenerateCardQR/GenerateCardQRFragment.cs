using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Familia.Helpers;
using Familia.OngBenefits.GenerateCardQR.Entities;
using Familia.OngBenefits.GenerateCardQR.OCR;
using Google.Android.Material.Snackbar;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using Org.Json;
using Console = System.Console;
using Environment = Android.OS.Environment;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Familia.OngBenefits.GenerateCardQR
{
    class GenerateCardQRFragment : Fragment
    {
        public static string KEY_GENERATE_CARD_QR_BENEFITS = "key generate card qr benefits";
        private View containerView;

        private CompoundButton autoFocus;
        private CompoundButton useFlash;
        private TextView statusMessage;
        private TextView textValue;
        private CardView cardViewData;
        private RelativeLayout rlActions;

        private TextView tvCnp;
        private TextView tvFirstName;
        private TextView tvLastName;
        private TextView tvNationality;
        private TextView tvBirthplace;
        private TextView tvHomeAddress;
        private TextView tvSeriesAndNumber;
        private TextView tvValidity;
        private TextView tvIssued;
        private PersonIdInfo _personIdInfo;
        private string _imageAbsolutePath;
        private static readonly int RC_OCR_CAPTURE = 9003;

        public GenerateCardQRFragment()
        {
        }

        public static GenerateCardQRFragment newInstance(string param1, string param2)
        {
            return new GenerateCardQRFragment();
        }

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

        private string GetBase64Img(string imagePath)
        {
            File imgFile = new File(imagePath);
            if (imgFile.Exists())
            {
                return "data:image/jpg;base64," +
                       Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath));
            }

            return null;
        }

        private async Task SendData()
        {
            if (_personIdInfo is null) return;
            await Task.Run(async () =>
            {
                JSONObject model = new JSONObject();
                model.Put("imei", Utils.GetDeviceIdentificator(Activity) ?? string.Empty);
                model.Put("cnp", _personIdInfo.Cnp);
                model.Put("nume", _personIdInfo.LastName);
                model.Put("prenume", _personIdInfo.FirstName);
                model.Put("cetatenie", _personIdInfo.Nationality);
                model.Put("locNastere", _personIdInfo.Birthplace);
                model.Put("domiciliu", _personIdInfo.HomeAddress);
                model.Put("eliberat", _personIdInfo.Issued);
                model.Put("seria", _personIdInfo.Series);
                model.Put("nr", _personIdInfo.Number);
                model.Put("valabilitate", _personIdInfo.Validity);
                // personalInfoJson.Put("imagine", GetBase64Img(_imageAbsolutePath));
                // model.Put("idClient", Utils.GetDefaults("IdClient"));
                // model.Put("data", personalInfoJson);
                // Log.Error("Payload", model.ToString());
                // Log.Error("Payload", GetBase64Img(_imageAbsolutePath));
                byte[] fileBytes = System.IO.File.ReadAllBytes(_imageAbsolutePath);
                MultipartFormDataContent form = new MultipartFormDataContent
                {
                    {new StringContent(Utils.GetDefaults("IdClient")), "idClient"},
                    {new StringContent(model.ToString()), "date"},
                    {new ByteArrayContent(fileBytes, 0, fileBytes.Length), "fisier", "CI.jpg"}
                };

                Log.Error("personalInfoJson", model.ToString());
                Log.Error("Request", form.ToString());
                string response =
                    await WebServices.WebServices.Post<MultipartFormDataContent>(
                        "https://asisocdev.indecosoft.net/scanare_pers_extern.php", form);
                string message;
                if (response is null)
                {
                    message = "A fost intampinata o eroare";
                }
                else
                {
                    var responseJson = new JSONObject(response);
                    message = responseJson.GetString(responseJson.Has("message") ? "message" : "error");
                }

                Activity.RunOnUiThread(() =>
                {
                    cardViewData.Visibility = ViewStates.Gone;
                    RelativeLayout.LayoutParams lp = (RelativeLayout.LayoutParams) rlActions.LayoutParameters;
                    lp?.RemoveRule(LayoutRules.AlignParentTop);
                    lp?.AddRule(LayoutRules.CenterInParent);
                    Snackbar.Make(rlActions, message,
                        Snackbar.LengthLong).Show();
                });
            });
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            initUI();
        }

        private void initUI()
        {
            Objects.RequireNonNull(Activity, "Context must not be null");
            statusMessage = Activity.FindViewById<TextView>(Resource.Id.status_message);
//        textValue = getActivity().findViewById(R.id.text_value);
            autoFocus = Activity.FindViewById<CompoundButton>(Resource.Id.auto_focus);
            useFlash = Activity.FindViewById<CompoundButton>(Resource.Id.use_flash);

            rlActions = Activity.FindViewById<RelativeLayout>(Resource.Id.rl_actions);

            tvCnp = Activity.FindViewById<TextView>(Resource.Id.tv_cnp_f);
            tvFirstName = Activity.FindViewById<TextView>(Resource.Id.tv_prenume_f);
            tvLastName = Activity.FindViewById<TextView>(Resource.Id.tv_nume_f);
            tvNationality = Activity.FindViewById<TextView>(Resource.Id.tv_cetatenie_f);
            tvBirthplace = Activity.FindViewById<TextView>(Resource.Id.tv_loc_nastere_f);
            tvHomeAddress = Activity.FindViewById<TextView>(Resource.Id.tv_domiciliu_f);
            tvSeriesAndNumber = Activity.FindViewById<TextView>(Resource.Id.tv_serie_nr_f);
            tvValidity = Activity.FindViewById<TextView>(Resource.Id.tv_valabilitate_f);
            tvIssued = Activity.FindViewById<TextView>(Resource.Id.tv_emisa_f);

            RelativeLayout.LayoutParams lp = (RelativeLayout.LayoutParams) rlActions.LayoutParameters;
            lp.AddRule(LayoutRules.CenterInParent);

            Activity.FindViewById(Resource.Id.cardViewScan).Click += OnScanCardClick;
            cardViewData = Activity.FindViewById<CardView>(Resource.Id.cardViewData);
            cardViewData.Visibility = ViewStates.Gone;
        }

        private void OnScanCardClick(object sender, EventArgs e)
        {
            ReadText();
        }

        private void ReadText()
        {
            Intent intent = new Intent(Activity, typeof(OcrCameraScanActivity));
            intent.PutExtra(OcrCameraScanActivity.AutoFocus, autoFocus.Checked);
            intent.PutExtra(OcrCameraScanActivity.UseFlash, useFlash.Checked);
            StartActivityForResult(intent, RC_OCR_CAPTURE);
        }

        private void DispatchTakePictureIntent()
        {
            Intent takePictureIntent = new Intent(MediaStore.ActionImageCapture);
            // Ensure that there's a camera activity to handle the intent
            if (takePictureIntent.ResolveActivity(Activity.PackageManager) != null)
            {
                // Create the File where the photo should go
                File photoFile = null;
                try
                {
                    photoFile = CreateImageFile();
                }
                catch (IOException ex)
                {
                    // Error occurred while creating the File
                }

                // Continue only if the File was successfully created
                if (photoFile != null)
                {
                    try
                    {
                        Android.Net.Uri photoURI =
                            FileProvider.GetUriForFile(Activity, "IndecoSoft.Familia.fileprovider", photoFile);

                        takePictureIntent.PutExtra(MediaStore.ExtraOutput, photoURI);
                        StartActivityForResult(takePictureIntent, 10001);
                    }
                    catch (Exception e)
                    {
                        Log.Error("AAAAAAA", "error " + e.Message);
                    }
                }
            }
        }

        private File CreateImageFile()
        {
            // Create an image file name
            string timeStamp = "test";
            string imageFileName = "JPEG_" + timeStamp + "_";
            File storageDir = Activity.GetExternalFilesDir(Environment.DirectoryPictures);
            File image = File.CreateTempFile(
                imageFileName, /* prefix */
                ".jpg", /* suffix */
                storageDir /* directory */
            );

            // Save a file: path for use with ACTION_VIEW intents
            // Storage.getInstance().setImagePath(image.getAbsolutePath());
            _imageAbsolutePath = image.AbsolutePath;
            return image;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == RC_OCR_CAPTURE)
            {
                Log.Error("Scanned finished", "done");
                if (data != null)
                {
                    string serializedData = data.GetStringExtra("Result");
                    if (!string.IsNullOrEmpty(serializedData))
                    {
                        PersonIdInfo info = JsonConvert.DeserializeObject<PersonIdInfo>(serializedData);
                        _personIdInfo = info;
                        tvCnp.Text = $"{GetString(Resource.String.cnp)} {info.Cnp}";
                        tvFirstName.Text = $"{GetString(Resource.String.prenume)} {info.FirstName}";
                        tvLastName.Text = $"{GetString(Resource.String.nume)} {info.LastName}";
                        tvNationality.Text = $"{GetString(Resource.String.cetatenie)} {info.Nationality}";
                        tvBirthplace.Text = $"{GetString(Resource.String.loc_nastere)} {info.Birthplace}";
                        tvHomeAddress.Text = $"{GetString(Resource.String.domiciliu)} {info.HomeAddress}";
                        tvSeriesAndNumber.Text = $"{GetString(Resource.String.seria_nr)} {info.Series}/{info.Number}";
                        tvValidity.Text = $"{GetString(Resource.String.valabilitate)} {info.Validity}";
                        tvIssued.Text = $"{GetString(Resource.String.emisa_de)} {info.Issued}";

                        RelativeLayout.LayoutParams lp = (RelativeLayout.LayoutParams) rlActions.LayoutParameters;
                        lp.AddRule(LayoutRules.AlignParentTop);

                        cardViewData.Visibility = ViewStates.Visible;

                        DispatchTakePictureIntent();
                    }
                }
                else
                {
                    statusMessage.Text = GetString(Resource.String.ocr_error);
                }
            }
            else if (requestCode == 10001)
            {
                Log.Error("From Cammera", _imageAbsolutePath);
                _ = SendData();
            }
        }
    }
}