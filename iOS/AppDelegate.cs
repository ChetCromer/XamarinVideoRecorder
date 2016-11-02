using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Octane.Xam.VideoPlayer.iOS;
using UIKit;

namespace XamarinVideoRecorder.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			//Init the video player
			FormsVideoPlayer.Init();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
