﻿using System;
using UIKit;
using CoreGraphics;
using CoreLocation;

namespace Navigator.iOS
{
	public class LocationArrowImageView : UIImageView 
	{
		public double X {get;private set;}
		public double Y {get;private set;}

		private double _heading;

		private nfloat _scaleFactor;
		public nfloat ScaleFactor {
			get { return _scaleFactor; }
			set {
				_scaleFactor = value;
				calculateRelPositions ();
			}
		}

		private UIImage locationArrow = 
			UIImage.FromBundle ("Images/location-arrow-solid.png").Scale(new CoreGraphics.CGSize(20,20));

		public LocationArrowImageView(){
			this.Image = locationArrow;
			this.SizeToFit();
		}

		public void setLocation(double x, double y){
			X = x;
			Y = y;
			calculateRelPositions ();
		}

		public void modLocation(double x, double y){
			X += x;
			Y += y;
			calculateRelPositions ();
		}
			
		public void moveForwards(double distance){

		}

		public void lookAtHeading(float angle) {
			this.Transform = CGAffineTransform.MakeRotation(angle);
		}

			
		public void calculateRelPositions(){
			this.Center = new CoreGraphics.CGPoint (X * _scaleFactor, Y * _scaleFactor);
		}
	}
}