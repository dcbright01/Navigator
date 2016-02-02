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
                SetContentView(Resource.Layout.ScaleImage);
                _imgMap = FindViewById<CustomImageView>(Resource.Id.imgMap);
                _imgMap.LongPress += ImgMapOnLongPress;
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