using System;
using System.Drawing;
using CoreGraphics;
using CoreAnimation;
using UIKit;
using System.Collections.Generic;

namespace Navigator.iOS
{
	public class PathView : UIView
	{
		CGPath path;

		private nfloat _scaleFactor;
		public nfloat ScaleFactor {
			get { return _scaleFactor; }
			set {
				_scaleFactor = value;
				SetNeedsDisplay ();
			}
		} 

		public PathView ()
		{
			BackgroundColor = UIColor.Clear;

			path = new CGPath ();
		}

		public CGPoint getLatestPoint(){
			return path.CurrentPoint;
		}

		public void setPoints(CGPoint[] points){
			path.AddLines (points);
			SetNeedsDisplay ();
		}

		public override void Draw (CGRect rect)
		{
			base.Draw (rect);


			//get graphics context
			using(CGContext g = UIGraphics.GetCurrentContext ()){

				//set up drawing attributes
				g.SetLineWidth (2/_scaleFactor);
				UIColor.Red.SetStroke ();

				//use a dashed line
				g.SetLineDash (0, new nfloat[]{5, 2/_scaleFactor});

				//add geometry to graphics context and draw it
				g.AddPath (path);
				g.DrawPath (CGPathDrawingMode.Stroke);

			}

		}	       
	}
}

