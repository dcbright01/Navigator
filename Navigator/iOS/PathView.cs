using System;
using CoreGraphics;
using UIKit;

namespace Navigator.iOS
{
    public class PathView : UIView
    {
        private nfloat _scaleFactor;
        private readonly CGPath path;

        public PathView()
        {
            BackgroundColor = UIColor.Clear;

            path = new CGPath();
        }

        public nfloat ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = value;
                SetNeedsDisplay();
            }
        }

        public CGPoint getLatestPoint()
        {
            return path.CurrentPoint;
        }

        public void setPoints(CGPoint[] points)
        {
            path.AddLines(points);
            SetNeedsDisplay();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);


            //get graphics context
            using (var g = UIGraphics.GetCurrentContext())
            {
                //set up drawing attributes
                g.SetLineWidth(2/_scaleFactor);
                UIColor.Red.SetStroke();

                //use a dashed line
                g.SetLineDash(0, new[] {5, 2/_scaleFactor});

                //add geometry to graphics context and draw it
                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}