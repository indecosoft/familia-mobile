using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.JsonModels;
using Java.IO;
using Newtonsoft.Json;
using Refractored.Controls;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Environment = Android.OS.Environment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentPagerAdapter = Android.Support.V4.App.FragmentPagerAdapter;

namespace FamiliaXamarin
{
    [Activity(Label = "FirstSetup", Theme = "@style/AppTheme.Dark")]
    public class FirstSetup : FragmentActivity
    {

        private SectionsPagerAdapter _sectionsPagerAdapter;
        private FirstSetupViewPager _viewPager;
        private readonly FirstSetupModel _firstSetupModel = new FirstSetupModel();
        public static FirstSetup FragmentContext;

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
            _viewPager = FindViewById<FirstSetupViewPager>(Resource.Id.container);
            _viewPager.SetPagingEnabled(false);
            _viewPager.Adapter = _sectionsPagerAdapter;
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_first_setup, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            return id == Resource.Id.action_settings || base.OnOptionsItemSelected(item);
        }

        public class PlaceholderFragment : Android.Support.V4.App.Fragment
        {
            private const string ArgSectionNumber = "section_number";
            private Button _btnNext;
            private Button _btnBack;
            private Button _btnUpload;
            private Button _btnDate;
            private EditText _tbDate;
            private Spinner _genderSpinner;
            private Spinner _diseaseSpinner;
            private CircleImageView _profileImage;
            private bool _imageValidator;
            private Android.Net.Uri _photoUri;
            private string _imageExtension, _imagePath;

            

            public static PlaceholderFragment NewInstance(int sectionNumber)
            {
                var fragment = new PlaceholderFragment();
                var args = new Bundle();
                args.PutInt(ArgSectionNumber, sectionNumber);
                fragment.Arguments = args;
                return fragment;
            }
            public void ShowPictureDialog()
            {
                var pictureDialog = new AlertDialog.Builder(FragmentContext);
                pictureDialog.SetTitle("Incarcati o imagine");
                string[] pictureDialogItems = {
                    "Alegeti din galerie",
                    "Faceti una acum"};
                pictureDialog.SetItems(pictureDialogItems,
                    delegate (object sender, DialogClickEventArgs args)
                    {
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
            public void ChoosePhotoFromGallary()
            {
                var a = new Intent();
                a.SetType("image/*");
                a.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(a, "Selectati imagine de profil"), Constants.RequestGallery);

            }

            private void TakePhotoFromCamera()
            {
                var intentCamera = new Intent(MediaStore.ActionImageCapture);
                var filePhoto = new File(Environment.ExternalStorageDirectory + "/Familia", "Avatar [" + DateTime.Now + "].jpg");
                _photoUri = Android.Net.Uri.FromFile(filePhoto);
                intentCamera.PutExtra(MediaStore.ExtraOutput, _photoUri);
                StartActivityForResult(intentCamera, Constants.RequestCamera);
            }
            private static string GetPathToImage(Android.Net.Uri uri)
            {
                string docId;
                using (var c1 = FragmentContext.ContentResolver.Query(uri, null, null, null, null))
                {
                    c1.MoveToFirst();
                    var documentId = c1.GetString(0);
                    docId = documentId.Substring(documentId.LastIndexOf(":", StringComparison.Ordinal) + 1);
                }

                string path;

                // The projection contains the columns we want to return in our query.
                string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
#pragma warning disable CS0618 // Type or member is obsolete
                using (var cursor = FragmentContext.ManagedQuery(MediaStore.Images.Media.ExternalContentUri, null, selection, new[] { docId }, null))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    if (cursor == null) return null;
                    var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
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
                            _imagePath = _photoUri.Path;
                            _profileImage.SetImageBitmap(Utils.CheckRotation(_photoUri.Path, MediaStore.Images.Media.GetBitmap(FragmentContext.ContentResolver, _photoUri)));
                            GalleryAddPic();
                            _imageValidator = true;
                            break;
                        case 2:
                            var uri = data.Data;
                            _profileImage.SetImageBitmap(Utils.CheckRotation(GetPathToImage(uri), MediaStore.Images.Media.GetBitmap(FragmentContext.ContentResolver, uri)));
                            _imagePath = GetPathToImage(uri);
                            _imageValidator = true;
                            break;
                        default:
                            _imageValidator = false;
                            break;
                    }

                    if (!_imageValidator) return;
                    _imageExtension = _imagePath.Substring(_imagePath.LastIndexOf(".", StringComparison.Ordinal) + 1);
                    if (_imageExtension.ToLower().Equals("jpeg"))
                        _imageExtension = "jpg";
                    FragmentContext._firstSetupModel.Base64Image = "data:image/" + _imageExtension + ";base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(_imagePath));
                    Log.Error("Base64Image", FragmentContext._firstSetupModel.Base64Image);
                    Log.Error("BImage_EXT", _imageExtension);


                }
                else
                {
                    Toast.MakeText(FragmentContext, "Alege o imagine", ToastLength.Short).Show();
                }

            }


            private void GalleryAddPic()
            {
                var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                var f = new File(_photoUri.Path);
                var contentUri = Android.Net.Uri.FromFile(f);
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
                _genderSpinner = v.FindViewById<Spinner>(Resource.Id.gender_spinner);
                _tbDate = v.FindViewById<EditText>(Resource.Id.tbDate);

                _btnDate = v.FindViewById<Button>(Resource.Id.btnDate);

            }
            private void IniThirdViewUi(View v)
            {
                _diseaseSpinner = v.FindViewById<Spinner>(Resource.Id.Disease_spinner);
            }

