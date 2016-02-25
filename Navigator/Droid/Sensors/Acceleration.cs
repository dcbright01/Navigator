using System.Linq;
using Android.Hardware;
using Android.Opengl;
using Navigator.Primitives;

namespace Navigator.Droid.Sensors
{
    public class Acceleration : SensorProcessorBase<Vector3>
    {
        private float[] _accelerometer;
        private float[] _geomagnetic;
        private float[] _gravity;

        public Acceleration(SensorManager manager) : base(manager)
        {
            AcceptedSensorTypes.Add(SensorType.Accelerometer);
            AcceptedSensorTypes.Add(SensorType.MagneticField);
            AcceptedSensorTypes.Add(SensorType.Gravity);
            ReadingDelay = 10;
        }

        public override void SensorChangedProcess(SensorEvent e)
        {
            // Check if we even want to read this sensor
            switch (e.Sensor.Type)
            {
                case SensorType.Accelerometer:
                    _accelerometer = e.Values.ToArray();
                    break;
                case SensorType.MagneticField:
                    _geomagnetic = e.Values.ToArray();
                    break;
                case SensorType.Gravity:
                    _gravity = e.Values.ToArray();
                    break;
            }


            if (_gravity != null && _geomagnetic != null && _accelerometer != null)
            {
                var R = new float[16];
                var I = new float[16];
                SensorManager.GetRotationMatrix(R, I, _gravity, _geomagnetic);
                var relativacc = new float[4];
                var inv = new float[16];

                relativacc[0] = _accelerometer[0];
                relativacc[1] = _accelerometer[1];
                relativacc[2] = _accelerometer[2];
                relativacc[3] = 0;

                var A_W = new float[4];

                Matrix.InvertM(inv, 0, R, 0);
                Matrix.MultiplyMV(A_W, 0, inv, 0, relativacc, 0);
                Value = new Vector3(A_W);
                // Value = new Vector3(_accelerometer);
                ValueHistory.Enqueue(Value);
                if (OnValueChanged != null)
                {
                    OnValueChanged(Value);
                }
            }
        }

        #region <Event stuff>

        public event OnValueChangedHandler OnValueChanged;

        public delegate void OnValueChangedHandler(Vector3 value);

        #endregion
    }
}