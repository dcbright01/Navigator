using System;
using Android.App;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using Navigator.Droid.Extensions;
using Navigator.Droid.Helpers;
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


            SetContentView(Resource.Layout.ScaleImage);

            // Register our sensor listener
            _sensorManager = (SensorManager) GetSystemService(SensorService);
            _sensorListener = new CustomListener(_sensorManager);
            _sensorListener.AccelerationProcessor.OnValueChanged += AccelerationProcessorOnValueChanged;
            _sensorListener.RotationProcessor.OnValueChanged += RotationProcessorOnValueChanged;
            _sensorListener.StepDetector.OnStep += StepDetectorOnStep;

            // Decode resources for pathfinding test
            MapMaker.PlainMap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloor);
            MapMaker.PlainMapGrid = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid);
            MapMaker.UserRepresentation = BitmapFactory.DecodeResource(Resources, Resource.Drawable.arrow);
            MapMaker.Initialize();


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

            var asset = Assets.Open("test.xml");
            var g = Graph.Load(asset);
            MapMaker.PathfindingGraph = g;

            _col.addGraph(g);
            _col.GiveStartingLocation(787.0f, 717.0f);
            realPos = Tuple.Create<float, float>(787.0f, 717.0f);
            _col.passHeading(90);

            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            // Register our tabs
            ActionBar.AddNewTab("Main", () =>
            {
                inDebug = false;
                SetContentView(Resource.Layout.ScaleImage);
                _imgMap = FindViewById<CustomImageView>(Resource.Id.imgMap);
                MapMaker.CIVInstance = _imgMap;
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
                    MapMaker.StartPoint = MapMaker.RelativeToAbsolute((int) motionEvent.GetX(), (int) motionEvent.GetY());
                    MapMaker.DrawMap();
                    _imgMap.SetImageBitmap(MapMaker.CurrentImage);
                })
                .SetNegativeButton("End Location", (s, args) =>
                {
                    // User pressed no
                    MapMaker.EndPoint = MapMaker.RelativeToAbsolute((int)motionEvent.GetX(), (int)motionEvent.GetY());
                    MapMaker.DrawMap();
                    _imgMap.SetImageBitmap(MapMaker.CurrentImage);
                })
                .SetNeutralButton("Place user", (s, args) =>
                {
                    MapMaker.UserPosition = MapMaker.RelativeToAbsolute((int)motionEvent.GetX(), (int)motionEvent.GetY());
                    MapMaker.DrawMap();
                    _imgMap.SetImageBitmap(MapMaker.CurrentImage);
                })
                .SetMessage("Start or end location?")
                .SetTitle("Pick some shit")
                .Show();
        }

        private void DrawGridButtonToggle(object sender, EventArgs eventArgs)
        {
            if (_btnDrawGridToggle.Checked)
            {
                MapMaker.DrawGrid = true;
                MapMaker.DrawMap();
                _imgMap.SetImageBitmap(MapMaker.CurrentImage);
            }
            else
            {
                MapMaker.DrawGrid = true;
                MapMaker.DrawMap();
                _imgMap.SetImageBitmap(MapMaker.CurrentImage);
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