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

namespace Navigator.iOS
{
    public partial class ViewController : UIViewController
    {
        //Instantiate step detector and collision class
        private ICollision _col;

        private Graph floorPlanGraph;

        private UIImage floorplanImageNoGrid;

        private UIImageView floorplanImageView;
        private UIImage floorplanImageWithGrid;
        private readonly int GlobalStepCounter = 0;


        private LocationArrowImageView locationArrow;

        //Location manager for heading information
        private CLLocationManager locationManager;
        private readonly PathView pathView = new PathView();
        private int toggle = 1;


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
            var asset = assembly.GetManifestResourceStream("Navigator.iOS.Resources.test.xml");
            floorPlanGraph = Graph.Load(asset);

            //Test path


            //Collision class
            _col = new Collision(floorPlanGraph);
            _col.SetLocation(707.0f, 677.0f);
            _col.PassHeading(90);
            _col.PositionChanged += HandleStepsTaken;


            var container = new UIView();
            floorplanImageView = new UIImageView();

            floorplanImageNoGrid = UIImage.FromBundle("Images/dcsfloor.png");
            floorplanImageWithGrid = UIImage.FromBundle("Images/dcsFloorGrid.png");
            locationArrow = new LocationArrowImageView();
            locationArrow.setLocation(707.0f, 677.0f);
            locationArrow.ScaleFactor = floorplanView.ZoomScale;
            pathView.ScaleFactor = floorplanView.ZoomScale;

            floorplanView.ContentSize = floorplanImageNoGrid.Size;
            pathView.Frame = new CGRect(new CGPoint(0, 0), floorplanImageNoGrid.Size);


            container.AddSubview(floorplanImageView);
            container.AddSubview(locationArrow);
            floorplanImageView.AddSubview(pathView);
            changeFloorPlanImage(floorplanImageView, floorplanImageNoGrid);
            container.SizeToFit();

            floorplanView.MaximumZoomScale = 1f;
            floorplanView.MinimumZoomScale = .25f;
            floorplanView.AddSubview(container);
            floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImageView; };

            floorplanView.DidZoom += (sender, e) =>
            {
                locationArrow.ScaleFactor = floorplanView.ZoomScale;
                pathView.ScaleFactor = floorplanView.ZoomScale;
            };

            motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue,
                (data, error) =>
                {
                    _col.PassSensorReadings(CollisionSensorType.Accelometer, data.Acceleration.X*9.8,
                        data.Acceleration.Y*9.8, data.Acceleration.Z*9.8);
                });

            longPressManager.AllowableMovement = 0;
            longPressManager.AddTarget(() => handleLongPress(longPressManager, floorPlanGraph));
            floorplanView.AddGestureRecognizer(longPressManager);

            locationManager = new CLLocationManager();
            locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            locationManager.HeadingFilter = 1;
            locationManager.UpdatedHeading += HandleUpdatedHeading;
            locationManager.StartUpdatingHeading();

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

            simulationButton.TouchUpInside += delegate { _col.StepTaken(false); };
        }

        private void HandleUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            var newRad = (float) (e.NewHeading.TrueHeading*Math.PI/180f);
            _col.PassHeading(newRad);


            //floorplanImageView.Layer.AnchorPoint = new CGPoint (locationArrow.X/floorplanImageNoGrid.Size.Width, locationArrow.Y/floorplanImageNoGrid.Size.Height);
            locationArrow.lookAtHeading(newRad);
        }

        private void HandleStepsTaken(object s, PositionChangedHandlerEventArgs args)
        {
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
            var userNode = floorPlanGraph.FindClosestNode(locationArrow.X, locationArrow.Y, 6);
            var pathStart = floorPlanGraph.Vertices.First(x => x == userNode.ToPointString());
            var destinationNode = floorPlanGraph.FindClosestNode(endX, endY, 6);
            var pathEnd = floorPlanGraph.Vertices.First(x => x == destinationNode.ToPointString());
            var path = floorPlanGraph.FindPath(pathStart, pathEnd);

            var pathLength = path.Count();
            var pathPoints = new CGPoint[pathLength];


            for (var i = 0; i < pathLength; i++)
            {
                var dash = path.ElementAt(i).Source.IndexOf("-");
                var yVal = float.Parse(path.ElementAt(i).Source.Substring(dash + 1),
                    CultureInfo.InvariantCulture.NumberFormat) /* + 88 */;
                var xVal = float.Parse(path.ElementAt(i).Source.Remove(dash), CultureInfo.InvariantCulture.NumberFormat)
                    /* + 96 */;
                pathPoints[i] = new CGPoint(xVal, yVal);
            }
            pathView.setPoints(pathPoints);
        }


        private void handleLongPress(UILongPressGestureRecognizer gesture, Graph g)
        {
            //debugLabel.Text = "x:" + gesture.LocationInView(floorplanImageView).X + "y:" + gesture.LocationInView(floorplanImageView).Y;

            //Get x and y of press location
            var tapX = gesture.LocationInView(floorplanImageView).X;
            var tapY = gesture.LocationInView(floorplanImageView).Y;

            drawPathFromUser((float) tapX, (float) tapY);

            /*
			// Create a new Alert Controller
			UIAlertController actionSheetAlert = UIAlertController.Create("Options", "Set", UIAlertControllerStyle.Alert);

			// Add Actions
			actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, null));

			//actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, (action) => Console.WriteLine ("Cancel button pressed.")));
			CGPoint point = new CGPoint (gesture.LocationInView (floorplanImageView).X, gesture.LocationInView (floorplanImageView).Y);

			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover!=null) {
				presentationPopover.SourceRect = new CGRect(point, new CGSize(0.1, 0.1));
				//presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}
				
			this.PresentViewController(actionSheetAlert,true,null);
			*/
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}