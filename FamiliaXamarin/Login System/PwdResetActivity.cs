using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ConstraintLayout.Widget;
using Familia.Helpers;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;
using Org.Json;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Familia.Login_System {
    [Activity(Label = "PwdResetActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class PwdResetActivity : AppCompatActivity, View.IOnClickListener {
        private TextInputLayout _emailInputLayout;
        private TextInputLayout _passwordInputLayout;
        private TextInputLayout _pwdRetypeInputLayout;
        private Button _resetButton;
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private EditText _pwdRetypeEditText;
        private Button _signInTextView;

        private ConstraintLayout _layout;
        //private readonly IWebServices _webServices = new WebServices();


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pwdreset);
            InitUi();
            InitListeners();
        }

        private void InitUi() {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;


            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);

            _emailInputLayout = FindViewById<TextInputLayout>(Resource.Id.EmailInputLayout);
            _passwordInputLayout = FindViewById<TextInputLayout>(Resource.Id.PasswordInputLayout);
            _pwdRetypeInputLayout = FindViewById<TextInputLayout>(Resource.Id.PasswordRetypeInputLayout);

            _emailEditText = FindViewById<EditText>(Resource.Id.etEmail);
            _passwordEditText = FindViewById<EditText>(Resource.Id.etPassword);
            _pwdRetypeEditText = FindViewById<EditText>(Resource.Id.etPasswordRetype);

            _resetButton = FindViewById<Button>(Resource.Id.btnReset);
            _signInTextView = FindViewById<Button>(Resource.Id.btnCancel);
            _resetButton.Enabled = FormValidator();
        }

        private void InitListeners() {
            _emailEditText.TextChanged += EmailEditTextOnTextChanged;
            _passwordEditText.TextChanged += PasswordEditTextOnTextChanged;
            _pwdRetypeEditText.TextChanged += PwdRetypeEditTextOnTextChanged;
            _resetButton.SetOnClickListener(this);
            _signInTextView.SetOnClickListener(this);
        }

        private void PwdRetypeEditTextOnTextChanged(object sender, TextChangedEventArgs e) {
            _pwdRetypeInputLayout.Error =
                _pwdRetypeEditText.Text != null && !_pwdRetypeEditText.Text.Equals(_passwordEditText.Text)
                    ? "Parolele nu coincid"
                    : null;
            _resetButton.Enabled = FormValidator();
        }

        private void PasswordEditTextOnTextChanged(object sender, TextChangedEventArgs e) {
            _passwordInputLayout.Error = !Utils.PasswordValidator(_passwordEditText.Text)
                ? "Parola trebuie sa contina cel putin 8 caractere si cel putin o majuscula si o cifra"
                : null;
            _resetButton.Enabled = FormValidator();
        }

        private void EmailEditTextOnTextChanged(object sender, TextChangedEventArgs e) {
            _emailInputLayout.Error = !Utils.EmailValidator(_emailEditText.Text) ? "Email invalid" : null;
            _resetButton.Enabled = FormValidator();
        }

        private bool FormValidator() {
            return _passwordEditText.Text != null && Utils.EmailValidator(_emailEditText.Text) &&
                   Utils.PasswordValidator(_passwordEditText.Text) &&
                   _passwordEditText.Text.Equals(_pwdRetypeEditText.Text);
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.btnReset:
                    if (FormValidator()) {
                        SendData();
                    }

                    break;
                case Resource.Id.btnCancel:
                    Finish();
                    break;
            }
        }

        private async void SendData() {
            var dialog = new ProgressBarDialog("Va rugam asteptati", "Resetare...", this, false);
            dialog.Show();

            JSONObject dataToSent = new JSONObject().Put("email", _emailEditText.Text)
                .Put("password", _passwordEditText.Text);
            string res =
                await WebServices.WebServices.Post("/api/passwordReset", dataToSent);

            if (res != null) {
                Log.Error("PwdResetActivity", res);

                try {
                    var response = new JSONObject(res);
                    Log.Error("Response", response.ToString());
                    switch (response.GetInt("status")) {
                        case 0:
                            Snackbar.Make(_layout, "Email inexistent", Snackbar.LengthShort).Show();
                            break;
                        case 1:
                            Snackbar.Make(_layout, "Eroare de comunicare cu server-ul!", Snackbar.LengthShort).Show();
                            break;
                        case 2:
                            Snackbar.Make(_layout, "Un email de validare a fost trimis catre " + _emailEditText.Text,
                                Snackbar.LengthShort).Show();
                            break;
                        default:
                            Snackbar.Make(_layout, "Eroare " + response, Snackbar.LengthShort).Show();
                            break;
                    }
                }
                catch (Exception e) {
                    var snack = Snackbar.Make(_layout, "Eroare " + e.Message, Snackbar.LengthShort);
                    snack.Show();
                }
            }
            else {
                Log.Error("PwdResetActivity", "res is null");
                var snack = Snackbar.Make(_layout, "Eroare preluare date de pe server", Snackbar.LengthShort);
                snack.Show();
            }

            RunOnUiThread(() => { dialog.Dismiss(); });
        }
    }
}
