using Android.Content;
using Android.Hardware;
using Android.Util;

namespace Familia.Activity_Tracker
{
    class StepCounterSensor : Java.Lang.Object, ISensorEventListener
    {
        private Context context;
        private SensorManager sensorManager;
        private Sensor sensor;
        private IStepCounterSensorChangedListener listener;
        
//        public StepCounterSensor()
//        {
//            SetSettings();
//        }

        public StepCounterSensor(Context context)
        {
            this.context = context;
            SetSettings();
        }
        
        private void SetSettings()
        {
            sensorManager = (SensorManager) context.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(SensorType.StepCounter);
            sensorManager.RegisterListener(this, sensor, SensorDelay.Fastest);
           
        }

        public void SetListener(IStepCounterSensorChangedListener listener)
        {
            this.listener = listener;
        }
        
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) {}

        public void OnSensorChanged(SensorEvent e)
        {
           listener.OnStepCounterSensorChanged((long)e.Values[0]);
        }

        public void CloseListener()
        {
            sensorManager.UnregisterListener(this);
            Log.Error("StepCounterListener", "unregistred");
        }

        public interface IStepCounterSensorChangedListener
        {
            void OnStepCounterSensorChanged(long count);
        }
    }
}