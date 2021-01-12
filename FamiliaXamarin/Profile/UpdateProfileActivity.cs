using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Signature;
using Familia.Asistenta_sociala;
using Familia.Helpers;
using Familia.Medicatie;
using Familia.Profile.Data;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using Org.Json;
using Refractored.Controls;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace Familia.Profile
{
    [Activity(Label = "UpdateProfileActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdateProfileActivity : AppCompatActivity, View.IOnClickListener
    {

        private PersonalData personalData;
        private PersonView personView;
        private CircleImageView ciwProfileImage;
        private TextView tvBirthDate;
        private TextView tvName;
        private EditText etName;
        private AppCompatButton btnUploadImage;
        private AppCompatButton btnChangeDiseases;
        private TextView btnLabelDiseases;
        private string gender;
        private string name;
        private string birthdate;
        private File Dir;
        private Uri _photoUri;
        private FileInfo _fileInformations;
        private string _imageExtension, _imagePath;
        private List<SearchListModel> _selectedDiseases = new List<SearchListModel>();
        private readonly string[] _permissionsArray =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };
        private List<PersonalDisease> listToBeSaved;
        private string tempImgBase64;

        private bool isSavedClick;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_update_profile);
            InitUI();
            Log.Error("UpdateProfileActivity  person view", "oncreat start");
            personView = new PersonView(Intent.GetStringExtra("name"),
                                        Utils.GetDefaults("Email"),
                                        Intent.GetStringExtra("birthdate"),
                                        Intent.GetStringExtra("gender"),
                                        Intent.GetStringExtra("avatar"),
                                        null, "none");

            Log.Error("UpdateProfileActivity  person view", Intent.GetStringExtra("name"));
            Log.Error("UpdateProfileActivity  person view", Utils.GetDefaults("Email"));
            Log.Error("UpdateProfileActivity  person view", Intent.GetStringExtra("birthdate"));
            Log.Error("UpdateProfileActivity  person view", Intent.GetStringExtra("gender"));
            Log.Error("UpdateProfileActivity  person view", Intent.GetStringExtra("avatar"));

            Log.Error("UpdateProfileActivity  person view", "data was extrated from intent");

            LoadModel();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {

            if (grantResults[0] != Permission.Granted)
            {
                Toast.MakeText(this, "Permisiuni pentru telefon refuzate", ToastLength.Short).Show();

            }
            else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted)
            {
                Toast.MakeText(this, "Permisiuni pentru locatie refuzate", ToastLength.Short).Show();

            }
            else if (grantResults[3] != Permission.Granted)
            {
                Toast.MakeText(this, "Permisiuni pentru camera refuzate", ToastLength.Short).Show();
            }
        }

        private void InitUI()
        {
            FindViewById<Button>(Resource.Id.btn_closeit).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.btn_save).SetOnClickListener(this);
            ciwProfileImage = FindViewById<CircleImageView>(Resource.Id.profile_image);
            tvName = FindViewById<EditText>(Resource.Id.et_name);
            tvBirthDate = FindViewById<TextView>(Resource.Id.tv_birthdate);
            tvBirthDate.SetOnClickListener(this);
            etName = FindViewById<EditText>(Resource.Id.et_name);
            btnUploadImage = FindViewById<AppCompatButton>(Resource.Id.btn_upload);
            btnUploadImage.SetOnClickListener(this);
            btnLabelDiseases = FindViewById<TextView>(Resource.Id.tv_labelDiseases);
            btnChangeDiseases = FindViewById<AppCompatButton>(Resource.Id.btn_diseases);
            btnChangeDiseases.SetOnClickListener(this);

            const string permission = Manifest.Permission.ReadPhoneState;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                RequestPermissions(_permissionsArray, 0);
            }

            isSavedClick = false;
        }

        public async void OnClick(View v)
        {
            var returnIntent = new Intent();

            switch (v.Id)
            {
                case Resource.Id.btn_closeit:
                    returnIntent = new Intent();
                    SetResult(Result.Canceled, returnIntent);
                    Finish();
                    break;
                case Resource.Id.btn_save:

                    if (isSavedClick == false)
                    {
                        isSavedClick = true;

                    name = tvName.Text;

                    if (FindViewById<RadioButton>(Resource.Id.rb_female).Checked)
                    {
                        gender = "Feminin";
                    }

                    if (FindViewById<RadioButton>(Resource.Id.rb_male).Checked)
                    {
                        gender = "Masculin";
                    }

                    if (listToBeSaved == null)
                    {
                        listToBeSaved = personalData.listOfPersonalDiseases;
                    }

                    if (!name.Equals(""))
                        {

                            if (tempImgBase64 == null)
                            {
                                tempImgBase64 = personalData.Base64Image;
                            }

                            var pd = new PersonalData(
                                listToBeSaved,
                                tempImgBase64,
                                tvBirthDate.Text,
                                gender,
                                Utils.GetDefaults("Email"),
                                ".jpg"
                                );

                            await SaveProfileData(pd);

                            returnIntent = new Intent();
                            returnIntent.PutExtra("name", name);
                            returnIntent.PutExtra("birthdate", birthdate);
                            returnIntent.PutExtra("gender", gender);
                            SetResult(Result.Ok, returnIntent);
                            Finish();
                        }

                    }


                    break;
                case Resource.Id.tv_birthdate:
                    OnDateClick();
                    break;
                case Resource.Id.btn_diseases:
                    LoadSelectDiseaseActivity();
                    break;
                case Resource.Id.btn_upload:
                    ShowPictureDialog();
                    break;
            }
        }

        private async void LoadSelectDiseaseActivity()
        {
            var progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se aduc date...", this, false);
            progressBarDialog.Show();
            var list = new List<SearchListModel>();

            if (listToBeSaved != null)
            {
                Log.Error("UpdateProfileActivity", "diseases count: " + listToBeSaved.Count);
                foreach (PersonalDisease element in listToBeSaved)
                {
                    var slm = new SearchListModel();
                    slm.Id = element.Cod;
                    slm.Title = element.Name;
                    slm.IsSelected = true;
                    list.Add(slm);
                }
            }
            else
            {
                if (personalData.listOfPersonalDiseases.Count != 0)
                {
                    Log.Error("UpdateProfileActivity", "diseases count: " + personalData.listOfPersonalDiseases.Count);
                    foreach (PersonalDisease element in personalData.listOfPersonalDiseases)
                    {
                        var slm = new SearchListModel();
                        slm.Id = element.Cod;
                        slm.Title = element.Name;
                        slm.IsSelected = true;
                        list.Add(slm);
                    }
                }
            }

            var intent = new Intent(this, typeof(SearchListActivity));
            intent.PutExtra("Items", JsonConvert.SerializeObject(await GetDiseaseList()));
            intent.PutExtra("SelectedItems", JsonConvert.SerializeObject(list));
            StartActivityForResult(intent, 3);
            RunOnUiThread(() => progressBarDialog.Dismiss());
        }

        private async Task<List<SearchListModel>> GetDiseaseList()
        {
            var items = new List<SearchListModel>();
            await Task.Run(async () =>
            {
                string result = await WebServices.WebServices.Get(Constants.PublicServerAddress + "/api/getDisease", Utils.GetDefaults("Token"));
                if (result != null)
                {
                    Log.Error("UpdateProfileActivity", "RESULT " + result);
                    var arrayOfDiseases = new JSONArray(result);

                    for (var i = 0; i < arrayOfDiseases.Length(); i++)
                    {
                        var jsonModel = new JSONObject(arrayOfDiseases.Get(i).ToString());

                        items.Add(new SearchListModel
                        {
                            Id = jsonModel.GetInt("cod"),
                            Title = jsonModel.GetString("denumire")
                        });
                    }
                    Log.Error("UpdateProfileActivity", "RESULT" + items.Count);
                }
                else
                {
                    Log.Error("UpdateProfileActivity", "RESULT is null ");
                }
            });

            return items;
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            if (resultCode == Result.Ok)
            {

                RequestOptions requestOptions = getGlideRequestOptions();

                switch (requestCode)
                {
                    case 1:

                        Glide.With(this)
                            .Load(new File(_imagePath))
                            .Apply(requestOptions)
                            .Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated)))
                            .Into(ciwProfileImage);

                        GalleryAddPic();
                        _fileInformations = new FileInfo(new File(_imagePath).Path);
                        Log.Error("Size", _fileInformations.Length.ToString());
                        if (_fileInformations.Length >= 10485760)
                        {
                            ImageTooLargeWarning();
                        }
                        _imageExtension =
                            _imagePath.Substring(_imagePath.LastIndexOf(".", StringComparison.Ordinal) +
                                                 1);
                        if (_imageExtension.ToLower().Equals("jpeg"))
                            _imageExtension = "jpg";
                        tempImgBase64 =
                            "data:image/" + _imageExtension + ";base64," +
                            Convert.ToBase64String(System.IO.File.ReadAllBytes(_imagePath));
                        personalData.ImageExtension = _imageExtension;

                        break;
                    case 2:
                        try
                        {
                        Uri uri = data.Data;
                        _imagePath = GetPathToImage(uri);


                        Glide.With(this)
                             .Load(new File(_imagePath))
                             .Apply(requestOptions)
                             .Into(ciwProfileImage);

                        _fileInformations = new FileInfo(_imagePath);
                        Log.Error("Size", _fileInformations.Length.ToString());
                        if (_fileInformations.Length >= 10485760)
                        {
                            ImageTooLargeWarning();
                        }

                        _imageExtension =
                            _imagePath.Substring(_imagePath.LastIndexOf(".", StringComparison.Ordinal) +
                                                 1);
                        if (_imageExtension.ToLower().Equals("jpeg"))
                            _imageExtension = "jpg";
                        tempImgBase64 =
                            "data:image/" + _imageExtension + ";base64," +
                            Convert.ToBase64String(System.IO.File.ReadAllBytes(_imagePath));
                        personalData.ImageExtension = _imageExtension;

                        }
                        catch (Exception e)
                        {
                            Log.Error("UpdateProfileActivity", "err " + e.Message);
                            Toast.MakeText(this, "Nu este permisa incarcarea imaginilor din cloud", ToastLength.Long).Show();
                        }

                        break;
                    case 3:

                        Log.Error("UpdateProfileActivity", "result updated: " + data.GetStringExtra("result"));
                        _selectedDiseases = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));
                        listToBeSaved = new List<PersonalDisease>();
                        foreach (SearchListModel item in _selectedDiseases)
                        {
                            listToBeSaved.Add(new PersonalDisease(item.Id, item.Title));
                        }
                        btnLabelDiseases.Text = "Afecțiuni selectate:" + listToBeSaved.Count;

                        break;
                }
            }

            if (resultCode == Result.Canceled)
            {
                Log.Error("UpdateProfileActivity", "cancel result");
                if (requestCode == 3)
                {
                    listToBeSaved = new List<PersonalDisease>();
                    btnLabelDiseases.Text = "Afecțiuni selectate:" + listToBeSaved.Count;
                }
            }

        }

        private static RequestOptions getGlideRequestOptions()
        {
            var requestOptions = new RequestOptions();
            requestOptions.Placeholder(Resource.Drawable.account_profile_default);
            requestOptions.CenterCrop();
            return requestOptions;
        }

        private void OnDateClick()
        {
            var frag = DatePickerMedicine.NewInstance(delegate (DateTime time)
            {
                tvBirthDate.Text = time.ToString("dd/MM/yyyy");
                birthdate = time.ToString("MM/dd/yyyy");
            });
            frag.Show(SupportFragmentManager, DatePickerMedicine.TAG);
        }

        public async void LoadModel()
        {
            var dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
            dialog.Show(); try
            {
                Log.Error("UpdateProfileActivity ERR load model", "load picture");

                var requestedOptions = getGlideRequestOptions();
                Glide.With(this)
                    .Load(personView.Avatar)
                    .Apply(requestedOptions)
                    .Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated)))
                    .Into(ciwProfileImage);

                etName.Text = personView.Name;
                SetGender(personView.Gender);

                birthdate = getDateString(personView.Birthdate);

                if (birthdate == null)
                {
                    birthdate = "-/-/-";
                }

                personView.Birthdate = getDateString(personView.Birthdate);

                if (personView.Birthdate == null)
                {
                    personView.Birthdate = "-/-/-";
                }

                tvBirthDate.Text = personView.Birthdate;

                Log.Error("UpdateProfileActivity ERR load model", "start getting diseases");
                personalData = await ProfileStorage.GetInstance().read();
                Log.Error("UpdateProfileActivity ERR load model", "after read data from db");


                if (personalData == null)
                {
                    Log.Error("UpdateProfileActivity ERR load model", "something went wrong");
                    Toast.MakeText(this, "S-a intampinat o eroare.", ToastLength.Long).Show();
                }
                else {
                    Log.Error("UpdateProfileActivity ERR load model", " else ");

                    if (personalData.listOfPersonalDiseases == null) {
                        Log.Error("UpdateProfileActivity ERR load model", "list is null");
                        personalData.listOfPersonalDiseases = new List<PersonalDisease>();
                    }
                    Log.Error("UpdateProfileActivity ERR load model", "set disease count to TV " + personalData.listOfPersonalDiseases.Count);
                    btnLabelDiseases.Text = "Afecțiuni curente:" + personalData.listOfPersonalDiseases.Count;
                }

                if (int.Parse(Utils.GetDefaults("UserType")) == 2)
                {
                   FindViewById<ImageView>(Resource.Id.iw_icon).Visibility = ViewStates.Gone;
                   FindViewById<TextView>(Resource.Id.tv_labelDiseases).Visibility = ViewStates.Gone;
                   FindViewById<AppCompatButton>(Resource.Id.btn_diseases).Visibility = ViewStates.Gone;
                }


                RunOnUiThread(() => dialog.Dismiss());
            }
            catch (Exception e)
            {
                Log.Error("UpdateProfileActivity ERR 1", e.Message);
                RunOnUiThread(() => dialog.Dismiss());
            }
        }

        private string getDateString(string dateString)
        {
            try
            {
                //format datetime de pe server
                var birthdate = Convert.ToDateTime(dateString);
                return birthdate.Day + "/" + birthdate.Month + "/" + birthdate.Year;
            }
            catch (Exception e)
            {
                Log.Error("UpdateProfileActivity", "birthdate convert: " + e.Message);

                try
                {   //format zi/luna/an
                    var refact = personView.Birthdate.Split("/");
                    string dt;
                    DateTime mytime;
                    mytime = new DateTime(int.Parse(refact[2]), int.Parse(refact[1]), int.Parse(refact[0]));
                    dt = mytime.ToString("MM/dd/yyyy");
                    return mytime.Day + "/" + mytime.Month + "/" + mytime.Year;

                }
                catch (Exception ex)
                {
                    Log.Error("UpdateProfileActivity ERR", ex.Message);
                }
                return null;
            }
           
        }

        private void SetGender(string gender)
        {
            if (gender.Equals("Feminin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_female).Checked = true;
            }

            if (gender.Equals("Masculin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_male).Checked = true;
            }
        }

        private async Task SaveProfileData(PersonalData pd)
        {
            try
            {
                var data = new PersonalData(
                    pd.listOfPersonalDiseases,
                    pd.Base64Image,
                    pd.DateOfBirth,
                    pd.Gender,
                    pd.ImageName,
                    pd.ImageExtension
                );

                ProfileStorage.GetInstance().personalData = data;
                if (!(await ProfileStorage.GetInstance().save()))
                {
                    Log.Error("UpdateProfileActivity", "Error saving profile data ..");
                }
            }
            catch (Exception e)
            {
                Log.Error("UpdateProfileActivity ERR", e.Message);
            }
        }


        #region select photo
        private void ShowPictureDialog()
        {
            var pictureDialog = new AlertDialog.Builder(this, Resource.Style.AppTheme_Dialog);
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

        private void CreateDirectoryForPictures()
        {
            Dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "Familia");
            if (!Dir.Exists())
            {
                Dir.Mkdirs();
            }
        }

        private void ImageTooLargeWarning()
        {
            Toast.MakeText(this, "Fotografie prea mare! Dimensiunea maxima acceptata este de 10 Mb.", ToastLength.Long).Show();
            var resourcePath = "@drawable/profile"; // where myresource (without the extension) is the file
            int imageResource = Resources.GetIdentifier(resourcePath, null, PackageName);
            Drawable res = ContextCompat.GetDrawable(this, imageResource);
            ciwProfileImage.SetImageDrawable(res);
        }

        private void GalleryAddPic()
        {
            var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            var f = new File(_photoUri.Path);
            Uri contentUri = Uri.FromFile(f);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
        }

        private bool IsThereAnAppToTakePictures()
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            var availableActivities =
                PackageManager.QueryIntentActivities(intent,
                    PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private File CreateImageFile()
        {
            string timeStamp = new SimpleDateFormat("yyyy.MM.dd_HH:mm").Format(new Date());
            string imageFileName = "Avatar_" + timeStamp + "";
            File storageDir = GetExternalFilesDir(Environment.DirectoryPictures);
            var image = File.CreateTempFile(
                imageFileName, /* prefix */
                ".jpg", /* suffix */
                storageDir /* directory */
            );

            _imagePath = image.AbsolutePath;
            return image;
        }

        private void TakePhotoFromCamera()
        {
            if (!IsThereAnAppToTakePictures()) return;
            CreateDirectoryForPictures();

            var intent = new Intent(MediaStore.ActionImageCapture);
            _photoUri = FileProvider.GetUriForFile(this,
                "IndecoSoft.Familia.fileprovider",
                CreateImageFile());
            intent.PutExtra(MediaStore.ExtraOutput, _photoUri);
            StartActivityForResult(intent, Constants.RequestCamera);
        }

        private string GetPathToImage(Uri uri)
        {
            string docId;
            using (ICursor c1 = ContentResolver.Query(uri,
                null, null, null, null))
            {
                c1.MoveToFirst();
                string documentId = c1.GetString(0);
                docId = documentId.Substring(
                    documentId.LastIndexOf(":", StringComparison.Ordinal) + 1);
            }

            string path;
            const string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (ICursor cursor = ContentResolver.Query(
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

        #endregion

    }
}