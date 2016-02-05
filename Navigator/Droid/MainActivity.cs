using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Android.Opengl; 
using Navigator.Droid.Extensions;
using Navigator.Droid.UIElements;
using Navigator.Pathfinding.Graph;
using Navigator.Primitives;

namespace Navigator.Droid
{
    [Activity(Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        #region < Properties > 

        private Bitmap _currentMapImage;
        private bool _isDrawingGrid;

        #endregion

        #region <UI Elements>

        private ToggleButton _btnDrawGridToggle;
        private CustomImageView _imgMap;

        #endregion

        #region <Sensors>

        private SensorManager _sensorManager;
        private StepDetector _step = new StepDetector();
        private long initialMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        private int _stepCount;
        private TextView _azimuthText;
        private TextView _stepText;
		private TextView _XAccelText; 
		private TextView _YAccelText; 
		private TextView _ZAccelText; 
        private double _azimuth;
        private float[] mGravity;
        private float[] mGeomagnetic;
		private float[] mAccelerometer; 
		private float[] mLinear; 
        private bool inDebug = false;

        #endregion

        #region <Position Data>

        private Vector2 startPoint;
        private Vector2 endPoint;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _step.OnStep += OnStepTaken;
            _sensorManager = (SensorManager)GetSystemService(SensorService);
            // Small pathfinding test
            /*
            var asset = Assets.Open("pbSmall.xml");
            var g = Graph.Load(asset);

            var start = g.Vertices.OrderBy(x => x.DistanceTo(new Vertex() { X = 201, Y = 379 })).First();
            var end = g.Vertices.OrderBy(x => x.DistanceTo(new Vertex() { X = 621, Y = 149 })).First();
            var path = g.FindPath(start,end);
            var path2 = g.FindPath(start,end);
            */
            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ScaleImage);

            // Register our tabs
            ActionBar.AddNewTab("Main", () =>
            {
                inDebug = false;
                SetContentView(Resource.Layout.ScaleImage);
                _imgMap = FindViewById<CustomImageView>(Resource.Id.imgMap);
                _imgMap.LongPress += ImgMapOnLongPress;
                // Reset to saved state
                if (_currentMapImage != null)
                    _imgMap.SetImageBitmap(_currentMapImage);

            });
            ActionBar.AddNewTab("Settings", () =>
            {
                inDebug = false;
                SetContentView(Resource.Layout.ImageSettings);
                _btnDrawGridToggle = FindViewById<ToggleButton>(Resource.Id.drawGridCB);
                _btnDrawGridToggle.Click += DrawGridButtonToggle;

                // Reset to saved state
                if (_isDrawingGrid)
                    _btnDrawGridToggle.Checked = true;
            });
            ActionBar.AddNewTab("Debug", () =>
            {
                inDebug = true;
                SetContentView(Resource.Layout.Debug);
            });
        }

