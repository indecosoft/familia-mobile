using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Familia.Asistenta_sociala;
using Familia.Helpers;
using Familia.JsonModels;
using Familia.Medicatie.Alarm;
using Familia.Profile;
using Familia.Profile.Data;
using Familia.Services;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using Org.Json;
using Refractored.Controls;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Uri = Android.Net.Uri;

namespace Familia.Login_System
{
    [Activity(Label = "FirstSetup", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class FirstSetup : FragmentActivity
    {
        private File _dir;
        private SectionsPagerAdapter _sectionsPagerAdapter;
        private FirstSetupViewPager _viewPager;

        private readonly FirstSetupModel _firstSetupModel = new FirstSetupModel();

        private ConstraintLayout _mainContent;
        private static FirstSetup FragmentContext;
        private ProgressBarDialog _progressBarDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_first_setup);
            InitUi();
        }

        private void InitUi()
        {
            FragmentContext = this;
            _sectionsPagerAdapter = new SectionsPagerAdapter(SupportFragmentManager);
            _mainContent = FindViewById<ConstraintLayout>(Resource.Id.main_content);
            _viewPager = FindViewById<FirstSetupViewPager>(Resource.Id.container);
            _viewPager.SetPagingEnabled(false);
            _viewPager.Adapter = _sectionsPagerAdapter;
            _progressBarDialog =
                new ProgressBarDialog(
                    "Va rugam asteptati",
                    "Se trimit datele",
                    this, false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_first_setup, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            return base.OnOptionsItemSelected(item);
        }

        public class PlaceholderFragment : Fragment, View.IOnTouchListener
        {
            private const string ArgSectionNumber = "section_number";
            private Button _btnNext;
            private Button _btnBack;
            private Button _btnUpload;
            //private Button _btnDate;
            private Button _btnDate;
            //            private Spinner _genderSpinner;
            private Button _diseasaesButton;
            private ToggleButton _maleToggleButton;
            private ToggleButton _femaleToggleButton;
            private CircleImageView _profileImage;
            private bool _imageValidator;
            private Uri _photoUri;
            private FileInfo _fileInformations;
            private string _imageExtension, _imagePath;
            private List<SearchListModel> _selectedDiseases = new List<SearchListModel>();
            private List<DiseaseModel> _diseaseList;

            private List<PersonalDisease> listOfPersonalDiseases = new List<PersonalDisease>();


            public static PlaceholderFragment NewInstance(int sectionNumber)
            {
                var fragment = new PlaceholderFragment();
                var args = new Bundle();
                args.PutInt(ArgSectionNumber, sectionNumber);
                fragment.Arguments = args;
                return fragment;
            }

            private void ShowPictureDialog()
            {
                var pictureDialog = new AlertDialog.Builder(FragmentContext, Resource.Style.AppTheme_Dialog);
                pictureDialog.SetTitle("Incarcati o imagine");
                string[] pictureDialogItems =
                {
                    "Alegeti din galerie",
                    "Faceti una acum"
                };
                pictureDialog.SetItems(pictureDialogItems,
                    delegate (object sender, DialogClickEventArgs args)
                    {
                        Contract.Requires(sender != null);
                        switch (args.Which)
                        {
                            case 0:
                                ChoosePhotoFromGallary();
                                break;
                            case 1:
                                TakePhotoFromCamera();
                                break;
                        }
                    });
                pictureDialog.Show();
            }

            private void ChoosePhotoFromGallary()
            {
                var a = new Intent();
                a.SetType("image/*");
                a.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(a, "Selectati imagine de profil"),
                    Constants.RequestGallery);
            }

            private static void CreateDirectoryForPictures()
            {
                FragmentContext._dir = new File(
                    Environment.GetExternalStoragePublicDirectory(
                        Environment.DirectoryPictures), "Familia");
                if (!FragmentContext._dir.Exists())
                {
                    FragmentContext._dir.Mkdirs();
                }
            }

            private bool IsThereAnAppToTakePictures()
            {
                var intent = new Intent(MediaStore.ActionImageCapture);
                var availableActivities =
                    Activity.PackageManager.QueryIntentActivities(intent,
                        PackageInfoFlags.MatchDefaultOnly);
                return availableActivities != null && availableActivities.Count > 0;
            }

            private File CreateImageFile()
            {
                // Create an image file name
                string timeStamp = new SimpleDateFormat("yyyy.MM.dd_HH:mm").Format(new Date());
                string imageFileName = "Avatar_" + timeStamp + "";
                File storageDir = Activity.GetExternalFilesDir(Environment.DirectoryPictures);
                //File storageDir = new File(Environment.GetExternalStoragePublicDirectory(
                //Environment.DirectoryDcim), "Camera");
                var image = File.CreateTempFile(
                    imageFileName, /* prefix */
                    ".jpg", /* suffix */
                    storageDir /* directory */
                );

                // Save a file: path for use with ACTION_VIEW intents
                _imagePath = image.AbsolutePath;
                return image;
            }

            private void TakePhotoFromCamera()
            {
                if (!IsThereAnAppToTakePictures()) return;
                CreateDirectoryForPictures();

                var intent = new Intent(MediaStore.ActionImageCapture);
                //App._file = new File(App._dir, "Avatar [" + DateTime.Now + "].jpg");
                //App._file = new File(App._dir, "Avatar [" + DateTime.Now + "].jpg");
                _photoUri = FileProvider.GetUriForFile(Activity,
                    "IndecoSoft.Familia.fileprovider",
                    CreateImageFile());

                intent.PutExtra(MediaStore.ExtraOutput, _photoUri);
                StartActivityForResult(intent, Constants.RequestCamera);
            }

            private static string GetPathToImage(Uri uri)
            {
                string docId;
                using (ICursor c1 = FragmentContext.ContentResolver.Query(uri,
                    null, null, null, null))
                {
                    c1.MoveToFirst();
                    string documentId = c1.GetString(0);
                    docId = documentId.Substring(
                        documentId.LastIndexOf(":", StringComparison.Ordinal) + 1);
                }

                string path;

                // The projection contains the columns we want to return in our query.
                const string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
                using (ICursor cursor = FragmentContext.ContentResolver.Query(
                    MediaStore.Images.Media.ExternalContentUri, null, selection, new[] { docId },
                    null))
                {
                    if (cursor == null) return null;
                    int columnIndex =
                        cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(columnIndex);
                }

                return path;
            }

            public override void OnActivityResult(int requestCode, int resultCode, Intent data)
            {
                if (resultCode == (int)Result.Ok)
                {
                    switch (requestCode)
                    {
                        case 1:
                            //_imagePath = _photoUri.Path;
                            Glide.With(this).Load(new File(_imagePath)).Into(_profileImage);

                            GalleryAddPic();
                            _fileInformations = new FileInfo(new File(_imagePath).Path);
                            Log.Error("Size", _fileInformations.Length.ToString());
                            if (_fileInformations.Length >= 10485760)
                            {
                                _imageValidator = false;
                                ImageTooLargeWarning();
                            }
                            else
                            {
                                _imageValidator = true;
                            }

                            break;
                        case 2:
                            Uri uri = data.Data;


                            _imagePath = GetPathToImage(uri);
                            Glide.With(this).Load(new File(_imagePath)).Into(_profileImage);
                            _fileInformations = new FileInfo(_imagePath);
                            Log.Error("Size", _fileInformations.Length.ToString());
                            if (_fileInformations.Length >= 10485760)
                            {
                                _imageValidator = false;
                                ImageTooLargeWarning();
                            }
                            else
                            {
                                _imageValidator = true;
                            }

                            _imageValidator = true;
                            break;
                        case 3:

                                _selectedDiseases = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));
                                Log.Error("Avem result", data.GetStringExtra("result"));
                            _diseasaesButton.Text = $"Ati Selectat {_selectedDiseases.Count} afecțiuni";
                   
                            break;
                        default:
                            _imageValidator = false;
                            break;
                    }

                    if (!_imageValidator) return;
                    _imageExtension =
                        _imagePath.Substring(_imagePath.LastIndexOf(".", StringComparison.Ordinal) +
                                             1);
                    if (_imageExtension.ToLower().Equals("jpeg"))
                        _imageExtension = "jpg";
                    FragmentContext._firstSetupModel.Base64Image =
                        "data:image/" + _imageExtension + ";base64," +
                        Convert.ToBase64String(System.IO.File.ReadAllBytes(_imagePath));
                    FragmentContext._firstSetupModel.ImageExtension = _imageExtension;
                }
                else
                {
                    Toast.MakeText(FragmentContext, "Alege o imagine", ToastLength.Short).Show();
                }
            }

