using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Navigator.Droid.Extensions;
using Navigator.Droid.UIElements;
using Navigator.Pathfinding.Graph;

namespace Navigator.Droid
{
    [Activity(Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        #region < Properties > 

        private Bitmap _currentMapImage;
        private bool _isDrawingGrid;

        #endregion

        #region <UI Elements>

        private ToggleButton _btnDrawGridToggle;
        private CustomImageView _imgMap;

        #endregion


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Small pathfinding test
            var asset = Assets.Open("pbSmall.xml");
            var g = Graph.Load(asset);

            var start = g.Vertices.OrderBy(x => x.DistanceTo(new Vertex() { X = 201, Y = 379 })).First();
            var end = g.Vertices.OrderBy(x => x.DistanceTo(new Vertex() { X = 621, Y = 149 })).First();
            var path = g.FindPath(start,end);
            var path2 = g.FindPath(start,end);
            
            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ScaleImage);

            // Register our tabs
            ActionBar.AddNewTab("Main", () =>
            {
                SetContentView(Resource.Layout.ScaleImage);
                _imgMap = FindViewById<CustomImageView>(Resource.Id.imgMap);
                // Reset to saved state
                if (_currentMapImage != null)
                    _imgMap.SetImageBitmap(_currentMapImage);
                
            });
            ActionBar.AddNewTab("Settings", () =>
            {
                SetContentView(Resource.Layout.ImageSettings);
                _btnDrawGridToggle = FindViewById<ToggleButton>(Resource.Id.drawGridCB);
                _btnDrawGridToggle.Click += DrawGridButtonToggle;

                // Reset to saved state
                if (_isDrawingGrid)
                    _btnDrawGridToggle.Checked = true;
            });
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