            private void InitDefaultUi(View v)
            {
                _btnBack = v.FindViewById<Button>(Resource.Id.btnBack);
                _btnNext = v.FindViewById<Button>(Resource.Id.btnNext);
            }
            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                View rootView;

                switch (Arguments.GetInt(ArgSectionNumber))
                {
                    case 1:
                        rootView = inflater.Inflate(Resource.Layout.fragment_setup1, container, false);
                        InitDefaultUi(rootView);
                        InitFirstViewUi(rootView);
                        _btnUpload.Click += delegate
                        {
                            ShowPictureDialog();
                        };

                        break;
                    case 2:
                        rootView = inflater.Inflate(Resource.Layout.fragment_setup2, container, false);
                        InitDefaultUi(rootView);
                        InitSecondViewUi(rootView);
                        // Create an ArrayAdapter using the string array and a default spinner layout
                        string[] genderArray = { "Masculin", "Feminin" };
                        var genderAdapter = new ArrayAdapter<string>(Context, Resource.Layout.spinner_item, genderArray);
                        // Specify the layout to use when the list of choices appears
                        genderAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                        // Apply the adapter to the spinner
                        _genderSpinner.Adapter = genderAdapter;
                        _genderSpinner.ItemSelected += delegate
                        {
                            FragmentContext._firstSetupModel.Gender = _genderSpinner.SelectedItem.ToString();
                        };
                        _btnDate.Click += delegate
                        {
                            var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                            {
                                _tbDate.Text = time.ToShortDateString();
                                FragmentContext._firstSetupModel.DateOfBirth = time.ToShortDateString();
                            });
                            frag.Show(FragmentContext.FragmentManager, DatePickerFragment.TAG);
                        };
                        _tbDate.FocusChange += delegate
                        {
                            if (!_tbDate.IsFocused) return;
                            var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                            {
                                _tbDate.Text = time.ToShortDateString();
                                FragmentContext._firstSetupModel.DateOfBirth = time.ToShortDateString();
                            });
                            frag.Show(FragmentContext.FragmentManager, DatePickerFragment.TAG);
                        };
                        _tbDate.Click += delegate
                        {
                            var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                            {
                                _tbDate.Text = time.ToShortDateString();
                                FragmentContext._firstSetupModel.DateOfBirth = time.ToShortDateString();
                            });
                            frag.Show(FragmentContext.FragmentManager, DatePickerFragment.TAG);
                        };

                        _btnBack.Click += delegate { FragmentContext._viewPager.CurrentItem = Arguments.GetInt(ArgSectionNumber) - 2; };
                        break;
                    case 3:
                        rootView = inflater.Inflate(Resource.Layout.fragment_setup3, container, false);
                        InitDefaultUi(rootView);
                        IniThirdViewUi(rootView);
                        // Create an ArrayAdapter using the string array and a default spinner layout
                        string[] stringArray = { "Niciuna", "Afectiune 1", "Afectiune 2", "Afectiune 3" };
                        var adapter = new ArrayAdapter<string>(Context, Resource.Layout.spinner_item, stringArray);
                        // Specify the layout to use when the list of choices appears
                        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                        // Apply the adapter to the spinner
                        _diseaseSpinner.Adapter = adapter;
                        _diseaseSpinner.ItemSelected += delegate
                        {
                            FragmentContext._firstSetupModel.Disease = _diseaseSpinner.SelectedItem.ToString();
                        };

                        _btnBack.Click += delegate { FragmentContext._viewPager.CurrentItem = Arguments.GetInt(ArgSectionNumber) - 2; };
                        break;
                    default:
                        rootView = inflater.Inflate(Resource.Layout.fragment_setup1, container, false);
                        InitDefaultUi(rootView);
                        break;
                }

                _btnNext.Click += delegate
                {
                    switch (Arguments.GetInt(ArgSectionNumber))
                    {
                        case 1:
                            if (_imageValidator)
                                FragmentContext._viewPager.CurrentItem = Arguments.GetInt(ArgSectionNumber);
                            else
                                Toast.MakeText(FragmentContext, "Alege o imagine!", ToastLength.Short).Show();
                            break;
                        case 2:
//                            _firstSetupModel.Gender = _genderSpinner.SelectedItem.ToString();
                            PreferenceManager.GetDefaultSharedPreferences(FragmentContext)
                                .Edit()
                                .PutString("Avatar",
                                    Constants.PUBLIC_SERVER_ADDRESS +
                                    PreferenceManager.GetDefaultSharedPreferences(FragmentContext).GetString("Email", "") +
                                    "." + _imageExtension)
                                .Apply();
                            FragmentContext._viewPager.CurrentItem = Arguments.GetInt(ArgSectionNumber);
                            Log.Error("Avatar",
                                PreferenceManager.GetDefaultSharedPreferences(FragmentContext).GetString("Email", ""));
                            break;
                        case 3:
                            string jsonData = JsonConvert.SerializeObject(FragmentContext._firstSetupModel);
                            Log.Error("Json To Send", jsonData);

//                            Toast.MakeText(FragmentContext, jsonData, ToastLength.Long).Show();
                            FragmentContext.StartActivity(typeof(MainActivity));
                            FragmentContext.Finish();
                            break;
                    }
                };

                if (Arguments.GetInt(ArgSectionNumber) == 3)
                    _btnNext.Text = "Gata";
                return rootView;
            }
        }

        public class SectionsPagerAdapter : FragmentPagerAdapter
        {
            public SectionsPagerAdapter(FragmentManager fm) : base(fm) { }

            public override int Count { get; } = 3;
            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                return PlaceholderFragment.NewInstance(position + 1);
            }
        }
    }
}