using System;
using CoreGraphics;
using UIKit;
using Navigator.Helpers;

namespace Navigator.iOS
{
    public class PathView : UIView
    {
        private nfloat _scaleFactor;
        private readonly CGPath path;
        private CGPoint[] pointsList;
        private bool pathSet = false;
        private WallCollision wallCol;

        public PathView(WallCollision wc)
        {
            BackgroundColor = UIColor.Clear;

            path = new CGPath();
            wallCol = wc;

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
            pathSet = true;
			path.AddLines(points);
            pointsList = points;
            SetNeedsDisplay();
        }

        /*
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);



            //get graphics context
            using (var g = UIGraphics.GetCurrentContext())
            {
                //set up drawing attributes
                g.SetLineWidth(3/_scaleFactor);
                UIColor.Cyan.SetStroke ();
                g.SetShadow (new CGSize (1, 1), 10, UIColor.Blue.CGColor);
                //use a dashed line
                //g.SetLineDash(0, new[] {5, 2/_scaleFactor});

                //add geometry to graphics context and draw it
                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
        */

        public override void Draw (CGRect rect)
        {
            if (pathSet == true) {
                base.Draw (rect);

                using (var context = UIGraphics.GetCurrentContext ()) {
                

                    //set up drawing attributes
                    context.SetLineWidth (3 / _scaleFactor);
                    UIColor.Cyan.SetStroke ();
                    context.SetShadow (new CGSize (1, 1), 10, UIColor.Blue.CGColor);

                    CGPoint start = pointsList [0];
                    int inter = 0;
                    CGPoint end = pointsList[0];
                    CGPoint[] line = new CGPoint[2];
                    int seg = 4;

                    int iter = (int)Math.Floor((float)(pointsList.Length/seg));
                    int iter2 = pointsList.Length;
                    for (int i = 0; i < iter2 - 1; i++) {
                        if (wallCol.IsValidStep ((int)start.X, (int)start.Y, (int)pointsList [i].X, ((int)pointsList [i].Y))) {
                            inter = i;
                            end = pointsList [i];
                        } else {
                            line [0] = start;
                            line [1] = pointsList[i-1];
                            context.AddLines (line);
                            context.StrokePath ();

                            inter = i;
                            start = pointsList[i-1];

                            /*
                            context.MoveTo (pointsList [i - 2].X, pointsList [i - 2].Y);
                            context.AddQuadCurveToPoint (pointsList[i-1].X, pointsList[i-1].Y, start.X, start.Y);
                            context.StrokePath ();
                            */

                        }
                    }
                    line [0] = start;
                    line [1] = end;
                    context.AddLines (line);
                    context.StrokePath ();


                    //context.AddPath(path);
                    //context.DrawPath(CGPathDrawingMode.Stroke);


                    /*
                    // Draw a bezier curve with end points s,e and control points cp1,cp2
                    var s = new CGPoint (30, 120);
                    var e = new CGPoint (300, 120);
                    var cp1 = new CGPoint (120, 30);
                    var cp2 = new CGPoint (210, 210);
                    */



                    /*
                    int seg = 5;
                    int a1 = 2;
                    int a2 = 3;
                    int a3 = 5;

                    var start = pointsList [0];

                    int iter = (int)Math.Floor((float)(pointsList.Length/seg));
                    for (int i = 0; i < iter-1; i++) {



                        context.MoveTo (pointsList [i*seg].X, pointsList [i*seg].Y);
                        context.AddCurveToPoint (pointsList [(i*seg)+a1].X, pointsList [(i*seg)+a1].Y, pointsList [(i*seg)+a2].X, pointsList [(i*seg)+a2].Y, pointsList [(i*seg)+a3].X, pointsList [(i*seg)+a3].Y);
                        context.StrokePath ();
                    }
                    */


                    /*
                    // Draw a quad curve with end points s,e and control point cp1
                    context.SetStrokeColor (1, 1, 1, 1);
                    s = new CGPoint (30, 300);
                    e = new CGPoint (270, 300);
                    cp1 = new CGPoint (150, 180);
                    context.MoveTo (s.X, s.Y);
                    context.AddQuadCurveToPoint (cp1.X, cp1.Y, e.X, e.Y);
                    context.AddQuadCurveToPoint (cp1.X + 100, cp1.Y + 100, e.X + 100, e.Y + 100);

                    context.StrokePath ();
                    */

                }
            }
        }
    }
}