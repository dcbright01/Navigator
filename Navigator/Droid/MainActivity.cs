using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Navigator.Droid.Extensions;
using Navigator.Droid.UIElements;
using Navigator.Pathfinding.Graph;

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
        private double _azimuth;
        private float[] mGravity;
        private float[] mGeomagnetic;
        private bool inDebug = false;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _sensorManager = (SensorManager)GetSystemService(SensorService);
            _step.Taken += _step_Taken;
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
                _azimuthText = FindViewById<TextView>(Resource.Id.azimuth);
                _azimuthText.Text = string.Format("Azimuth is {0:F1}", RadianToDegree(_azimuth));
                _stepText = FindViewById<TextView>(Resource.Id.stepCounter);
                _stepText.Text = string.Format("Steps: {0}", _stepCount);
            });
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                SensorDelay.Ui);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gyroscope), SensorDelay.Ui);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.MagneticField), SensorDelay.Ui);
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
                    mGravity = e.Values.ToArray();

                    long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    // only want 10 values per second to be fed 
                    if ((currentMilliseconds - initialMilliseconds) > 100)
                    {
                        _step.passValue((double)mGravity[0], (double)mGravity[1], (double)mGravity[2]);
                        initialMilliseconds = currentMilliseconds;
                    }
                    break;
                case SensorType.Gyroscope:
                    break;
                case SensorType.MagneticField:
                    mGeomagnetic = e.Values.ToArray();
                    break;
            }

            calculateAzimuth();
        }

        private void _step_Taken(int steps)
        {
            _stepCount = steps;
            if (inDebug)
            {
                SetContentView(Resource.Layout.Debug);
                _stepText = FindViewById<TextView>(Resource.Id.stepCounter);
                string x = string.Format("Steps: {0}", _stepCount);
                _stepText.Text = x;
            }   
        }

        //http://www.codingforandroid.com/2011/01/using-orientation-sensors-simple.html
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
                .SetPositiveButton("(Start) Its a trap !", (s, args) =>
                {
                    // User pressed yes
					DrawPointOnMap((int)motionEvent.GetX(), (int)motionEvent.GetY());
                })
                .SetNegativeButton("(End) Its still a fucking trap !", (s, args) =>
                {
                    // User pressed no 
                })
                .SetMessage("O noes ! A long press ! What do what do !?!")
                .SetTitle("Pick some shit")
                .Show();
        }

        // Translates coordinates of the imageview touch event to coordinates of the bitmap image
        // so that points can be drawn on the correct place to account for any offset.
        private float[] RelativeToAbsoluteCoordinates(int x, int y)
        {
            float[] point = new float[] { x, y };
            Matrix inverse = new Matrix();
            _imgMap.ImageMatrix.Invert(inverse);
            inverse.MapPoints(point);
            return point;
        }

        // Draws a point on the bitmap floorplan image at a specified (x,y). Colour is magenta
		// so it stands out.
		private void DrawPointOnMap(int x, int y)
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

			Paint paint = new Paint();
			paint.AntiAlias = true;
			paint.Color = Color.Magenta;

            // Draw the damn point.
            float[] point = RelativeToAbsoluteCoordinates(x, y);
            Canvas canvas = new Canvas(bitmap);
            canvas.DrawCircle(point[0], point[1], 20, paint);

			// Change the displayed image to the new one and update current map image
			// to ensure that change is consistent when tabs are changed.
			_imgMap.SetAdjustViewBounds(true);
			_currentMapImage = bitmap;
			_imgMap.SetImageBitmap(_currentMapImage);

			// Avoiding a memory leak upon redrawing the image many times
			//bitmap.Dispose();
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