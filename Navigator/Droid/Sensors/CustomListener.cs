using System;
using Android.Hardware;

namespace Navigator.Droid.Sensors
{
    public class CustomListener
    {
        private readonly SensorManager _sensorManager;

        public CustomListener(SensorManager manager)
        {
            _sensorManager = manager;
            AccelerationProcessor = new Acceleration(_sensorManager);
            RotationProcessor = new Rotation(_sensorManager);
        }

        public void Dispose()
        {
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // We dont care   
        }

        public void OnSensorChanged(SensorEvent e)
        {
            AccelerationProcessor.SensorChangedProcess(e);
            RotationProcessor.SensorChangedProcess(e);
            if (AccelerationProcessor.HasValue)
            {
                StepDetector.passValue(AccelerationProcessor.Value);
            }
        }

        #region <Sensors>

        public Acceleration AccelerationProcessor;
        public Rotation RotationProcessor;
        public StepDetector StepDetector = new StepDetector();

        #endregion
    }
}