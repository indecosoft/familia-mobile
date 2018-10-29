using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Security;

namespace FamiliaXamarin.Settings
{
    public class SnoozePreferences
    {
        private ISharedPreferences mSharedPrefs;
        private ISharedPreferencesEditor mPrefsEditor;
        private Context mContext;
        public static readonly string PREFERENCE_ACCES_KEY = "PREFEENCE_ACCES_KEY";

        public SnoozePreferences(Context context)
        {
            this.mContext = context;
            mSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            mPrefsEditor = mSharedPrefs.Edit();
        }

        public void SaveAccesKey(string key)
        {
            mPrefsEditor.PutString(PREFERENCE_ACCES_KEY, key);
            mPrefsEditor.Commit();
        }

        public string GetAccessKey()
        {
            return mSharedPrefs.GetString(PREFERENCE_ACCES_KEY, "");
        }

    }
}