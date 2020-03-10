using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Exception = System.Exception;

namespace Familia.Games.Sensors {
	class AccelerometerSensor : Object, ISensorEventListener {
		private SensorManager sensorManager;
		private Sensor sensor;
		private Context context;
		private IAccelerometerSensorChangedListener sensorChangedListener;


		public AccelerometerSensor(Context context) {
			this.context = context;
			SetSettings();
		}

		public void SetListener(IAccelerometerSensorChangedListener listener) {
			sensorChangedListener = listener;
		}

		public new void Dispose() {
			Log.Error("Accelerometer SENSOR", "dispose");
			sensorManager.UnregisterListener(this);
		}

		public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy) { }

		public void OnSensorChanged(SensorEvent e) {
			try {
				Log.Error("Accelerometer SENSOR", "changed values: " + e.Values[1] + ", " + e.Values[0] + ", " + e.Values[2]);
				sensorChangedListener.OnSensorChanged(e.Values[1], e.Values[0], e.Values[2]);
			} catch (Exception ex) {
				Log.Error("Accelerometer SENSOR Error", ex.Message);
			}
		}


		private void SetSettings() {
			sensorManager = (SensorManager) context.GetSystemService(Context.SensorService);
			sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

			if (sensor == null) {
				Log.Error("Accelerometer SENSOR", " not found");
				return;
			}

			sensorManager.RegisterListener(this, sensor, SensorDelay.Game);
		}
	}


	public interface IAccelerometerSensorChangedListener {
		void OnSensorChanged(float x, float y, float z);
	}
}