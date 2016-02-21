using System;
using Android.App;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using Navigator.Droid.Extensions;
using Navigator.Droid.Sensors;
using Navigator.Droid.UIElements;
using Navigator.Helpers;
using Navigator.Primitives;
using Navigator.Pathfinding;

namespace Navigator.Droid
{
    [Activity(Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        #region <ISensorEventListener>
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            _sensorListener.OnSensorChanged(e);
        }
        #endregion

        private Collision _col = new Collision();
        private int stepCounter = 0;
        private Tuple<float, float> realPos;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _sensorManager = (SensorManager) GetSystemService(SensorService);
            _sensorListener = new CustomListener(_sensorManager);
            _sensorListener.AccelerationProcessor.OnValueChanged += AccelerationProcessorOnValueChanged;
            _sensorListener.RotationProcessor.OnValueChanged += RotationProcessorOnValueChanged;
            _sensorListener.StepDetector.OnStep += StepDetectorOnStep;

            // Small pathfinding test

            /*
            var asset = Assets.Open("test.xml");
			Stopwatch sw = new Stopwatch ();
			sw.Start ();
            var g = Graph.Load(asset);
			sw.Stop ();
			long time = (sw.ElapsedMilliseconds / 1000);
			var start = g.Vertices.First(x=>x=="789-717");
			var end = g.Vertices.First(x => x == "1479-1167");
            var path = g.FindPath(start,end);
            var path2 = g.FindPath(start,end);
            */

            var asset = Assets.Open("wall.xml");
            var g = Graph.Load(asset);

            _col.addGraph(g);
            _col.GiveStartingLocation(7.0f, 0.0f);
            realPos = Tuple.Create<float, float>(7.0f, 0.0f);
            _col.passHeading(90);

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
                _stepText = FindViewById<TextView>(Resource.Id.stepCounter);
                _azimuthText = FindViewById<TextView>(Resource.Id.azimuth);
                _XAccelText = FindViewById<TextView>(Resource.Id.XAccel);
                _YAccelText = FindViewById<TextView>(Resource.Id.YAccel);
                _ZAccelText = FindViewById<TextView>(Resource.Id.ZAccel);
                _realX = FindViewById<TextView>(Resource.Id.realX);
                _realY = FindViewById<TextView>(Resource.Id.realY);
            });
        }

        private void StepDetectorOnStep(bool stationaryStart)
        {
            Tuple<float, float> tempPos = _col.testStepTrigger();
            if (!Tuple.Equals(tempPos, realPos))
            {
                if (stationaryStart)
                    stepCounter += 2;

                stepCounter++;
                realPos = tempPos;
            }
            if (inDebug)
            {
                RunOnUiThread(() => {
                    _stepText.Text = string.Format("Steps: {0}", stepCounter);
                });
                _realX.Text = string.Format("Real X: {0}", realPos.Item1);
                _realY.Text = string.Format("Real Y: {0}", realPos.Item2);
            }
        }

        private void RotationProcessorOnValueChanged(double value)
        {
            if (inDebug)
            {
                RunOnUiThread(
                    () => { _azimuthText.Text = string.Format("Azimuth: {0}", VarunMaths.RadianToDegree(value)); });
            }
        }

        private void AccelerationProcessorOnValueChanged(Vector3 value)
        {
            if (inDebug)
            {
                RunOnUiThread(() =>
                {
                    _XAccelText.Text = string.Format("East accel : {0}", value.X);
                    _YAccelText.Text = string.Format("North accel : {0}", value.Y);
                    _ZAccelText.Text = string.Format("Forward accel : {0}", value.Z);
                });
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                SensorDelay.Ui);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.MagneticField),
                SensorDelay.Ui);
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gravity), SensorDelay.Ui);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }

        private void ImgMapOnLongPress(object sender, MotionEvent motionEvent)
        {
            new AlertDialog.Builder(this)
                .SetPositiveButton("Start Location", (s, args) =>
                {
                    // User pressed yes
                    ResetMap();

                    var point = RelativeToAbsoluteCoordinates((int) motionEvent.GetX(), (int) motionEvent.GetY());
                    startPoint = new Vector2(point[0], point[1]);
                    DrawOnMap();
                })
                .SetNegativeButton("End Location", (s, args) =>
                {
                    // User pressed no
                    ResetMap();

                    var point = RelativeToAbsoluteCoordinates((int) motionEvent.GetX(), (int) motionEvent.GetY());
                    endPoint = new Vector2(point[0], point[1]);
                    DrawOnMap();
                })
                .SetMessage("Start or end location?")
                .SetTitle("Pick some shit")
                .Show();
        }

        // Draws the current points and lines on the map.
        private void DrawOnMap()
        {
            var myOptions = new BitmapFactory.Options();
            myOptions.InDither = true;
            myOptions.InScaled = false;
            myOptions.InPreferredConfig = Bitmap.Config.Argb8888;
            myOptions.InPurgeable = true;
            myOptions.InMutable = true;
            Bitmap bitmap;

            // If the current map image is not initialised, initialise it to the correct one.
            if (_currentMapImage == null)
            {
                if (!_isDrawingGrid)
                {
                    _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor, myOptions);
                }
                else
                {
                    _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid, myOptions);
                }
            }

            bitmap = _currentMapImage;

            var paint = new Paint
            {
                AntiAlias = true,
                Color = Color.Magenta
            };

            // Draw the damn points.
            var canvas = new Canvas(bitmap);

            if (startPoint != null)
            {
                canvas.DrawCircle(startPoint.X, startPoint.Y, 20, paint);
            }

            if (endPoint != null)
            {
                canvas.DrawCircle(endPoint.X, endPoint.Y, 20, paint);
            }

            if ((startPoint != null) && (endPoint != null))
            {
                paint.StrokeWidth = 10;
                canvas.DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, paint);
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
            var myOptions = new BitmapFactory.Options();
            myOptions.InDither = true;
            myOptions.InScaled = false;
            myOptions.InPreferredConfig = Bitmap.Config.Argb8888;
            myOptions.InPurgeable = true;
            myOptions.InMutable = true;

            if (!_isDrawingGrid)
            {
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor, myOptions);
            }
            else
            {
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid, myOptions);
            }

            _imgMap.SetImageBitmap(_currentMapImage);
        }

        // Translates coordinates of the imageview touch event to coordinates of the bitmap image
        // so that points can be drawn on the correct place to account for any offset.
        private float[] RelativeToAbsoluteCoordinates(int x, int y)
        {
            float[] point = {x, y};
            var inverse = new Matrix();
            _imgMap.ImageMatrix.Invert(inverse);
            inverse.MapPoints(point);
            return point;
        }

        private void DrawGridButtonToggle(object sender, EventArgs eventArgs)
        {
            var myOptions = new BitmapFactory.Options();
            myOptions.InDither = true;
            myOptions.InScaled = false;
            myOptions.InPreferredConfig = Bitmap.Config.Argb8888;
            myOptions.InPurgeable = true;
            myOptions.InMutable = true;

            if (_btnDrawGridToggle.Checked)
            {
                _isDrawingGrid = true;
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid, myOptions);
                DrawOnMap();
            }
            else
            {
                _isDrawingGrid = false;
                _currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor, myOptions);
                DrawOnMap();
            }
        }

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
        private long initialMilliseconds = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
        private TextView _azimuthText;
        private TextView _stepText;
        private TextView _XAccelText;
        private TextView _YAccelText;
        private TextView _ZAccelText;
        private TextView _realX;
        private TextView _realY;
        private bool inDebug;
        private CustomListener _sensorListener;

        #endregion

        #region <Position Data>

        private Vector2 startPoint;
        private Vector2 endPoint;

        #endregion
    }
}