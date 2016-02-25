using System;
using CoreGraphics;
using UIKit;

namespace Navigator.iOS
{
    public class LocationArrowImageView : UIImageView
    {
        private nfloat _scaleFactor;

        private readonly UIImage locationArrow =
            UIImage.FromBundle("Images/location-arrow-solid.png").Scale(new CGSize(20, 20));

        public LocationArrowImageView()
        {
            Image = locationArrow;
            SizeToFit();
        }

        public float X { get; private set; }
        public float Y { get; private set; }

        public nfloat ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = value;
                calculateRelPositions();
            }
        }

        public void setLocation(float x, float y)
        {
            X = x;
            Y = y;
            calculateRelPositions();
        }

        public void modLocation(float x, float y)
        {
            X += x;
            Y += y;
            calculateRelPositions();
        }


        public void lookAtHeading(float angle)
        {
            Transform = CGAffineTransform.MakeRotation(angle);
        }


        public void calculateRelPositions()
        {
            Center = new CGPoint(X*_scaleFactor, Y*_scaleFactor);
        }
    }
}