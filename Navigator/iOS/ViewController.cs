using System;
using UIKit;

namespace Navigator.iOS
{
    public partial class ViewController : UIViewController
    {
        private int toggle = 1;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			UIView container = new UIView ();
			UIImageView floorplanImageView = new UIImageView();
			PathView pathView = new PathView ();



			UIImage floorplanImageNoGrid;
			UIImage floorplanImageWithGrid;
			LocationArrowImageView locationArrow;


			floorplanImageNoGrid = UIImage.FromBundle ("Images/dcsfloor.jpg");
			floorplanImageWithGrid = UIImage.FromBundle ("Images/dcsFloorGrid.jpg");
			locationArrow = new LocationArrowImageView ();
			locationArrow.setLocation (650, 850);
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
				pathView.setPoints(new CoreGraphics.CGPoint[]{
					new CoreGraphics.CGPoint(currentX, currentY),
					new CoreGraphics.CGPoint(currentX-50, currentY),
					new CoreGraphics.CGPoint(currentX-50, currentY+50),
					new CoreGraphics.CGPoint(currentX+50, currentY+50)
				});

				debugLabel.Text = "scaleFactor " + floorplanView.ZoomScale;
			};
				

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