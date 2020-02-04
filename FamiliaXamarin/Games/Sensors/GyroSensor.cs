using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Exception = System.Exception;

namespace Familia.Games.Sensors {
	class GyroSensor : Object, ISensorEventListener {
		private SensorManager sensorManager;
		private Sensor gyroscopeSensor;
		private Context context;
		private IGyroSensorChangedListener gyroSensorChangedListener;


		public GyroSensor(Context context) {
			this.context = context;
			SetSettings();
		}

		public void SetGyroListener(IGyroSensorChangedListener listener) {
			gyroSensorChangedListener = listener;
		}

		public new void Dispose() {
			Log.Error("GYRO SENSOR", "dispose");
			sensorManager.UnregisterListener(this);
		}

		public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy) { }

		public void OnSensorChanged(SensorEvent e) {
			try {
				gyroSensorChangedListener.OnGyroSensorChanged(e.Values[1], e.Values[0], e.Values[2]);
			} catch (Exception ex) {
				Log.Error("GYRO SENSOR Error", ex.Message);
			}
		}


		private void SetSettings() {
			sensorManager = (SensorManager) context.GetSystemService(Context.SensorService);
//            gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
			gyroscopeSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

			if (gyroscopeSensor == null) {
				Log.Error("GYRO SENSOR", "gyroscope not found");
				return;
			}

			sensorManager.RegisterListener(this, gyroscopeSensor, SensorDelay.Game);
		}
	}


	public interface IGyroSensorChangedListener {
		void OnGyroSensorChanged(float x, float y, float z);
	}
}