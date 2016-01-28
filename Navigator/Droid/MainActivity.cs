using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using com.refractored.monodroidtoolkit;
using Navigator.Droid.Extensions;

namespace Navigator.Droid
{
    [Activity(Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private Bitmap currentMapImage;
        private bool isDrawingGrid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ScaleImage);

            // Register our tabs
            ActionBar.AddNewTab("Main", () =>
            {
                SetContentView(Resource.Layout.ScaleImage);
                imgMap = FindViewById<ScaleImageView>(Resource.Id.imgMap);
                if (currentMapImage != null)
                {
                    imgMap.SetImageBitmap(currentMapImage);
                }
            });
            ActionBar.AddNewTab("Settings", () =>
            {
                SetContentView(Resource.Layout.ImageSettings);
                btnDrawGridToggle = FindViewById<ToggleButton>(Resource.Id.drawGridCB);
                btnDrawGridToggle.Click += drawGridButtonToggle;
                if (isDrawingGrid)
                    btnDrawGridToggle.Checked = true;
            });
        }

        private void drawGridButtonToggle(object sender, EventArgs eventArgs)
        {
            if (btnDrawGridToggle.Checked)
            {
                isDrawingGrid = true;
                currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsFloorGrid);
            }
            else
            {
                isDrawingGrid = false;
                currentMapImage = BitmapFactory.DecodeResource(Resources, Resource.Drawable.dcsfloor);
            }
        }

        #region <UI Elements>

        private ToggleButton btnDrawGridToggle;
        private ScaleImageView imgMap;

        #endregion

        #region < Properties > 

        #endregion
    }
}