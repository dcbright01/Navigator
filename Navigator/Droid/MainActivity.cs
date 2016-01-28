using Android.App;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using System; 
using System.Linq;

namespace Navigator.Droid
{
	[Activity (Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, ISensorEventListener
	{
		private SensorManager _sensorManager;
		private StepDetector step = new StepDetector(); 
		long initialMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		       
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			_sensorManager = (SensorManager)GetSystemService(SensorService);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
		}

		protected override void OnResume()
		{
			base.OnResume();
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
				SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gyroscope), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor (SensorType.MagneticField), SensorDelay.Ui); 
		}

		protected override void OnPause()
		{
			base.OnPause();
			_sensorManager.UnregisterListener(this);
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{

		}

		float[] mGravity;
		float[] mGeomagnetic;


		public void OnSensorChanged(SensorEvent e)
		{
			var sensor = e.Sensor;
			step.Taken += Step_Taken;
			switch (sensor.Type) 
			{
				case SensorType.Accelerometer:
					mGravity = e.Values.ToArray (); 

					long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
					// only want 10 values per second to be fed 
					if ((currentMilliseconds - initialMilliseconds) > 100) 
					{
						step.passValue ((double)mGravity [0], (double)mGravity [1], (double)mGravity [2]); 
						initialMilliseconds = currentMilliseconds; 
					}
					break;
				case SensorType.Gyroscope:
					break;
				case SensorType.MagneticField:
					mGeomagnetic = e.Values.ToArray(); 
					break; 
			}
				
			calculateAzimuth (); 
		}

		public int stepCounter = 0;

		public void Step_Taken(object sender, System.EventArgs e)
		{
			stepCounter++;
			TextView stepText = FindViewById<TextView>(Resource.Id.stepCounter);
			stepText.Text = string.Format("Steps: {0}", stepCounter);
		}

		// http://www.codingforandroid.com/2011/01/using-orientation-sensors-simple.html
		public void calculateAzimuth() 
		{
			if (mGravity != null && mGeomagnetic != null) 
			{
				float[] R = new float[9];
				float[] I = new float[9];
				bool success = SensorManager.GetRotationMatrix(R, I, mGravity, mGeomagnetic);
				if (success) 
				{
					float[] orientation = new float[3];
					SensorManager.GetOrientation(R, orientation);
					double azimuth = orientation[0]; // orientation contains: azimut, pitch and roll

					TextView azimuthText = FindViewById<TextView> (Resource.Id.azimuth); 
					azimuthText.Text = string.Format ("Azimuth is {0:F1}", RadianToDegree(azimuth)); 
				}
			}
		}

		private double RadianToDegree(double angle) 
		{
			return angle * (180.0 / Math.PI); 
		}
	}
}


