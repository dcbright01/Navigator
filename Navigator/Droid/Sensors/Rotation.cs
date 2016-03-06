using System.Linq;
using Android.Hardware;

namespace Navigator.Droid.Sensors
{
	public class Rotation : SensorProcessorBase<double>
	{
		private float[] _geomagnetic = new float[3];
		private float[] _gravity = new float[3];

		public Rotation(SensorManager manager) : base(manager)
		{
			AcceptedSensorTypes.Add(SensorType.MagneticField);
			AcceptedSensorTypes.Add(SensorType.Gravity);
		}

		public override void SensorChangedProcess(SensorEvent e)
		{
			float alpha = 0.97f;

			switch (e.Sensor.Type)
			{
			case SensorType.MagneticField:
				_geomagnetic [0] = alpha * _geomagnetic [0] + (1 - alpha) * e.Values [0];
				_geomagnetic [1] = alpha * _geomagnetic [1] + (1 - alpha) * e.Values [1];
				_geomagnetic [2] = alpha * _geomagnetic [2] + (1 - alpha) * e.Values [2];
				break;
			case SensorType.Gravity:
				_gravity [0] = alpha * _gravity [0] + (1 - alpha) * e.Values [0];
				_gravity [1] = alpha * _gravity [1] + (1 - alpha) * e.Values [1];
				_gravity [2] = alpha * _gravity [2] + (1 - alpha) * e.Values [2];
				break;
			}

			if (_gravity != null && _geomagnetic != null)
			{
				var R = new float[9];
				var I = new float[9];
				var success = SensorManager.GetRotationMatrix(R, I, _gravity, _geomagnetic);
				if (success)
				{
					float[] remappedRotationMatrix = new float[9];
					SensorManager.RemapCoordinateSystem (R, Android.Hardware.Axis.X, Android.Hardware.Axis.Z, remappedRotationMatrix);

					var orientation = new float[3];
					SensorManager.GetOrientation(remappedRotationMatrix, orientation);
					Value = orientation[0]; // orientation contains: azimut, pitch and roll
					ValueHistory.Enqueue(Value);
					if (OnValueChanged != null)
					{
						OnValueChanged(Value);
					}
				}
			}
		}

		#region <Event stuff>

		public event OnValueChangedHandler OnValueChanged;

		public delegate void OnValueChangedHandler(double value);

		#endregion
	}
}