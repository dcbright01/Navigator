using System;
using UIKit;
using CoreMotion;
using Foundation;
using CoreLocation;
using CoreGraphics;
using System.Reflection;
using System.Diagnostics;
using Navigator.Pathfinding;
using System.Linq;
using System.Globalization;

namespace Navigator.iOS
{
    public partial class ViewController : UIViewController
    {
        private int toggle = 1;
		CLLocationManager locationManager = null;
		LocationArrowImageView locationArrow;
		UIImageView floorplanImageView;
		StepDetector stepDetector = new StepDetector();

		UIImage floorplanImageNoGrid;
		UIImage floorplanImageWithGrid;

		int counter = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			stepDetector.OnStep += stepHandler;

			var motionManager = new CMMotionManager ();
			motionManager.AccelerometerUpdateInterval = 0.1; // 10Hz

			UIView container = new UIView ();
			floorplanImageView = new UIImageView();
			PathView pathView = new PathView ();

			var assembly = Assembly.GetExecutingAssembly ();		
			var asset = assembly.GetManifestResourceStream("Navigator.iOS.Resources.test.xml");
			Stopwatch sw = new Stopwatch ();
			sw.Start ();
			var g = Graph.Load (asset);
			sw.Stop ();

			long time = (sw.ElapsedMilliseconds);
			var start = g.Vertices.First(x=>x=="589-517");
			var end = g.Vertices.First(x => x == "1079-867");
			var path = g.FindPath(start,end);
			var path2 = g.FindPath(start,end);


			floorplanImageNoGrid = UIImage.FromBundle ("Images/dcsfloor.jpg");
			floorplanImageWithGrid = UIImage.FromBundle ("Images/dcsFloorGrid.jpg");
			locationArrow = new LocationArrowImageView ();
			locationArrow.setLocation (589 + 96, 517 + 88);
			locationArrow.ScaleFactor = floorplanView.ZoomScale;
			pathView.ScaleFactor = floorplanView.ZoomScale;

			floorplanView.ContentSize = floorplanImageNoGrid.Size;
			pathView.Frame = new CoreGraphics.CGRect (new CoreGraphics.CGPoint (0, 0), floorplanImageNoGrid.Size); 
				
			container.AddSubview (floorplanImageView);
			container.AddSubview (locationArrow);
			floorplanImageView.AddSubview (pathView);
			changeFloorPlanImage (floorplanImageView, floorplanImageNoGrid);
			container.SizeToFit ();

			floorplanView.MaximumZoomScale = 1f;
			floorplanView.MinimumZoomScale = .25f;
			floorplanView.AddSubview (container);
			floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImageView; };

			floorplanView.DidZoom += (sender, e) => {
				locationArrow.ScaleFactor = floorplanView.ZoomScale;
				pathView.ScaleFactor = floorplanView.ZoomScale;
			};

			motionManager.StartAccelerometerUpdates (NSOperationQueue.CurrentQueue, (data, error) => {
				stepDetector.passValue(data.Acceleration.X*9.8, data.Acceleration.Y*9.8, data.Acceleration.Z*9.8);
			});

		

			locationManager = new CLLocationManager ();
			locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
			locationManager.HeadingFilter = 1;

			locationManager.UpdatedHeading += HandleUpdatedHeading;
			locationManager.StartUpdatingHeading();

	
            // Perform any additional setup after loading the view, typically from a nib.
            Button.TouchUpInside += delegate
            {
				if (toggle == 1) {
                	var title = string.Format("Set to normal floorplan");
					Button.SetTitle(title, UIControlState.Normal);
					changeFloorPlanImage(floorplanImageView, floorplanImageWithGrid);
					toggle = 0;
				}
				else {
					var title = string.Format("Set to floorplan with grid");
					Button.SetTitle(title, UIControlState.Normal);
					changeFloorPlanImage(floorplanImageView, floorplanImageNoGrid);
					toggle = 1;
				}
            };

			simulationButton.TouchUpInside += delegate {
				var currentX = locationArrow.X;
				var currentY = locationArrow.Y;

				var pathLength = path.Count();
				var pathPoints = new CoreGraphics.CGPoint[pathLength];


				for (int i = 0; i < path.Count(); i++) {
					int dash = path.ElementAt(i).Source.IndexOf("-");
					float yVal = float.Parse(path.ElementAt(i).Source.Substring(dash+1), CultureInfo.InvariantCulture.NumberFormat) + 88;
					float xVal = float.Parse(path.ElementAt(i).Source.Remove(dash), CultureInfo.InvariantCulture.NumberFormat) + 96;
					pathPoints[i] = new CoreGraphics.CGPoint(xVal, yVal);


				}
				pathView.setPoints(pathPoints);

				/*
				pathView.setPoints(new CoreGraphics.CGPoint[]{
					new CoreGraphics.CGPoint(currentX, currentY),
					new CoreGraphics.CGPoint(currentX-50, currentY),
					new CoreGraphics.CGPoint(currentX-50, currentY+50),
					new CoreGraphics.CGPoint(currentX+50, currentY+50)
				});
				*/

				locationArrow.lookAtHeading((float)-2);
				//locationArrow.setLocation (650, 850);

				debugLabel.Text = "" + floorplanImageView.Layer.AnchorPoint.X;
			};

        }

		void HandleUpdatedHeading (object sender, CLHeadingUpdatedEventArgs e)
		{
			//double oldRad = -locationManager.Heading.TrueHeading * Math.PI / 180D;
			double newRad = -e.NewHeading.TrueHeading * Math.PI / 180D;

			//floorplanImageView.Layer.AnchorPoint = new CGPoint (locationArrow.X/floorplanImageNoGrid.Size.Width, locationArrow.Y/floorplanImageNoGrid.Size.Height);
			locationArrow.lookAtHeading((float)newRad);

		}

		void stepHandler (int steps) {
			counter++;
			debugLabel.Text = "s:" + steps + "c:" + counter;

		}
			
		private void floorplanLookAtHeading(float angle) 
		{
			floorplanImageView.Transform = CGAffineTransform.MakeRotation(angle);

		}

		private void changeFloorPlanImage(UIImageView imageView, UIImage image){
			imageView.Image = image;
			imageView.SizeToFit ();

		}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}