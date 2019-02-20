﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Widget;
using Familia;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace FamiliaXamarin
{
    [Activity(Label = "RegisterActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class RegisterActivity : AppCompatActivity
    {
        private RelativeLayout _layout;
        private TextInputLayout _lastNameInputLayout;
        private TextInputLayout _firstNameInputLayout;
        private TextInputLayout _emailInputLayout;
        private TextInputLayout _passwordInputLayout;
        private TextInputLayout _passwordRetypeInputLayout;
        private EditText _nameEditText;
        private EditText _emailEditText;
        private EditText _firstNameEditText;
        private EditText _passwordEditText;
        private EditText _passwordRetypeEditText;
        private AppCompatButton _btnRegister;
        private AppCompatButton _bntCancel;
        private ProgressBarDialog _progressBarDialog;

        //private readonly IWebServices _webServices = new WebServices();
        private bool _validateForm;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_register);

            // Create your application here
            InitUi();
            InitListeners();
        }

        private void InitUi()
        {
            Android.Support.V7.Widget.Toolbar toolbar =FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            _layout = FindViewById<RelativeLayout>(Resource.Id.layout);
            _lastNameInputLayout = FindViewById<TextInputLayout>(Resource.Id.et_last_name_layout);
            _firstNameInputLayout = FindViewById<TextInputLayout>(Resource.Id.et_first_name_layout);
            _emailInputLayout = FindViewById<TextInputLayout>(Resource.Id.et_email_layout);

            _passwordInputLayout = FindViewById<TextInputLayout>(Resource.Id.et_password_layout);
            _passwordRetypeInputLayout = FindViewById<TextInputLayout>(Resource.Id.et_retype_password_layout);

            _nameEditText = FindViewById<EditText>(Resource.Id.et_last_name);
            _firstNameEditText = FindViewById<EditText>(Resource.Id.et_first_name);
            _emailEditText = FindViewById<EditText>(Resource.Id.et_email);
            _passwordEditText = FindViewById<EditText>(Resource.Id.et_password);
            _passwordRetypeEditText = FindViewById<EditText>(Resource.Id.et_retype_password);
            _btnRegister = FindViewById<AppCompatButton>(Resource.Id.btn_register);
            _bntCancel = FindViewById<AppCompatButton>(Resource.Id.btn_cancel);


            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Inregistrare...", this, false);

            _btnRegister.Enabled = false;
        }

        private void InitListeners()
        {        
            _btnRegister.Click += BtnRegisterOnClick;
            _bntCancel.Click += BntCancelOnClick;
            _nameEditText.TextChanged +=NameEditTextOnTextChanged;
            _firstNameEditText.TextChanged += FirstNameEditTextOnTextChanged;
            _emailEditText.TextChanged += EmailEditTextOnTextChanged;
            _passwordEditText.TextChanged += PasswordEditTextOnTextChanged;
            _passwordRetypeEditText.TextChanged += PasswordRetypeEditTextOnTextChanged;
        }

        private void FirstNameEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            string name = _nameEditText.Text;
            if (name == string.Empty)
            {
                _firstNameInputLayout.Error = "Camp obligatoriu";
                _validateForm = false;
            }
            else
            {
                _firstNameInputLayout.Error = null;
                _validateForm = true;
            }
            _btnRegister.Enabled = ValidateForm();
        }

        private bool ValidateForm()
        {
            return _nameEditText.Text != string.Empty && 
                   _firstNameEditText.Text != string.Empty &&
                   _emailEditText.Text != string.Empty &&
                   _passwordEditText.Text != string.Empty &&
                   _passwordRetypeEditText.Text != string.Empty &&
                   _validateForm;
        }

        private void PasswordRetypeEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {

            if (_passwordRetypeEditText.Text.Equals(_passwordEditText.Text))
            {
                _passwordRetypeInputLayout.Error = null;
                _validateForm = true;
            }
            else
            {
                _passwordRetypeInputLayout.Error = "Parolele nu coincid!";
                _validateForm = false;
            }

            _btnRegister.Enabled = ValidateForm();
        }

        private void PasswordEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            string password = _passwordEditText.Text;
            if (password == string.Empty)
            {
                _passwordInputLayout.Error = "Camp obligatoriu";
                _validateForm = false;

            }
            else if (!Utils.PasswordValidator(password))
            {
                _passwordInputLayout.Error = "Parola trebuie sa contina cel putin 8 caractere si cel putin o majuscula si o cifra";
                _validateForm = false;
            }
            else
            {
                _passwordInputLayout.Error = null;
                _validateForm = true;
            }
            _btnRegister.Enabled = ValidateForm();
        }

        private void EmailEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            string email = _emailEditText.Text;
            if (email == string.Empty)
            {
                _emailInputLayout.Error ="Camp obligatoriu";
                _validateForm = false;

            }
            else if (!Utils.EmailValidator(email))
            {
                _emailInputLayout.Error = "Adresa de email invalida";
                _validateForm = false;
            }
            else
            {
                _emailInputLayout.Error = null;
                _validateForm = true;
            }
            _btnRegister.Enabled = ValidateForm();
        }

        private void NameEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            string name = _nameEditText.Text;
            if (name == string.Empty)
            {
                _lastNameInputLayout.Error = "Camp obligatoriu";
                _validateForm = false;
            }
            else
            {
                _lastNameInputLayout.Error = null;
                _validateForm = true;
            }
            _btnRegister.Enabled = ValidateForm();
        }

        private void BntCancelOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private async void BtnRegisterOnClick(object sender, EventArgs e)
        {
            _progressBarDialog.Show();

            await Task.Run(async () => {

                var dataToSend = new JSONObject().Put("name", $"{_nameEditText.Text} {_firstNameEditText.Text}").Put("email", _emailEditText.Text).Put("password", _passwordEditText.Text).Put("type", 4).Put("imei", Utils.GetImei(this));

                var response = await WebServices.Post(Constants.PublicServerAddress + "/api/register", dataToSend);
                if (response != null)
                {
                    Snackbar snack;
                    var responseJson = new JSONObject(response);
                    switch (responseJson.GetInt("status"))
                    {
                        case 0:
                            snack = Snackbar.Make(_layout, "Wrong Data", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 1:
                            snack = Snackbar.Make(_layout, "Internal Server Error", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 2:
                            snack = Snackbar.Make(_layout, "Account created", Snackbar.LengthLong);
                            snack.Show();
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
            _progressBarDialog.Dismiss();
        }
    }
}