            private void ImageTooLargeWarning()
            {
                Toast.MakeText(FragmentContext,
                    "Fotografie prea mare! Dimensiunea maxima acceptata este de 10 Mb.",
                    ToastLength.Long).Show();
                var resourcePath =
                    "@drawable/profile"; // where myresource (without the extension) is the file

                int imageResource =
                    Activity.Resources.GetIdentifier(resourcePath, null, Activity.PackageName);


                //Drawable res = Activity.Resources.GetDrawable(imageResource);
                Drawable res = ContextCompat.GetDrawable(Activity, imageResource);
                _profileImage.SetImageDrawable(res);
            }

            private void GalleryAddPic()
            {
                var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                var f = new File(_photoUri.Path);
                Uri contentUri = Uri.FromFile(f);
                mediaScanIntent.SetData(contentUri);
                FragmentContext.SendBroadcast(mediaScanIntent);
            }

            private void InitFirstViewUi(View v)
            {
                _btnUpload = v.FindViewById<Button>(Resource.Id.btnUpload);
                _profileImage = v.FindViewById<CircleImageView>(Resource.Id.ProfileImage);
            }

            private void InitSecondViewUi(View v)
            {
                //_genderSpinner = v.FindViewById<Spinner>(Resource.Id.gender_spinner);
                _btnDate = v.FindViewById<Button>(Resource.Id.btnDate);

                //_btnDate = v.FindViewById<Button>(Resource.Id.btnDate);
            }

