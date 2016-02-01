// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Navigator.iOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIButton Button { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel debugLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIScrollView floorplanView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton OptionsButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton simulationButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Button != null) {
				Button.Dispose ();
				Button = null;
			}
			if (debugLabel != null) {
				debugLabel.Dispose ();
				debugLabel = null;
			}
			if (floorplanView != null) {
				floorplanView.Dispose ();
				floorplanView = null;
			}
			if (OptionsButton != null) {
				OptionsButton.Dispose ();
				OptionsButton = null;
			}
			if (simulationButton != null) {
				simulationButton.Dispose ();
				simulationButton = null;
			}
		}
	}
}
