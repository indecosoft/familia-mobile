using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;
using Org.Json;

namespace FamiliaXamarin
{
    [Activity(Label = "RegisterActivity", Theme = "@style/AppTheme.Dark")]
    public class RegisterActivity : AppCompatActivity
    {
        private ConstraintLayout _layout;
        private TextInputLayout _nameInputLayout;
        private TextInputLayout _emailInputLayout;
        private TextInputLayout _passwordInputLayout;
        private TextInputLayout _passwordRetypeInputLayout;
        private EditText _nameEditText;
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private EditText _passwordRetypeEditText;
        private Button _btnRegister;
        private TextView _signInTextView;
#pragma warning disable CS0618 // Type or member is obsolete
        private ProgressDialog _progressDialog;
#pragma warning restore CS0618 // Type or member is obsolete

        private readonly IWebServices _webServices = new WebServices();
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

            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);
            _nameInputLayout = FindViewById<TextInputLayout>(Resource.Id.NameInputLayout);
            _emailInputLayout = FindViewById<TextInputLayout>(Resource.Id.EmailInputLayout);
            _passwordInputLayout = FindViewById<TextInputLayout>(Resource.Id.PasswordInputLayout);
            _passwordRetypeInputLayout = FindViewById<TextInputLayout>(Resource.Id.PasswordRetypeInputLayout);
            _nameEditText = FindViewById<EditText>(Resource.Id.etName);
            _emailEditText = FindViewById<EditText>(Resource.Id.etEmail);
            _passwordEditText = FindViewById<EditText>(Resource.Id.etPassword);
            _passwordRetypeEditText = FindViewById<EditText>(Resource.Id.etPasswordRetype);
            _btnRegister = FindViewById<Button>(Resource.Id.btnRegister);
            _signInTextView = FindViewById<TextView>(Resource.Id.tvSignIn);

#pragma warning disable CS0618 // Type or member is obsolete
            _progressDialog = new ProgressDialog(this);
#pragma warning restore CS0618 // Type or member is obsolete
            _progressDialog.SetTitle("Va rugam asteptati ...");
            _progressDialog.SetMessage("Inregistrare");
            _progressDialog.SetCancelable(false);

            _btnRegister.Enabled = false;
        }

        private void InitListeners()
        {        
            _btnRegister.Click += BtnRegisterOnClick;
            _signInTextView.Click += SignInTextViewOnClick;
            _nameEditText.TextChanged +=NameEditTextOnTextChanged;
            _emailEditText.TextChanged += EmailEditTextOnTextChanged;
            _passwordEditText.TextChanged += PasswordEditTextOnTextChanged;
            _passwordRetypeEditText.TextChanged += PasswordRetypeEditTextOnTextChanged;
        }

        private bool ValidateForm()
        {
            return _nameEditText.Text != string.Empty &&
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

            if (ValidateForm())
            {
                _btnRegister.Enabled = true;
            }
        }

        private void PasswordEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            String password = _passwordEditText.Text;
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
            if (ValidateForm())
            {
                _btnRegister.Enabled = true;
            }
            else
                _btnRegister.Enabled = false;
        }

        private void EmailEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            String email = _emailEditText.Text;
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
            if (ValidateForm())
            {
                _btnRegister.Enabled= true;
            }
            else
                _btnRegister.Enabled= false;
        }

        private void NameEditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            String name = _nameEditText.Text;
            if (name == string.Empty)
            {
                _nameInputLayout.Error = "Camp obligatoriu";
                _validateForm = false;
            }
            else
            {
                _nameInputLayout.Error = null;
                _validateForm = true;
            }
            if (ValidateForm())
            {
                _btnRegister.Enabled = true;
            }
            else
                _btnRegister.Enabled = false;
        }

        private void SignInTextViewOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private async void BtnRegisterOnClick(object sender, EventArgs e)
        {
            _progressDialog.Show();

            await Task.Run(async () => {

                var dataToSend = new JSONObject().Put("name", _nameEditText.Text).Put("email", _emailEditText.Text).Put("password", _passwordEditText.Text).Put("type", 4).Put("imei", Utils.GetImei(this));

                var response = await _webServices.Post(Constants.PublicServerAddress + "api/register", dataToSend);
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
            _progressDialog.Dismiss();
        }
    }
}