            private void IniThirdViewUi(View v)
            {
                _diseasaesButton = v.FindViewById<Button>(Resource.Id.diseases_button);
                _diseasaesButton.Text = "Selectati Afectiuni";
            }

            private void InitDefaultUi(View v)
            {
                _btnBack = v.FindViewById<Button>(Resource.Id.btnBack);
                _btnNext = v.FindViewById<Button>(Resource.Id.btnNext);
                _maleToggleButton = v.FindViewById<ToggleButton>(Resource.Id.male_toggle);
                _femaleToggleButton = v.FindViewById<ToggleButton>(Resource.Id.female_toggle);
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container,
                Bundle savedInstanceState)
            {
                View rootView = null;
                try
                {
                    int index = Arguments.GetInt(ArgSectionNumber) - 1;
                    switch (index)
                    {
                        case 0:
                            rootView = inflater.Inflate(Resource.Layout.fragment_setup1, container,
                                false);
                            InitDefaultUi(rootView);
                            InitFirstViewUi(rootView);
                            _btnUpload.Click += delegate
                            {
                                ShowPictureDialog();
                            };

                            break;
                        case 1:
                            rootView = inflater.Inflate(Resource.Layout.fragment_setup2, container,
                                false);
                            InitDefaultUi(rootView);
                            InitSecondViewUi(rootView);

                            _maleToggleButton.CheckedChange += delegate
                            {
                                if (!_maleToggleButton.Checked) return;
                                _femaleToggleButton.Checked = false;
                                FragmentContext._firstSetupModel.Gender = "Masculin";
                            };
                            _femaleToggleButton.CheckedChange += delegate
                            {
                                if (!_femaleToggleButton.Checked) return;
                                _maleToggleButton.Checked = false;
                                FragmentContext._firstSetupModel.Gender = "Feminin";
                            };

                            _btnDate.Click += delegate
                            {

                                var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                                {
                                    _btnDate.Text = time.ToShortDateString();
                                    FragmentContext._firstSetupModel.DateOfBirth =
                                        time.ToString("yyyy-MM-dd");
                                });
                                frag.Show(FragmentContext.SupportFragmentManager,
                                    DatePickerFragment.TAG);
                            };

                            break;
                        case 2:
                            rootView = inflater.Inflate(Resource.Layout.fragment_setup3, container,
                                false);
                            InitDefaultUi(rootView);
                            IniThirdViewUi(rootView);
                            // Create an ArrayAdapter using the string array and a default spinner layout
                            //FragmentContext._firstSetupModel.Disease = new JSONArray();
                            _diseasaesButton.Click += GetDiseaseList;
                            break;
                            //default:
                            //rootView = inflater.Inflate(Resource.Layout.fragment_setup1, container,
                            //    false);
                            //InitDefaultUi(rootView);
                            //InitFirstViewUi(rootView);
                            //_btnUpload.Click += delegate {
                            //    ShowPictureDialog();
                            //};
                            //break;
                    }

                    _btnNext.Click += _btnNext_Click;
                    if (Arguments.GetInt(ArgSectionNumber) != 1)
                    {
                        _btnBack.Click += delegate
                        {
                            FragmentContext._viewPager.CurrentItem =
                                Arguments.GetInt(ArgSectionNumber) - 2;
                        };
                    }
                    if (Arguments.GetInt(ArgSectionNumber) == 3)
                        _btnNext.Text = "Gata";
                }
                catch (Exception ex)
                {
                    Log.Error("FirstSetup err: ", ex.Message);
                }



