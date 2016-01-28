using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Navigator.Droid.Extensions
{
    public static class ActionBarExtensions
    {
        public static void AddNewTab(this ActionBar aBar,string tabName, Action viewFunc )
        {
            var tab = aBar.NewTab();
            tab.SetText(tabName);
            tab.TabSelected += (sender, args) =>
            {
                viewFunc();
            };

            aBar.AddTab(tab);
        }
    }
}