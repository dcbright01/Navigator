using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Content.Res;
using Android.Graphics;
using Navigator.Droid.UIElements;
using Navigator.Pathfinding;
using Navigator.Primitives;

namespace Navigator.Droid.Helpers
{
    /// <summary>
    ///     Class that will be responsible for drawing the whole bitmap and stuff :P
    /// </summary>
    public class MapMaker
    {
        public BitmapFactory.Options BitmapOptions = new BitmapFactory.Options
        {
            InDither = true,
            InScaled = true,
            InPreferredConfig = Bitmap.Config.Argb8888,
            InPurgeable = true,
            InMutable = true
        };

        public CustomImageView CIVInstance;

        public Bitmap CurrentImage;
        public Bitmap CurrentUserRepresentation;
        public Vector2 EndPoint;


        public Paint PaintBrush = new Paint
        {
            AntiAlias = true,
            Color = Color.Magenta
        };

        public Graph PathfindingGraph;

        public Vector2 StartPoint;
        public float UserHeading;
        public List<UndirEdge> UserPath = new List<UndirEdge>();
        public Vector2 UserPosition;

        /// <summary>
        ///     The matrix used for all calculations and whatnot
        /// </summary>
        public Matrix InverseMatrix
        {
            get { return CIVInstance.ImageMatrix; }
        }

        public bool DrawGrid { get; set; } = false;

        public void Initialize(Resources res)
        {
            // Decode resources for pathfinding test
            if (PlainMap == null)
                PlainMap = BitmapFactory.DecodeResource(res, Resource.Drawable.dcsFloor);
            if (PlainMapGrid == null)
                PlainMapGrid = BitmapFactory.DecodeResource(res, Resource.Drawable.dcsFloorGrid);
            if (UserRepresentation == null)
                UserRepresentation = BitmapFactory.DecodeResource(res, Resource.Drawable.arrow);
            if(CurrentUserRepresentation == null)
                CurrentUserRepresentation = Bitmap.CreateScaledBitmap(UserRepresentation, 20, 20, true);

            // Just some checks to see if we have everything
            if (PlainMap == null)
                throw new Exception("No plain map specified");
            if (PlainMapGrid == null)
                throw new Exception("No plain map grid specified");
        }

        private Bitmap GetPlainMapClone()
        {
            return PlainMap.Copy(Bitmap.Config.Argb8888, true);
        }

        private Bitmap GetPlainMapGridClone()
        {
            return PlainMapGrid.Copy(Bitmap.Config.Argb8888, true);
        }

        private Bitmap GetUserRepresentationClone()
        {
            return UserRepresentation.Copy(Bitmap.Config.Argb8888, true);
        }

        /// <summary>
        ///     Loads a clean version of the image (copy)
        /// </summary>
        public void ResetMap()
        {
            // Check if we have a current image 
            if (CurrentImage != null)
            {
                // We have a current image 
                CurrentImage.Recycle(); // Get rid of it
            }
            CurrentImage = DrawGrid ? GetPlainMapGridClone() : GetPlainMapClone();
        }

        public void DrawMap()
        {
            // Clean up
            ResetMap();
            // Get out drawing 
            var canvas = new Canvas(CurrentImage);

            if (StartPoint != null)
                canvas.DrawCircle(StartPoint.X, StartPoint.Y, 20, PaintBrush);

            if (EndPoint != null)
                canvas.DrawCircle(EndPoint.X, EndPoint.Y, 20, PaintBrush);

            if (UserPosition != null)
            {
                // Just some maths to scale the image (we dont want a big arrow at least not for now lol)
                canvas.DrawBitmap(CurrentUserRepresentation, UserPosition.X - CurrentUserRepresentation.Width/2,
                    UserPosition.Y - CurrentUserRepresentation.Height/2, PaintBrush);
            }

            if (StartPoint != null && EndPoint != null)
            {
                // Map points 
                var startPoint = PathfindingGraph.FindClosestNode((int) StartPoint.X, (int) StartPoint.Y);
                var endPoint = PathfindingGraph.FindClosestNode((int) EndPoint.X, (int) EndPoint.Y);
                UserPath = PathfindingGraph.FindPath(startPoint, endPoint);
            }

            if (UserPath.Count > 0)
            {
                PaintBrush.StrokeWidth = 5;
                PaintBrush.Color = Color.Red;
                // If we have some path, draw it
                foreach (var edge in UserPath)
                {
                    var start = new Vector2(edge.Source);
                    var target = new Vector2(edge.Target);
                    canvas.DrawLine(start.X, start.Y, target.X, target.Y, PaintBrush);
                }
            }
            canvas.Dispose();
            CIVInstance.SetImageBitmap(CurrentImage);
        }

        public Vector2 RelativeToAbsolute(int x, int y)
        {
            var points = new float[] {x, y};
            var inverse = new Matrix();
            InverseMatrix.Invert(inverse);
            inverse.MapPoints(points);
            return new Vector2(points);
        }

        public Vector2 RelativeToAbsolute(Vector2 coordinate)
        {
            return RelativeToAbsolute((int) coordinate.X, (int) coordinate.Y);
        }

        #region <Images>

        public Bitmap PlainMap;

        public Bitmap PlainMapGrid;

        public Bitmap UserRepresentation;

        #endregion
    }
}