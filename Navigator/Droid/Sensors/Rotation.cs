using System.Linq;
using Android.Hardware;

namespace Navigator.Droid.Sensors
{
    public class Rotation : SensorProcessorBase<double>
    {
        private float[] _geomagnetic;
        private float[] _gravity;

        public Rotation(SensorManager manager) : base(manager)
        {
            AcceptedSensorTypes.Add(SensorType.MagneticField);
            AcceptedSensorTypes.Add(SensorType.Gravity);
            ReadingDelay = 500;
        }

        public override void SensorChangedProcess(SensorEvent e)
        {
            switch (e.Sensor.Type)
            {
                case SensorType.MagneticField:
                    _geomagnetic = e.Values.ToArray();
                    break;
                case SensorType.Gravity:
                    _gravity = e.Values.ToArray();
                    break;
            }

            if (_gravity != null && _geomagnetic != null)
            {
                var R = new float[9];
                var I = new float[9];
                var success = SensorManager.GetRotationMatrix(R, I, _gravity, _geomagnetic);
                if (success)
                {
                    var orientation = new float[3];
                    SensorManager.GetOrientation(R, orientation);
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