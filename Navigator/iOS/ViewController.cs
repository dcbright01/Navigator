using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CoreGraphics;
using CoreLocation;
using CoreMotion;
using Foundation;
using Navigator.Pathfinding;
using UIKit;
using CoreAnimation;
using Navigator.Helpers;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Navigator.iOS
{
    public partial class ViewController : UIViewController
    {
        //Instantiate step detector and collision class
        private ICollision collisionHandler;

		//Will contain graph data
        private Graph floorPlanGraph;

		//Arrow to display users location
		private LocationArrowImageView locationArrow;

		//Will de used to display the floorplans
		private UIImageView floorplanImageView;
        private UIImage floorplanImageNoGrid;
        private UIImage floorplanImageWithGrid;
        private UIImage wallCollImg;

		//Keeps track of steps taken
        private int GlobalStepCounter = 0;

		//Will hold the paths users should follow
		private readonly PathView pathView = new PathView();

        //Location manager for heading information
        private CLLocationManager locationManager;

		//Toggle for button press
        private int toggle = 1;

        private int count = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // For accelerometer readings
            var motionManager = new CMMotionManager();
            motionManager.AccelerometerUpdateInterval = 0.01; // 100Hz

            //To handle long presses and bring up path start/end menu
            var longPressManager = new UILongPressGestureRecognizer();

            //Graph loading code
            var assembly = Assembly.GetExecutingAssembly();
            var asset = assembly.GetManifestResourceStream("Navigator.iOS.Resources.dcsfloorWideDoors.xml");
            floorPlanGraph = Graph.Load(asset);


            collisionHandler = new Collision(floorPlanGraph, new StepDetector());


            collisionHandler.SetLocation(707.0f, 677.0f);
            collisionHandler.PassHeading(0);
            collisionHandler.PositionChanged += HandleStepsTaken;

			//Container for floorplan and any overlaid images
            var container = new UIView();

			//Will contain floorplan images
            floorplanImageView = new UIImageView();

			//Load floorplan images
			floorplanImageNoGrid = UIImage.FromBundle("Images/dcsfloorWideDoors.png");
            floorplanImageWithGrid = UIImage.FromBundle("Images/dcsfloorWideDoorsGrid.png");

			//Initiate the location arrow
            locationArrow = new LocationArrowImageView();
            locationArrow.setLocation(707.0f, 677.0f);
            locationArrow.ScaleFactor = floorplanView.ZoomScale;
            pathView.ScaleFactor = floorplanView.ZoomScale;

			//Set sizes for floorplan view and path view
            floorplanView.ContentSize = floorplanImageNoGrid.Size;
            pathView.Frame = new CGRect(new CGPoint(0, 0), floorplanImageNoGrid.Size);

			//Add subviews to the container (including pathview and floorplanview)
            container.AddSubview(floorplanImageView);
            container.AddSubview(locationArrow);
            floorplanImageView.AddSubview(pathView);
            changeFloorPlanImage(floorplanImageView, floorplanImageNoGrid);
            container.SizeToFit();

			//Adjust scrolling and zooming properties for the floorplanView
            floorplanView.MaximumZoomScale = 1f;
            floorplanView.MinimumZoomScale = .25f;
            floorplanView.AddSubview(container);
            floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImageView; };

			//Variables needed to convert device acceleration to world z direction acceleration
			double accelX = 0, accelY = 0, accelZ = 0;

			//Scale location arrow and paths when zooming the floorplan
            floorplanView.DidZoom += (sender, e) =>
            {
                locationArrow.ScaleFactor = floorplanView.ZoomScale;
                pathView.ScaleFactor = floorplanView.ZoomScale;
            };

			//Pass acceleremoter values to the collision class
            motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue,
                (data, error) =>
                {
					accelX = data.Acceleration.X*9.8;
					accelY = data.Acceleration.Y*9.8;
					accelZ = Math.Sqrt(Math.Pow(accelX, 2) + Math.Pow(accelY, 2) + Math.Pow(data.Acceleration.Z*9.8, 2));

                    collisionHandler.PassSensorReadings(CollisionSensorType.Accelometer, accelX,
                        accelY, accelZ);
                    displayAccelVal((float)accelZ);
                });

			/*
			motionManager.StartDeviceMotionUpdates(NSOperationQueue.CurrentQueue, (data, error) =>
				{ 
					//data.Attitude.MultiplyByInverseOfAttitude(data.Attitude);
					var test = data.UserAcceleration.X;
					var accelRelZ = data.Attitude.RotationMatrix.m31 * accelX + data.Attitude.RotationMatrix.m32 * accelY + data.Attitude.RotationMatrix.m33 * accelZ;
					debugLabel.Text = "" + Math.Round(test, 2);//Math.Round(accelRelZ, 2);
				}
			);
			*/

			//LongPressManager will cause the path input menu to appear after a stationary long press
            longPressManager.AllowableMovement = 0;
            longPressManager.AddTarget(() => handleLongPress(longPressManager, floorPlanGraph));
            floorplanView.AddGestureRecognizer(longPressManager);

			//the location manager handles the phone heading
            locationManager = new CLLocationManager();
            locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            locationManager.HeadingFilter = 1;
            locationManager.UpdatedHeading += HandleUpdatedHeading;
            locationManager.StartUpdatingHeading();

			//Button currently used for testing purposes only
            Button.TouchUpInside += delegate
            {
                if (toggle == 1)
                {
                    var title = "Set to normal floorplan";
                    Button.SetTitle(title, UIControlState.Normal);
                    changeFloorPlanImage(floorplanImageView, floorplanImageWithGrid);
                    toggle = 0;
                }
                else
                {
                    var title = "Set to floorplan with grid";
                    Button.SetTitle(title, UIControlState.Normal);
                    changeFloorPlanImage(floorplanImageView, floorplanImageNoGrid);
                    toggle = 1;
                }
            };

			//Another testing button
            simulationButton.TouchUpInside += delegate { collisionHandler.StepTaken(false); };
        }
			

        private void HandleUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            var newRad = (float) (e.NewHeading.TrueHeading*Math.PI/180f);
            collisionHandler.PassHeading(newRad);

            //floorplanImageView.Layer.AnchorPoint = new CGPoint (locationArrow.X/floorplanImageNoGrid.Size.Width, locationArrow.Y/floorplanImageNoGrid.Size.Height);
            locationArrow.lookAtHeading(newRad);
        }

        private void HandleStepsTaken(object s, PositionChangedHandlerEventArgs args)
        {
            GlobalStepCounter++;
            locationArrow.setLocation(args.newX, args.newY);
            debugLabel.Text = "" + GlobalStepCounter;
        }


        private void floorplanLookAtHeading(float angle)
        {
            floorplanImageView.Transform = CGAffineTransform.MakeRotation(angle);
        }

        private void changeFloorPlanImage(UIImageView imageView, UIImage image)
        {
            imageView.Image = image;
            imageView.SizeToFit();
        }

        private void drawPathFromUser(float endX, float endY)
        {
			//Get nearest node to user location
            var userNode = floorPlanGraph.FindClosestNode(locationArrow.X, locationArrow.Y);

			//Get x and y of this nearest node
            var pathStart = floorPlanGraph.Vertices.First(x => x == userNode.ToPointString());

			//Get nearest node to end location
            var destinationNode = floorPlanGraph.FindClosestNode(endX, endY);

			//Get x and y of this node
            var pathEnd = floorPlanGraph.Vertices.First(x => x == destinationNode.ToPointString());

			//Calculate path
            var path = floorPlanGraph.FindPath(pathStart, pathEnd);

			//Get path length
            var pathLength = path.Count();

			//Extract node along path
            var pathPoints = new CGPoint[pathLength];

			//Iterate over all nodes and create a list of CGpoints
            for (var i = 0; i < pathLength; i++)
            {
                var dash = path.ElementAt(i).Source.IndexOf("-");
                var yVal = float.Parse(path.ElementAt(i).Source.Substring(dash + 1),
                    CultureInfo.InvariantCulture.NumberFormat);
                var xVal = float.Parse(path.ElementAt(i).Source.Remove(dash), CultureInfo.InvariantCulture.NumberFormat);
                pathPoints[i] = new CGPoint(xVal, yVal);
            }

			//Draw path on screen
            pathView.setPoints(pathPoints);
        }


        private void handleLongPress(UILongPressGestureRecognizer gesture, Graph g)
        {
            //Get x and y of press location
            var tapX = gesture.LocationInView(floorplanImageView).X;
            var tapY = gesture.LocationInView(floorplanImageView).Y;

			// Create a new Alert Controller
			UIAlertController actionSheetAlert = UIAlertController.Create("Options", null, UIAlertControllerStyle.Alert);

			// Add Actions
			actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, null));
			actionSheetAlert.AddAction(UIAlertAction.Create("Set Start Point",UIAlertActionStyle.Default, (action) => setStartPoint(tapX, tapY)));
			actionSheetAlert.AddAction(UIAlertAction.Create("Set End Point",UIAlertActionStyle.Default, (action) => setEndPoint(tapX, tapY)));

			// Display alert
			this.PresentViewController(actionSheetAlert,true,null);

			/*
			CGPoint point = new CGPoint (gesture.LocationInView (floorplanImageView).X, gesture.LocationInView (floorplanImageView).Y);

			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover!=null) {
				presentationPopover.SourceRect = new CGRect(point, new CGSize(0.1, 0.1));
				//presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}
			*/      
		}

        public void displayAccelVal(float a) {
            count++;
            debugLabel.Text = "" + count;
            if (count > 400) {
                breakpointCheck (a);
            }
        }
        private void breakpointCheck (float a){
            debugLabel.Text = "" + count;

        }

		public void setStartPoint(nfloat x, nfloat y) {
			locationArrow.setLocation ((float)x, (float)y);
			collisionHandler.SetLocation ((float)x, (float)y);
		}

		public void setEndPoint(nfloat x, nfloat y) {
			drawPathFromUser((float) x, (float) y);
		}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}