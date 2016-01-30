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

			UIImageView floorplanImage;
			UIImageView floorplanImageNoGrid;
			UIImageView floorplanImageWithGrid;


			floorplanImageNoGrid = new UIImageView (UIImage.FromBundle ("Images/dcsfloor.jpg"));
			floorplanImageWithGrid = new UIImageView (UIImage.FromBundle ("Images/dcsFloorGrid.jpg"));
			floorplanImage = floorplanImageNoGrid;

			floorplanView.ContentSize = floorplanImage.Image.Size;
			floorplanView.AddSubview (floorplanImage);

			floorplanView.MaximumZoomScale = 3f;
			floorplanView.MinimumZoomScale = .25f;
			floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImage; };

            // Perform any additional setup after loading the view, typically from a nib.
            Button.AccessibilityIdentifier = "myButton";
            Button.TouchUpInside += delegate
            {
				if (toggle == 1) {
                	var title = string.Format("Set to normal floorplan");
					Button.SetTitle(title, UIControlState.Normal);

					floorplanImage = floorplanImageWithGrid;
					floorplanView.AddSubview (floorplanImage);
					floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImage; };


					toggle = 0;
				}
				else {
					var title = string.Format("Set to floorplan with grid");
					Button.SetTitle(title, UIControlState.Normal);

					floorplanImage = floorplanImageNoGrid;
					floorplanView.AddSubview (floorplanImage);
					floorplanView.ViewForZoomingInScrollView += (UIScrollView sv) => { return floorplanImage; };

					toggle = 1;
				}
            };
				

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}