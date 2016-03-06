using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Navigator.Droid.Extensions
{
    public static class BitmapExtensions
    {
        public static int[,] ToColorArray(this Bitmap bmap)
        {
            int[,] result = new int[bmap.Width,bmap.Height];
            for (int x = 0; x < bmap.Width;x++)
            {
                for (int y = 0; y < bmap.Height;y++)
                {
                    result[x, y] = bmap.GetPixel(x, y);
                }
            }
            return result;
        }
    }
}