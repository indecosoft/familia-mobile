using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using V7Widget = Android.Support.V7.Widget;
using Org.Json;

namespace FamiliaXamarin
{
    [Activity(Label = "PwdResetActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PwdResetActivity : AppCompatActivity, View.IOnClickListener
    {
        private TextInputLayout _emailInputLayout;
        private TextInputLayout _passwordInputLayout;
        private TextInputLayout _pwdRetypeInputLayout;
        private Button _resetButton;
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private EditText _pwdRetypeEditText;
        private TextView _signInTextView;
        private ConstraintLayout _layout;
        //private readonly IWebServices _webServices = new WebServices();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pwdreset);
            InitUi();
            InitListeners();
        }

        private void InitUi()
        {
            var toolbar = FindViewById<V7Widget.Toolbar>(Resource.Id.toolbar);
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
            _signInTextView = FindViewById<TextView>(Resource.Id.tvSignIn);
        }

        private void InitListeners()
        {
            _emailEditText.TextChanged += EmailEditTextOnTextChanged;
            _passwordEditText.TextChanged += PasswordEditTextOnTextChanged;
            _pwdRetypeEditText.TextChanged += PwdRetypeEditTextOnTextChanged;
            _resetButton.SetOnClickListener(this);
            _signInTextView.SetOnClickListener(this);
        }

        private void PwdRetypeEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _pwdRetypeInputLayout.Error = !_pwdRetypeEditText.Text.Equals(_passwordEditText.Text) ? "Parolele nu coincid" : null;
            _resetButton.Enabled = FormValidator();
        }

        private void PasswordEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _passwordInputLayout.Error = !Utils.PasswordValidator(_passwordEditText.Text) ? "Parola trebuie sa contina cel putin 8 caractere si cel putin o majuscula si o cifra" : null;
            _resetButton.Enabled = FormValidator();
        }

        private void EmailEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _emailInputLayout.Error = !Utils.EmailValidator(_emailEditText.Text) ? "Email invalid" : null;
            _resetButton.Enabled = FormValidator();
        }

        private bool FormValidator()
        {
            return Utils.EmailValidator(_emailEditText.Text) && Utils.PasswordValidator(_passwordEditText.Text) &&
                   _passwordEditText.Text.Equals(_pwdRetypeEditText.Text);
        }
        public async void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btnReset:
                    if (FormValidator())
                    {
                        var dataToSent = new JSONObject().Put("email", _emailEditText.Text).Put("password", _passwordEditText.Text);
                        var response = new JSONObject(await WebServices.Post(Constants.PublicServerAddress + "/api/passwordReset", dataToSent));
                        
                        try
                        {
                            Log.Error("Response", response.ToString());
                            switch (response.GetInt("status"))
                            {
                                case 0:
                                    Snackbar.Make(_layout, "Email inexistent", Snackbar.LengthShort).Show();

                                    break;
                                case 1:
                                    Snackbar.Make(_layout, "Eroare de comunicare cu server-ul!", Snackbar.LengthShort).Show();

                                    break;
                                case 2:
                                    Snackbar.Make(_layout, "Un email de validare a fost trimis catre " + _emailEditText.Text, Snackbar.LengthShort).Show();
                                    break;
                                default:
                                    Snackbar.Make(_layout, "Eroare " + response, Snackbar.LengthShort).Show();
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            var snack = Snackbar.Make(_layout, "Eroare " + e.Message, Snackbar.LengthShort);
                            snack.Show();
                        }

                    }
                    break;
                case Resource.Id.tvSignIn:
                    Finish();
                    break;
            }
        }
    }
}