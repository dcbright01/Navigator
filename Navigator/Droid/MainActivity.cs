using Android.App;
using Android.Widget;
using Android.OS;

namespace Navigator.Droid
{
	[Activity (Label = "Navigator", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ScaleImage);

            ActionBar.Tab tab = ActionBar.NewTab();
            tab.SetText("Main");
		    tab.TabSelected += (sender, args) =>
		    {
                SetContentView(Resource.Layout.ScaleImage);
		    };
            ActionBar.AddTab(tab);

            tab = ActionBar.NewTab();
            tab.SetText("Settings");
		    tab.TabSelected += (sender, args) =>
		    {
                SetContentView(Resource.Layout.ImageSettings);
		    };
            ActionBar.AddTab(tab);

		}
	}
}


