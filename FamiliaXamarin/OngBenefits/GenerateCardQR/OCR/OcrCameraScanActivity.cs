using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Familia.OngBenefits.GenerateCardQR.Camera;
using Familia.OngBenefits.GenerateCardQR.Entities;
using Java.Interop;
using Newtonsoft.Json;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace Familia.OngBenefits.GenerateCardQR.OCR
{
    [Activity(Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class OcrCameraScanActivity : AppCompatActivity, Detector.IProcessor
    {
        public static readonly string AutoFocus = "AutoFocus";
        public static readonly string UseFlash = "UseFlash";
        private CameraSourcePreview cameraView;
        private TextView tvCnp;
        private TextView tvFirstName;
        private TextView tvLastName;
        private TextView tvNationality;
        private TextView tvBirthplace;
        private TextView tvHomeAddress;
        private TextView tvSeriesAndNumber;
        private TextView tvValidity;
        private TextView tvIssued;
        private CameraSource _cameraSource;
        private const int RequestCameraPermissionId = 1001;
        private readonly StringComputations ocrStringComputations = new StringComputations();
        private File _dir;
        private Uri _photoUri;
        private string _imageExtension, _imagePath;



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case RequestCameraPermissionId:
                    if (grantResults[0] == Permission.Granted)
                    {
                        cameraView.Start(_cameraSource);
                    }

                    break;
                default:
                    return;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_ocr_capture);
            cameraView = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            tvCnp = FindViewById<TextView>(Resource.Id.tv_cnp);
            tvFirstName = FindViewById<TextView>(Resource.Id.tv_prenume);
            tvLastName = FindViewById<TextView>(Resource.Id.tv_nume);
            tvNationality = FindViewById<TextView>(Resource.Id.tv_cetatenie);
            tvBirthplace = FindViewById<TextView>(Resource.Id.tv_loc_nastere);
            tvHomeAddress = FindViewById<TextView>(Resource.Id.tv_domiciliu);
            tvSeriesAndNumber = FindViewById<TextView>(Resource.Id.tv_serie_nr);
            tvValidity = FindViewById<TextView>(Resource.Id.tv_valabilitate);
            tvIssued = FindViewById<TextView>(Resource.Id.tv_emisa);
            bool autoFocus = Intent.GetBooleanExtra(AutoFocus, false);
            bool useFocus = Intent.GetBooleanExtra(UseFlash, false);
            TextRecognizer textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!textRecognizer.IsOperational)
            {
                Log.Error("OcrCameraScanActivity", "Detector dependencies are not yet available!");
            }
            else
            {
                _cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(autoFocus)
                    
                    .Build();
                // _cameraSource.F
                cameraView.Start(_cameraSource);
                textRecognizer.SetProcessor(this);
                if (useFocus)
                {
                    
                    var _myCamera =  cameraView.GetCamera();
                    if (_myCamera != null)
                    {
                        var prams = _myCamera.GetParameters();
                        //prams.focus.setFocusMode(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE);
                        prams.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                        _myCamera.SetParameters(prams);
                    }
                    else
                    {
                        Log.Error("Error", "Camera null");
                    }
                    
                }
                
            }

            ocrStringComputations.ScanningCompleted += OcrStringComputationsOnScanningCompleted;
        }
        
        private void OcrStringComputationsOnScanningCompleted(object source, PersonIdInfo args)
        {
            ocrStringComputations.ScanningCompleted -= OcrStringComputationsOnScanningCompleted;
            Log.Error("Scanned data", args.ToString());
            tvCnp.Text = $"{GetString(Resource.String.cnp)} {args.Cnp}";
            tvFirstName.Text = $"{GetString(Resource.String.prenume)} {args.FirstName}";
            tvLastName.Text = $"{GetString(Resource.String.nume)} {args.LastName}";
            tvNationality.Text = $"{GetString(Resource.String.cetatenie)} {args.Nationality}";
            tvBirthplace.Text = $"{GetString(Resource.String.loc_nastere)} {args.Birthplace}";
            tvHomeAddress.Text = $"{GetString(Resource.String.domiciliu)} {args.HomeAddress}";
            tvSeriesAndNumber.Text = $"{GetString(Resource.String.seria_nr)} {args.Series}/{args.Number}";
            tvValidity.Text = $"{GetString(Resource.String.valabilitate)} {args.Validity}";
            tvIssued.Text = $"{GetString(Resource.String.emisa_de)} {args.Issued}";

            Intent data = new Intent();
            data.PutExtra("Result", JsonConvert.SerializeObject (args));
            SetResult(Result.Ok, data);
            Finish();
        }


        public void ReceiveDetections(Detector.Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if (items.Size() > 0)
            {
                for (int i = 0; i < items.Size(); i++)
                {
                    TextBlock item = (TextBlock) items.ValueAt(i);
                    ocrStringComputations.RetrieveInfo(item);
                }
            }
        }


        public void Release()
        {
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            cameraView?.Start(_cameraSource);
        }

        protected override void OnPause()
        {
            base.OnPause();
            cameraView?.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            cameraView?.Release();
        }
    }
}