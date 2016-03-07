using System;
using System.Linq;
using System.Collections.Generic; 
using Android.Hardware;
using Navigator.Helpers; 

namespace Navigator.Droid.Sensors
{
	public class Rotation : SensorProcessorBase<double>
	{
		// http://stackoverflow.com/questions/17979238/android-getorientation-azimuth-gets-polluted-when-phone-is-tilted/17981374#17981374

		private List<float[]> mRotHist = new List<float[]>();
		private int mRotHistIndex;
		// Change the value so that the azimuth is stable and fit your requirement
		private int mHistoryMaxLength = 100;
		float[] mGravity;
		float[] mMagnetic;
		float[] mRotationMatrix = new float[9];
		// the direction of the back camera, only valid if the device is tilted up by
		// at least 25 degrees.
		private float mFacing = float.NaN;

		float TWENTY_FIVE_DEGREE_IN_RADIAN = 0.436332313f;
		float ONE_FIFTY_FIVE_DEGREE_IN_RADIAN = 2.7052603f;

		public Rotation(SensorManager manager) : base(manager)
		{
			AcceptedSensorTypes.Add(SensorType.MagneticField);
			AcceptedSensorTypes.Add(SensorType.Gravity);
		}

		public override void SensorChangedProcess(SensorEvent e)
		{
			switch (e.Sensor.Type)
			{
			case SensorType.MagneticField:
				mMagnetic = e.Values.ToArray(); 
				break;
			case SensorType.Gravity:
				mGravity = e.Values.ToArray(); 
				break;
			}

			if (mGravity != null && mMagnetic != null)
			{
				var success = SensorManager.GetRotationMatrix(mRotationMatrix, null, mGravity, mMagnetic);
				if (success)
				{
					// inclination is the degree of tilt by the device independent of orientation (portrait or landscape)
					// if less than 25 or more than 155 degrees the device is considered lying flat
					float inclination = (float) Math.Acos(mRotationMatrix[8]);
					if (inclination < TWENTY_FIVE_DEGREE_IN_RADIAN
					    || inclination > ONE_FIFTY_FIVE_DEGREE_IN_RADIAN) 
					{
						// mFacing is undefined, so we need to clear the history
						ClearRotHist ();
						mFacing = float.NaN;
					} else 
					{
						SetRotHist();
						// mFacing = azimuth is in radian
						mFacing = FindFacing(); 
					}
						
					Value = mFacing; 
					ValueHistory.Enqueue(Value);
					if (OnValueChanged != null)
					{
						OnValueChanged(Value);
					}
				}
			}
		}

		private void ClearRotHist()
		{
			mRotHist.Clear();
			mRotHistIndex = 0;
		}

		private void SetRotHist()
		{
			float[] hist = (float[]) mRotationMatrix.Clone();
			if (mRotHist.Count == mHistoryMaxLength) 
			{
				mRotHist.RemoveAt(mRotHistIndex);
			}
			mRotHist.Insert(mRotHistIndex++, hist);
			mRotHistIndex %= mHistoryMaxLength;
		}

		private float FindFacing()
		{
			float[] averageRotHist = Average(mRotHist);
			return (float)Math.Atan2 (-averageRotHist [2], -averageRotHist [5]); 
		}

		public float[] Average(List<float[]> values)
		{
			float[] result = new float[9];
			foreach (float[] value in values)
			{
				for (int i = 0; i < 9; i++)
				{
					result[i] += value[i];
				}
			}

			for (int i = 0; i < 9; i++)
			{
				result[i] = result[i] / values.Count; 
			}

			return result;
		}

		#region <Event stuff>

		public event OnValueChangedHandler OnValueChanged;

		public delegate void OnValueChangedHandler(double value);

		#endregion
	}
}