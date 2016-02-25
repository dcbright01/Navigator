using System;
using System.Collections.Generic;
using Android.Hardware;
using Navigator.Primitives;

namespace Navigator.Droid.Sensors
{
    public abstract class SensorProcessorBase<T>
    {
        /// <summary>
        ///     Sensor types we want to recieve updates about
        /// </summary>
        public readonly List<SensorType> AcceptedSensorTypes;

        protected DateTime LastReading = DateTime.MinValue;

        protected long ReadingDelay = 0;

        /// <summary>
        ///     Keeps track of the last 10 values produced by this sensor processor
        /// </summary>
        public FixedSizeQueue<T> ValueHistory;


        protected SensorProcessorBase(SensorManager manager)
        {
            ValueHistory = new FixedSizeQueue<T>(10);
            AcceptedSensorTypes = new List<SensorType>();
            SensorManager = manager;
        }

        /// <summary>
        ///     Value we obtained from the last processing cycle
        /// </summary>
        public T Value { get; protected set; }

        public long MsLastReading
        {
            get { return (long) DateTime.Now.Subtract(LastReading).TotalMilliseconds; }
        }

        protected SensorManager SensorManager { get; private set; }

        public bool HasValue
        {
            get { return Value != null; }
        }

        public abstract void SensorChangedProcess(SensorEvent e);

        public void OnSensorChanged(SensorEvent e)
        {
            if (!AcceptedSensorTypes.Contains(e.Sensor.Type))
                return;

            if (MsLastReading < ReadingDelay)
                return;

            // Two checks passed we now perform the action

            SensorChangedProcess(e);

            // Update time value
            LastReading = DateTime.Now;
        }
    }
}