        private void OnStepTaken(int steps)
        {
            _stepCount = steps;
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                SensorDelay.Ui);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.MagneticField), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gravity), SensorDelay.Ui);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
				var sensor = e.Sensor;

				switch (sensor.Type)
				{
					case SensorType.Accelerometer:
						mAccelerometer = e.Values.ToArray(); 
						break;
					case SensorType.MagneticField:
						mGeomagnetic = e.Values.ToArray();
						break;
					case SensorType.Gravity:
						mGravity = e.Values.ToArray();
						break;
				}
					
				getHorizontalAcceleration(); 

				if (inDebug)
				{
					_stepText = FindViewById<TextView>(Resource.Id.stepCounter);
					string currentText = string.Format("Steps: {0}", _stepCount);
					RunOnUiThread(() => _stepText.Text = currentText);
				}
        }

		private void getHorizontalAcceleration()
		{
			if (mGravity != null && mGeomagnetic != null) 
			{
				float[] R = new float[16];
				float[] I = new float[16];
				SensorManager.GetRotationMatrix (R, I, mGravity, mGeomagnetic);
				float[] relativacc = new float[4];
				float[] inv = new float[16];

				relativacc[0] = mAccelerometer[0];
				relativacc[1] = mAccelerometer[1];
				relativacc[2] = mAccelerometer[2];
				relativacc[3] = 0;

				float[] A_W = new float[4]; 

				Android.Opengl.Matrix.InvertM (inv, 0, R, 0); 
				Android.Opengl.Matrix.MultiplyMV (A_W, 0, inv, 0, relativacc, 0); 

				_step.passValue((double) A_W[0], (double) A_W[1], (double) A_W[2]);

				if (inDebug) 
				{
					SetContentView (Resource.Layout.Debug);
					_XAccelText = FindViewById<TextView> (Resource.Id.XAccel);
					_YAccelText = FindViewById<TextView> (Resource.Id.YAccel);
					_ZAccelText = FindViewById<TextView> (Resource.Id.ZAccel);
					_XAccelText.Text = string.Format ("East accel is {0:F1}", A_W [0]);
					_YAccelText.Text = string.Format ("North accel is {0:F1}", A_W [1]);
					_ZAccelText.Text = string.Format ("Forward accel is {0:F1}", A_W [2]);
				}
			}
		}

        //http://www.codingforandroid.com/2011/01/using-orientation-sensors-simple.html
        private void calculateAzimuth()
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
                    _azimuth = orientation[0]; // orientation contains: azimut, pitch and roll

                    if(inDebug)
                    {
                        SetContentView(Resource.Layout.Debug);
                        _azimuthText = FindViewById<TextView>(Resource.Id.azimuth);
                        _azimuthText.Text = string.Format("Azimuth is {0:F1}", RadianToDegree(_azimuth));
                    }
                }
            }
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private void ImgMapOnLongPress(object sender, MotionEvent motionEvent)
        {
            new AlertDialog.Builder(this)
                .SetPositiveButton("Start Location", (s, args) =>
                {
                    // User pressed yes
                    ResetMap();
                    startPoint = new Vector2(motionEvent.GetX(), motionEvent.GetY());
                    DrawPointsOnMap();
                })
                .SetNegativeButton("End Location", (s, args) =>
                {
                    // User pressed no
                    ResetMap();
                    endPoint = new Vector2(motionEvent.GetX(), motionEvent.GetY());
                    DrawPointsOnMap();
                })
                .SetMessage("Start or end location?")
                .SetTitle("Pick some shit")
                .Show();
        }

        // Draws the current start/end points on the map.
        private void DrawPointsOnMap()
        {
            BitmapFactory.Options myOptions = new BitmapFactory.Options ();
            myOptions.InDither = true;
            myOptions.InScaled = false;
            myOptions.InPreferredConfig = Bitmap.Config.Argb8888;
            myOptions.InPurgeable = true;
            myOptions.InMutable = true;
            Bitmap bitmap;

            // If the current map image is not initialised, initialise it to the correct one.
            if (_currentMapImage == null) {
                if (!_isDrawingGrid) {
                    _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor, myOptions);
                } else {
                    _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid, myOptions);
                }
            }

            bitmap = _currentMapImage;

            Paint paint = new Paint
            {
                AntiAlias = true,
                Color = Color.Magenta
            };

            // Draw the damn points.
            Canvas canvas = new Canvas(bitmap);
            float[] point;

            if(startPoint != null) {
                point = RelativeToAbsoluteCoordinates((int)startPoint.X, (int)startPoint.Y);
                canvas.DrawCircle(point[0], point[1], 20, paint);
            }

            if(endPoint != null) {
                point = RelativeToAbsoluteCoordinates((int)endPoint.X, (int)endPoint.Y);
                canvas.DrawCircle(point[0], point[1], 20, paint); 
            }

            // Change the displayed image to the new one and update current map image
            // to ensure that change is consistent when tabs are changed.
            _imgMap.SetAdjustViewBounds(true);
            _currentMapImage = bitmap;
            _imgMap.SetImageBitmap(_currentMapImage);
        }

        // Resets the floorplan bitmap to a mutable un-edited version
        private void ResetMap()
        {
            BitmapFactory.Options myOptions = new BitmapFactory.Options ();
            myOptions.InDither = true;
            myOptions.InScaled = false;
            myOptions.InPreferredConfig = Bitmap.Config.Argb8888;
            myOptions.InPurgeable = true;
            myOptions.InMutable = true;

            if (!_isDrawingGrid) {
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor, myOptions);
            } else {
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid, myOptions);
            }

            _imgMap.SetImageBitmap(_currentMapImage);
        }

        // Translates coordinates of the imageview touch event to coordinates of the bitmap image
        // so that points can be drawn on the correct place to account for any offset.
        private float[] RelativeToAbsoluteCoordinates(int x, int y)
        {
            float[] point = new float[] { x, y };
			Android.Graphics.Matrix inverse = new Android.Graphics.Matrix();
            _imgMap.ImageMatrix.Invert(inverse);
            inverse.MapPoints(point);
            return point;
        }
            
        private void DrawGridButtonToggle(object sender, EventArgs eventArgs)
        {
            if (_btnDrawGridToggle.Checked)
            {
                _isDrawingGrid = true;
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid);
            }
            else
            {
                _isDrawingGrid = false;
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor);
            }
        }
    }
}