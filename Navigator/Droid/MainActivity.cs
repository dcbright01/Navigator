﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.App;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using Navigator.Droid.Extensions;
using Navigator.Droid.Helpers;
using Navigator.Droid.Sensors;
using Navigator.Droid.UIElements;
using Navigator.Helpers;
using Navigator.Pathfinding;
using Navigator.Primitives;

namespace Navigator.Droid
{
    [Activity(Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        private Collision _collision;
        private MapMaker _mapMaker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            SetContentView(Resource.Layout.ScaleImage);

            // Register our sensor listener
            _sensorManager = (SensorManager) GetSystemService(SensorService);
            _sensorListener = new CustomListener(_sensorManager);
            _sensorListener.AccelerationProcessor.OnValueChanged += AccelerationProcessorOnValueChanged;
            _sensorListener.RotationProcessor.OnValueChanged += RotationProcessorOnValueChanged;

            // Class that will handle drawing of the map
            _mapMaker = new MapMaker();
            _mapMaker.Initialize(Resources);

            var graphAsset = Assets.Open("dcsGroundFloor.xml");
            var graphInstance = Graph.Load(graphAsset);

            _mapMaker.PathfindingGraph = graphInstance;
            var sw = new Stopwatch();
            sw.Start();
            graphInstance.FindClosestNode(686, 620,5);
            sw.Stop();
            long time = sw.ElapsedMilliseconds;

            _collision = new Collision(graphInstance, new StepDetector());
            _collision.SetLocation(707.0f, 677.0f);
            _collision.PassHeading(90);
            _collision.StepDetector.OnStep += StepDetectorOnStep;

            setUpUITabs();
        }

        private void StepDetectorOnStep(bool startFromStat)
        {
            RunOnUiThread(() =>
            {
                FindViewById<TextView>(Resource.Id.stepCounter).Text =
                    ((StepDetector) (_collision.StepDetector)).StepCounter.ToString();
            });
        }

        // Prepares the spinner
        private void setUpRoomsSpinner(List<Room> rooms)
        {
            var roomNames = rooms.ConvertAll(r => r.Name);

            var spinner = FindViewById<Spinner>(Resource.Id.roomSpinner);
            spinner.ItemSelected += spinnerItemSelected;
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, roomNames);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinner.Adapter = adapter;
        }

        private void spinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner) sender;
            string toast = string.Format ("The room is {0}", spinner.GetItemAtPosition (e.Position));
            Toast.MakeText (this, toast, ToastLength.Long).Show();
        }

        private void setUpUITabs()
        {
            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            // Register our tabs
            ActionBar.AddNewTab("Main", () =>
            {
                inDebug = false;
                SetContentView(Resource.Layout.ScaleImage);
                _imgMap = FindViewById<CustomImageView>(Resource.Id.imgMap);
                _mapMaker.CIVInstance = _imgMap;
                _imgMap.LongPress += ImgMapOnLongPress;
                // Set up spinner 
                setUpRoomsSpinner(_mapMaker.PathfindingGraph.Rooms);
                // Reset to saved state
                _mapMaker.DrawMap();
            });
            ActionBar.AddNewTab("Settings", () =>
            {
                inDebug = false;
                SetContentView(Resource.Layout.ImageSettings);
                _btnDrawGridToggle = FindViewById<ToggleButton>(Resource.Id.drawGridCB);
                _btnDrawGridToggle.Click += DrawGridButtonToggle;

                // Reset to saved state
                if (_mapMaker.DrawGrid)
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
            var t = ActionBar.SelectedTab.Text;
        }

        private void RotationProcessorOnValueChanged(double value)
        {
            _collision.PassHeading((float) value);
            if (inDebug)
            {
                RunOnUiThread(
                    () => { _azimuthText.Text = string.Format("Azimuth: {0}", VarunMaths.RadianToDegree(value)); });
            }
        }

        private void AccelerationProcessorOnValueChanged(Vector3 value)
        {
            // Pass our values
            _collision.PassSensorReadings(CollisionSensorType.Accelometer, value.X,value.Y,value.Z);
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
                    _mapMaker.StartPoint = _mapMaker.RelativeToAbsolute((int) motionEvent.GetX(),
                        (int) motionEvent.GetY());
                    _mapMaker.DrawMap();
                })
                .SetNegativeButton("End Location", (s, args) =>
                {
                    // User pressed no
                    _mapMaker.EndPoint = _mapMaker.RelativeToAbsolute((int) motionEvent.GetX(), (int) motionEvent.GetY());
                    _mapMaker.DrawMap();
                })
                .SetNeutralButton("Place user", (s, args) =>
                {
                    _mapMaker.UserPosition = _mapMaker.RelativeToAbsolute((int) motionEvent.GetX(),
                        (int) motionEvent.GetY());
                    _mapMaker.DrawMap();
                })
                .SetMessage("Start or end location?")
                .SetTitle("Pick some shit")
                .Show();
        }

        private void DrawGridButtonToggle(object sender, EventArgs eventArgs)
        {
            if (_btnDrawGridToggle.Checked)
            {
                _mapMaker.DrawGrid = true;
                _mapMaker.DrawMap();
            }
            else
            {
                _mapMaker.DrawGrid = true;
                _mapMaker.DrawMap();
            }
        }

        #region <ISensorEventListener>

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            _sensorListener.OnSensorChanged(e);
        }

        #endregion

        #region <UI Elements>

        private ToggleButton _btnDrawGridToggle;
        private CustomImageView _imgMap;

        private string[] roomNames;

        #endregion

        #region <Sensors>

        private SensorManager _sensorManager;
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
    }
}