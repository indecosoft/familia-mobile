using Android.Content;
using Android.Graphics;
using Android.Graphics;
using Android.Preferences;
using Android.Util;
using Familia.Helpers;
using Familia.OngBenefits.GenerateCardQR.Models;
using Java.IO;
using Config = Familia.OngBenefits.GenerateCardQR.Models.Config;

namespace Familia.OngBenefits.GenerateCardQR.Storage
{



public class Storage {
    private static Storage Instance;
    private static readonly object padlock = new object();
    private Config config;
    private Buletin buletin;
    private string imagePath;

    private string IMEI;

    private Storage() {
        config = new Config();
        buletin = new Buletin();
    }

    public static Storage getInstance() {
        lock (padlock)
        {
            if (Instance == null)
            {
                Instance = new Storage();
            }
        }
        return Instance;
    }

    public string getImei(Context context) {
        this.IMEI = Android.Provider.Settings.Secure.GetString(context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        return this.IMEI;
    }

    // TODO remove imei as soon as possible
    public void setImei(string IMEI) {
        this.IMEI = IMEI;
    }

    public Config getConfig() {
        return config;
    }

    public Buletin getBuletin() {
        return buletin;
    }

    public string getImagePath() {
        return imagePath;
    }

    public void setImagePath(string imagePath) {
        this.imagePath = imagePath;
    }

    public void setDefaults(string key, string value, Context context) {
        Utils.SetDefaults(key, value);
    }

    public string getDefaults(string key, Context context) {
        return Utils.GetDefaults(key);
    }

    // public string getBase64Img() {
    //     File imgFile = new File(imagePath);
    //     if (imgFile.Exists()) {
    //
    //         Bitmap myBitmap = BitmapFactory.DecodeFile(imgFile.AbsolutePath);
    //
    //         ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
    //         myBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, byteArrayOutputStream);
    //         byte[] byteArrayImage = byteArrayOutputStream.ToByteArray();
    //         return Base64.EncodeToString(byteArrayImage, Base64Flags.Default);
    //     }
    //     return null;
    // }
    //
    //
    // private string convertImageToBase64(Bitmap photo) {
    //     ByteArrayOutputStream baos = new ByteArrayOutputStream();
    //     photo.Compress(Bitmap.CompressFormat.Png, 100, baos);
    //     byte[] imageBytes = baos.ToByteArray();
    //     return Base64.EncodeToString(imageBytes, Base64Flags.Default);
    // }

}

}