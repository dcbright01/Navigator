using System;
using Android.App;

namespace Navigator.Droid.Extensions
{
    public static class ActionBarExtensions
    {
        public static void AddNewTab(this ActionBar aBar, string tabName, Action viewFunc)
        {
            var tab = aBar.NewTab();
            tab.SetText(tabName);
            tab.TabSelected += (sender, args) => { viewFunc(); };
            aBar.AddTab(tab);
        }
    }
}