                return rootView;
            }


            private void GetDiseaseList(object sender, EventArgs e)
            {
                var progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se aduc date...", Activity, false);
                progressBarDialog.Show();
                Task.Run(async () =>
                {
                    string result = await WebServices.WebServices.Get(Constants.PublicServerAddress + "/api/getDisease", Utils.GetDefaults("Token"));
                    if (result == null) return;

                    var arrayOfDiseases = new JSONArray(result);
                    var items = new List<SearchListModel>();
                    for (var i = 0; i < arrayOfDiseases.Length(); i++)
                    {
                        var jsonModel = new JSONObject(arrayOfDiseases.Get(i).ToString());

                        items.Add(new SearchListModel
                        {
                            Id = jsonModel.GetInt("cod"),
                            Title = jsonModel.GetString("denumire")

                        });
                    }

                    var intent = new Intent(Activity, typeof(SearchListActivity));
                    intent.PutExtra("Items", JsonConvert.SerializeObject(items));
                    intent.PutExtra("SelectedItems", JsonConvert.SerializeObject(_selectedDiseases));
                    StartActivityForResult(intent, 3);

                    Activity.RunOnUiThread(progressBarDialog.Dismiss);
                });
            }

            private async void _btnNext_Click(object sender, EventArgs e)
            {
                switch (Arguments.GetInt(ArgSectionNumber))
                {
                    case 1:
                        if (_imageValidator)
                            FragmentContext._viewPager.CurrentItem =
                                Arguments.GetInt(ArgSectionNumber);
                        else
                            Toast.MakeText(FragmentContext, "Alegeti o imagine!", ToastLength.Short)
                                .Show();
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(FragmentContext._firstSetupModel.Gender) && !string.IsNullOrEmpty(FragmentContext._firstSetupModel.DateOfBirth))
                            FragmentContext._viewPager.CurrentItem = Arguments.GetInt(ArgSectionNumber);
                        else
                            Toast.MakeText(FragmentContext, "Va rugam sa completati formularul", ToastLength.Short)
                                .Show();
                        break;
                    case 3:
                        FragmentContext._progressBarDialog.Show();
                        FragmentContext._firstSetupModel.ImageName =
                            Utils.GetDefaults("Email");


                        foreach (SearchListModel el in _selectedDiseases)
                        {
                            if (!el.IsSelected) continue;
                            FragmentContext._firstSetupModel.Disease.Add(el.Id);
                            listOfPersonalDiseases.Add(new PersonalDisease(el.Id, el.Title));
                        }
                        await Task.Run(async () =>
                        {
                            string jsonData =
                                JsonConvert.SerializeObject(FragmentContext._firstSetupModel);

                            Log.Error("data to send", jsonData);

                            string response = await WebServices.WebServices.Post(
                                Constants.PublicServerAddress + "/api/firstSetup",
                                new JSONObject(jsonData), Utils.GetDefaults("Token"));
                            if (response != null)
                            {
                                Snackbar snack;
                                var responseJson = new JSONObject(response);
                                switch (responseJson.GetInt("status"))
                                {
                                    case 0:
                                        snack = Snackbar.Make(FragmentContext._mainContent,
                                            "Wrong Data", Snackbar.LengthLong);
                                        snack.Show();
                                        break;
                                    case 1:
                                        snack = Snackbar.Make(FragmentContext._mainContent,
                                            "Internal Server Error", Snackbar.LengthLong);
                                        snack.Show();
                                        break;
                                    case 2:

                                        

                                        Utils.SetDefaults("Logins", true.ToString());
                                        Utils.SetDefaults("Avatar",
                                            $"{Constants.PublicServerAddress}/{Utils.GetDefaults("Email")}.{FragmentContext._firstSetupModel.ImageExtension}");


                                        await SaveProfileData();


                                        createSimpleChannelForServices();
                                        createNonstopChannelForServices();

                                        if (int.Parse(Utils.GetDefaults("UserType")) == 3)
                                        {
                                            var _medicationServerServiceIntent = new Intent(Application.Context, typeof(MedicationServerService));
                                            Activity.StartService(_medicationServerServiceIntent);
                                        }

                                        FragmentContext.StartActivity(typeof(MainActivity));
                                        FragmentContext.Finish();
                                        break;
                                }

                            }
                            else
                            {
                                var snack = Snackbar.Make(FragmentContext._mainContent,
                                    "Unable to reach the server!", Snackbar.LengthLong);
                                snack.Show();
                            }
                        });
                        FragmentContext._progressBarDialog.Dismiss();

                        break;
                }
            }

          

            private async Task SaveProfileData()
            {
                var personalData = new PersonalData(
                    listOfPersonalDiseases,
                    FragmentContext._firstSetupModel.Base64Image,
                    FragmentContext._firstSetupModel.DateOfBirth,
                    FragmentContext._firstSetupModel.Gender,
                    FragmentContext._firstSetupModel.ImageName,
                    FragmentContext._firstSetupModel.ImageExtension
                );
                if (!(await ProfileStorage.GetInstance().clearDb()))
                {
                    Log.Error("FirstSetup", "Error clearing profile data ..");
                }
                else
                {
                    ProfileStorage.GetInstance().personalData = personalData;
                    if (!(await ProfileStorage.GetInstance().save()))
                    {
                        Log.Error("FirstSetup", "Error saving profile data ..");
                    }
                }
               
            }

            private void createSimpleChannelForServices()
            {
                var channel = new NotificationChannel(App.SimpleChannelIdForServices, "Test simple channel",
                    NotificationImportance.Default);
                ((NotificationManager)Activity.GetSystemService(NotificationService)).CreateNotificationChannel(channel);
                Log.Error("App CreateChannel", "Test simple channel created");
            }

            private void createNonstopChannelForServices()
            {
                var channel = new NotificationChannel(App.NonStopChannelIdForServices,
                    "Test nonstop channel", NotificationImportance.Default);
                ((NotificationManager)Activity.GetSystemService(NotificationService)).CreateNotificationChannel(channel);
                Log.Error("App CreateChannel", "Test nonstop channel created");
            }


            private void createAlarmMedicationChannel()
            {
                Uri sound = Uri.Parse(ContentResolver.SchemeAndroidResource + "://" + Application.Context.PackageName +
                                      "/" + Resource.Raw
                                          .alarm); //Here is FILE_NAME is the name of file that you want to play
                AudioAttributes attributes = new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification).Build();

                var channel = new NotificationChannel(App.AlarmMedicationChannelId,
                    "Test alarm medication channel", NotificationImportance.High);

                channel.SetSound(sound, attributes);

                ((NotificationManager)Activity.GetSystemService(NotificationService)).CreateNotificationChannel(channel);
                Log.Error("MainActivity App CreateChannel", "Test alarm medication channel created");
            }




            public bool OnTouch(View v, MotionEvent e)
            {
                return true;
            }
        }

        private class SectionsPagerAdapter : FragmentPagerAdapter
        {
            public SectionsPagerAdapter(FragmentManager fm) : base(fm)
            {
            }

            public override int Count { get; } = 3;

            public override Fragment GetItem(int position)
            {
                return PlaceholderFragment.NewInstance(position + 1);
            }
        }
    }
}