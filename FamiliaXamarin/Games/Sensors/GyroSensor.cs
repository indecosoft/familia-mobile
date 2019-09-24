using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;



namespace Familia.Games.Sensors
{



    class GyroSensor: Java.Lang.Object, ISensorEventListener
    {
        private SensorManager sensorManager;
        private Sensor gyroscopeSensor;
        private Context context;
        private IGyroSensorChangedListener gyroSensorChangedListener;

        public GyroSensor(Context context)
        {
            this.context = context;
            SetSettings();
        }

        public void SetGyroListener(IGyroSensorChangedListener listener)
        {
            gyroSensorChangedListener = listener;
        }

        public void Dispose()
        {
            Log.Error("GyroSenso", "dispose");
            base.Dispose();
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            gyroSensorChangedListener.OnGyroSensorChanged(e.Values[1], e.Values[0]);
        }

        private void SetSettings()
        {
            sensorManager = (SensorManager)context.GetSystemService(Context.SensorService);
            gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);

            if (gyroscopeSensor == null)
            {
                Log.Error("GyroSenso", "gyroscope not found");
            }

            sensorManager.RegisterListener(this, gyroscopeSensor, SensorDelay.Game);
        }
        
    }


    public interface IGyroSensorChangedListener
    {
        void OnGyroSensorChanged(float x, float y);
    }
}