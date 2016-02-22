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

		//Location manager for heading information
		CLLocationManager locationManager = null;

		//Instantiate step detector
		StepDetector stepDetector = new StepDetector();
	
		PathView pathView = new PathView ();

		LocationArrowImageView locationArrow;

		UIImageView floorplanImageView;

		UIImage floorplanImageNoGrid;
		UIImage floorplanImageWithGrid;

		private Collision _col = new Collision ();
		private Tuple<float, float> realPos;

		int counter = 0;
        int stepCounter = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			stepDetector.OnStep += StepDetectorOnStep;

			// For accelerometer readings
			var motionManager = new CMMotionManager ();
			motionManager.AccelerometerUpdateInterval = 0.01; // 100Hz

			//To handle long presses and bring up path start/end menu
			var longPressManager = new UILongPressGestureRecognizer ();

			//Graph loading code
			var assembly = Assembly.GetExecutingAssembly ();		
			var asset = assembly.GetManifestResourceStream("Navigator.iOS.Resources.test.xml");
			var g = Graph.Load (asset);

			//Test path
			var start = g.Vertices.First(x=>x=="476-690");
			var end = g.Vertices.First(x=>x=="1066-760");
			var path = g.FindPath(start,end);

			//Collision class
			_col.addGraph(g);
			_col.GiveStartingLocation(787.0f, 717.0f);
			realPos = Tuple.Create<float, float>(787.0f, 717.0f);
			_col.passHeading(90);


			UIView container = new UIView ();
			PathView pathView = new PathView ();
			floorplanImageView = new UIImageView();	

			floorplanImageNoGrid = UIImage.FromBundle ("Images/dcsfloor.png");
			floorplanImageWithGrid = UIImage.FromBundle ("Images/dcsFloorGrid.png");
			locationArrow = new LocationArrowImageView ();
			locationArrow.setLocation (687  + 96, 600 + 88);
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
				
			longPressManager.AllowableMovement = 0;

			longPressManager.AddTarget(() => handleLongPress(longPressManager, g));
		
			floorplanView.AddGestureRecognizer (longPressManager);

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


				for (int i = 0; i < pathLength; i++) {
					int dash = path.ElementAt(i).Source.IndexOf("-");
					float yVal = float.Parse(path.ElementAt(i).Source.Substring(dash+1), CultureInfo.InvariantCulture.NumberFormat) /* + 88 */ ;
					float xVal = float.Parse(path.ElementAt(i).Source.Remove(dash), CultureInfo.InvariantCulture.NumberFormat)/* + 96 */ ;
					pathPoints[i] = new CoreGraphics.CGPoint(xVal, yVal);


				}
				pathView.setPoints(pathPoints);

				locationArrow.lookAtHeading((float)-2);
				//locationArrow.setLocation (650, 850);

				debugLabel.Text = "" + floorplanImageView.Layer.AnchorPoint.X;
			};

        }

		void HandleUpdatedHeading (object sender, CLHeadingUpdatedEventArgs e)
		{
			//double oldRad = -locationManager.Heading.TrueHeading * Math.PI / 180D;
			double newRad = e.NewHeading.TrueHeading * Math.PI / 180D;

			//floorplanImageView.Layer.AnchorPoint = new CGPoint (locationArrow.X/floorplanImageNoGrid.Size.Width, locationArrow.Y/floorplanImageNoGrid.Size.Height);
			locationArrow.lookAtHeading((float)newRad);

		}

		void stepHandler (bool stationaryStart) {
			counter++;
            if (stationaryStart)
            {
                stepCounter += 2;
            }
            stepCounter++;
			debugLabel.Text = "s:" + stepCounter + "c:" + counter;

		}
			
		private void floorplanLookAtHeading(float angle) 
		{
			floorplanImageView.Transform = CGAffineTransform.MakeRotation(angle);

		}

		private void changeFloorPlanImage(UIImageView imageView, UIImage image){
			imageView.Image = image;
			imageView.SizeToFit ();

		}


		private void handleLongPress( UILongPressGestureRecognizer gesture, Graph g ) {

			//debugLabel.Text = "x:" + gesture.LocationInView(floorplanImageView).X + "y:" + gesture.LocationInView(floorplanImageView).Y;

			//Get x and y of press location
			var tapX = gesture.LocationInView(floorplanImageView).X;
			var tapY = gesture.LocationInView (floorplanImageView).Y;

			// Create a new Alert Controller
			UIAlertController actionSheetAlert = UIAlertController.Create("Options", "Set", UIAlertControllerStyle.Alert);

			// Add Actions
			actionSheetAlert.AddAction(UIAlertAction.Create("Start Point",UIAlertActionStyle.Default, (action) => setStartPoint(tapX, tapY)));
			actionSheetAlert.AddAction(UIAlertAction.Create("End Point",UIAlertActionStyle.Default, (action) => setEndPoint(tapX, tapY, g)));
			actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, null));

			//actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, (action) => Console.WriteLine ("Cancel button pressed.")));
			CGPoint point = new CGPoint (gesture.LocationInView (floorplanImageView).X, gesture.LocationInView (floorplanImageView).Y);

			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover!=null) {
				presentationPopover.SourceRect = new CGRect(point, new CGSize(0.1, 0.1));
				//presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}
				
			this.PresentViewController(actionSheetAlert,true,null);

		}

		private void setStartPoint(nfloat x, nfloat y) {
			Tuple<int, int> temp = _col.getNearestNode ((float)x, (float)y);

			//Currently setting the location arrow to nearest node to make it easier to access nearest node value
			locationArrow.setLocation (temp.Item1, temp.Item2);
			string nodeStart = "\"" + locationArrow.X + "-" + locationArrow.Y + "\"";
			debugLabel.Text = nodeStart;

		}

		private void setEndPoint(nfloat X, nfloat Y, Graph g) {
			Tuple<int, int> temp = _col.getNearestNode ((float)X, (float)Y);
			debugLabel.Text = "x:" + temp.Item1 + "y:" + temp.Item2;

			string nodeStart = locationArrow.X + "-" + locationArrow.Y ;
			string nodeEnd = temp.Item1 + "-" + temp.Item2;

			var start = g.Vertices.First (x=>x==nodeStart);
			var end = g.Vertices.First(x=>x==nodeEnd);
			var path = g.FindPath (start, end);

			var pathLength = path.Count();
			debugLabel.Text = "" + pathLength;

			var pathPoints = new CoreGraphics.CGPoint[pathLength];

			for (int i = 0; i < pathLength; i++) {
				int dash = path.ElementAt(i).Source.IndexOf("-");
				float yVal = float.Parse(path.ElementAt(i).Source.Substring(dash+1), CultureInfo.InvariantCulture.NumberFormat);
				float xVal = float.Parse(path.ElementAt(i).Source.Remove(dash), CultureInfo.InvariantCulture.NumberFormat);
				pathPoints[i] = new CoreGraphics.CGPoint(xVal, yVal);

			}

			//This isn't displaying the paths, seems like it should
			pathView.setPoints(pathPoints);

		}

		private void StepDetectorOnStep(bool stationaryStart)
		{
			Tuple<float, float> tempPos = _col.testStepTrigger();
			if (!Tuple.Equals(tempPos, realPos))
			{
				if (stationaryStart)
					stepCounter += 2;

				stepCounter++;
				realPos = tempPos;
			}
		}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}