using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Javax.Crypto.Interfaces;
using Navigator.Droid.UIElements;
using Navigator.Pathfinding;
using Navigator.Primitives;

namespace Navigator.Droid.Helpers
{
    /// <summary>
    /// Class that will be responsible for drawing the whole bitmap and stuff :P 
    /// </summary>
    public static class MapMaker
    {
        /// <summary>
        /// The matrix used for all calculations and whatnot
        /// </summary>
        public static Matrix InverseMatrix
        {
            get { return CIVInstance.ImageMatrix; }
        }

        public static CustomImageView CIVInstance;

        #region <StaticImages>
        public static Bitmap PlainMap;
       
        public static Bitmap PlainMapGrid;

        public static Bitmap UserRepresentation;
        #endregion

        public static bool DrawGrid { get; set; } = false;

        public static Bitmap CurrentImage;
        public static Bitmap CurrentUserRepresentation;


        public static Bitmap.Config DefaultConfig = Bitmap.Config.Argb8888;

        public static Paint PaintBrush = new Paint()
        {
            AntiAlias = true,
            Color = Color.Magenta
        };

        public static Vector2 StartPoint;
        public static Vector2 EndPoint;
        public static Vector2 UserPosition;
        public static float UserHeading;
        public static List<UndirEdge> UserPath = new List<UndirEdge>();
        public static Graph PathfindingGraph;

        public static BitmapFactory.Options BitmapOptions = new BitmapFactory.Options()
        {
            InDither = true,
            InScaled = true,
            InPreferredConfig = DefaultConfig,
            InPurgeable = true,
            InMutable = true
        };

        public static void Initialize()
        {
            // Just some checks to see if we have everything
            if(PlainMap == null)
                throw new Exception("No plain map specified");
            if (PlainMapGrid == null)
                throw new Exception("No plain map grid specified");
        }

        private static Bitmap GetPlainMapClone()
        {
            return PlainMap.Copy(DefaultConfig, true);
        }

        private static Bitmap GetPlainMapGridClone()
        {
            return PlainMapGrid.Copy(DefaultConfig, true);
        }

        private static Bitmap GetUserRepresentationClone()
        {
            return UserRepresentation.Copy(DefaultConfig, true);
        }

        /// <summary>
        /// Loads a clean version of the image (copy)
        /// </summary>
        public static void ResetMap()
        {
            // Check if we have a current image 
            if (CurrentImage != null)
            {
                // We have a current image 
                CurrentImage.Recycle(); // Get rid of it
            }
            CurrentImage = DrawGrid ? GetPlainMapGridClone() : GetPlainMapClone();
        }

        public static void DrawMap()
        {
            // Clean up
            ResetMap();
            // Get out drawing 
            var canvas = new Canvas(CurrentImage);

            if(StartPoint != null)
                canvas.DrawCircle(StartPoint.X,StartPoint.Y,20,PaintBrush);

            if (EndPoint != null)
                canvas.DrawCircle(EndPoint.X, EndPoint.Y, 20, PaintBrush);

            if (UserPosition != null)
            {
                // Just some maths to scale the image (we dont want a big arrow at least not for now lol)
                if(CurrentUserRepresentation != null)
                    CurrentUserRepresentation.Recycle();
                var instance = GetUserRepresentationClone();
                CurrentUserRepresentation = Bitmap.CreateScaledBitmap(instance, 20, 20, true);
                instance.Recycle();
                canvas.DrawBitmap(CurrentUserRepresentation, UserPosition.X- CurrentUserRepresentation.Width/2,UserPosition.Y- CurrentUserRepresentation.Height/2,PaintBrush);
            }

            if (StartPoint != null && EndPoint != null)
            {
                // Map points 
                string startPoint = PathfindingGraph.FindClosestNode((int) StartPoint.X, (int) StartPoint.Y);
                string endPoint = PathfindingGraph.FindClosestNode((int) EndPoint.X, (int)EndPoint.Y);
                UserPath = PathfindingGraph.FindPath(startPoint, endPoint);
            }

            if (UserPath.Count > 0)
            {
                PaintBrush.StrokeWidth = 5;
                PaintBrush.Color = Color.Red;
                // If we have some path, draw it
                foreach (var edge in UserPath)
                {
                    Vector2 start = new Vector2(edge.Source);
                    Vector2 target = new Vector2(edge.Target);
                    canvas.DrawLine(start.X,start.Y,target.X,target.Y,PaintBrush);
                }
            }
            canvas.Dispose();
        }

        public static Vector2 RelativeToAbsolute(int x, int y)
        {
            var points = new float[] {x, y};
            var inverse = new Matrix();
            InverseMatrix.Invert(inverse);
            inverse.MapPoints(points);
            return new Vector2(points);
        }
        public static Vector2 RelativeToAbsolute(Vector2 coordinate)
        {
            return RelativeToAbsolute((int) coordinate.X,(int) coordinate.Y);   
        }
    }
}