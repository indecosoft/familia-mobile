using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Org.Json;

namespace FamiliaXamarin
{
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark", MainLauncher = true)]
    public class LoginActivity : AppCompatActivity
    {
        private ConstraintLayout _layout;

        private EditText _usernameEditText;
        private EditText _passwordEditText;
        private TextView _registerTextView;
        private TextView _pwdResetTextView;
        private Button _loginButton;
#pragma warning disable 618
        private ProgressDialog _progressDialog;
#pragma warning restore 618

        private readonly string[] _permissionsLocation =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        private readonly IWebServices _webServices = new WebServices();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);
            // Create your application here
            InitUi();
            InitListeners();

            const string permission = Manifest.Permission.ReadPhoneState;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                RequestPermissions(_permissionsLocation, 0);
            }
            Utils.SetDefaults("Token", "FFF", this);
            Utils.SetDefaults("Imei", Utils.GetImei(this), this);
            Utils.SetDefaults("Email", "voicu.babiciu@indecosoft.ro", this);
            Utils.SetDefaults("Logins", true.ToString(), this);
            Utils.SetDefaults("Avatar", Constants.PublicServerAddress + "sds", this);

            try
            {
                if (!bool.Parse(Utils.GetDefaults("Logins", this)) || Utils.GetDefaults("Token", this) == null) return;
                StartActivity(typeof(MainActivity));
                Finish();
            }
            catch
            {
                // ignored
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (grantResults[0] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru telefon refuzate", Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru locatie refuzate", Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[3] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru camera refuzate", Snackbar.LengthShort);
                snack.Show();
            }

        }
        private void InitUi()
        {
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);

            _usernameEditText = FindViewById<EditText>(Resource.Id.UsernameEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.PasswordEditText);

            _loginButton = FindViewById<Button>(Resource.Id.btnLogin);

            _registerTextView = FindViewById<TextView>(Resource.Id.Signup);
            _pwdResetTextView = FindViewById<TextView>(Resource.Id.PasswordReset);

#pragma warning disable 618
            _progressDialog = new ProgressDialog(this);
#pragma warning restore 618
            _progressDialog.SetTitle("Va rugam asteptati ...");
            _progressDialog.SetMessage("Autentificare");
            _progressDialog.SetCancelable(false);


        }

        private void InitListeners()
        {
            _loginButton.Click += BtnOnClick;
            _registerTextView.Click += RegisterTextViewOnClick;
            _pwdResetTextView.Click += PwdResetTextViewOnClick;

        }

        private void PwdResetTextViewOnClick(object sender, EventArgs e)
        {
           StartActivity(typeof(PwdResetActivity));
        }

        private void RegisterTextViewOnClick(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

        private async void BtnOnClick(object sender, EventArgs e)
        {
            Utils.HideKeyboard(this);
            _progressDialog.Show();

            await Task.Run(async () => {
                var dataToSend = new JSONObject().Put("email", _usernameEditText.Text)
                    .Put("password", _passwordEditText.Text).Put("imei", Utils.GetImei(this));

                string response = await _webServices.Post(Constants.PublicServerAddress + "api/login", dataToSend);
                if (response != null)
                {
                    Snackbar snack;
                    var responseJson = new JSONObject(response);
                    switch (responseJson.GetInt("status"))
                    {
                        case 0:
                            snack = Snackbar.Make(_layout, "Wrong Username or Password", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 1:
                            snack = Snackbar.Make(_layout, "Internal Server Error", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 2:
                            var token = new JSONObject(response).GetString("token");
                            var nume = new JSONObject(response).GetString("nume");
                            var logins = new JSONObject(response).GetBoolean("logins");
                            var avatar = new JSONObject(response).GetString("avatar");
                    
                            Utils.SetDefaults("Token", token, this);
                            Utils.SetDefaults("Imei", Utils.GetImei(this), this);
                            Utils.SetDefaults("Email", _usernameEditText.Text, this);
                            Utils.SetDefaults("Logins", logins.ToString(), this);
                            Utils.SetDefaults("Nume", nume, this);
                            Utils.SetDefaults("Avatar", Constants.PublicServerAddress + avatar, this);

                            if (logins)
                                StartActivity(typeof(MainActivity));
                            else
                                StartActivity(typeof(FirstSetup));

                            Finish();
                            break;
                    }
                }
                else
                {
                    var snack = Snackbar.Make(_layout, "Unable to reach the server!", Snackbar.LengthLong);
                    snack.Show();
                }
            });
            _progressDialog.Dismiss();
 
        }
    }
}