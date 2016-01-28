using System;
using Android.App;
using Android.Widget;
using Android.OS;
using com.refractored.monodroidtoolkit;
using Navigator.Droid.Extensions;

namespace Navigator.Droid
{
	[Activity (Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
    {
        #region <UI Elements>
	    private ToggleButton btnDrawGridToggle;
	    private ScaleImageView imgMap;
        #endregion

        protected override void OnCreate (Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);
			
            // Set nav mode
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ScaleImage);
               
            // Register our tabs
            ActionBar.AddNewTab("Main",()=>{SetContentView(Resource.Layout.ScaleImage);});
		    ActionBar.AddNewTab("Settings", () => { SetContentView(Resource.Layout.ImageSettings); });
		
            // Get our UI elements
            btnDrawGridToggle = FindViewById<ToggleButton>(Resource.Id.drawGridCB);

            // Assign events to our elements
            //btnDrawGridToggle.Click += drawGridButtonToggle;
		}

	    private void drawGridButtonToggle(object sender, EventArgs eventArgs)
	    {
	        if (btnDrawGridToggle.Checked)
	        {

	        }
	        else
	        {
	            
	        }
	    }
	}
}


