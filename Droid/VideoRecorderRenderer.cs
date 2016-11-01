using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using GbrApps.Droid;
using GbrApps;
using Android.Hardware;

[assembly: ExportRenderer(typeof(gbrVideoRecorder), typeof(VideoRecorderRenderer))]
namespace XamarinVideoRecorder.Droid
{
	public class VideoRecorderRenderer : ViewRenderer<VideoRecorder, AndroidVideoRecorder>
	{
		VideoRecorder cameraPreview;

		protected override void OnElementChanged(ElementChangedEventArgs<gbrVideoRecorder> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				cameraPreview = new VideoRecorder(Context);
				SetNativeControl(cameraPreview);
			}

			if (e.OldElement != null)
			{
				// Unsubscribe
				e.OldElement.OnStartRecording -= OnStartRecording; //unsubscribe from start recording event in xamarin.forms control
				e.OldElement.OnStopRecording -= OnStopRecording; //unsubscribe from stop recording event in xamarin.forms control
																 //cameraPreview.Click -= OnCameraPreviewClicked;
			}
			if (e.NewElement != null)
			{
				if (cameraPreview.IsCameraAvailable)
				{
					cameraPreview.InitCameraPreview();
					//Control.Preview = Camera.Open((int)e.NewElement.Camera);
				}
				// Subscribe
				e.NewElement.OnStartRecording += OnStartRecording; //subscribe from start recording event in xamarin.forms control
				e.NewElement.OnStopRecording += OnStopRecording;//subscribe from stop recording event in xamarin.forms control

				//cameraPreview.Click += OnCameraPreviewClicked;
			}
		}

		void OnStartRecording(object sender, EventArgs e)
		{
			cameraPreview.StartRecording(sender, e);
		}
		void OnStopRecording(object sender, EventArgs e)
		{
			cameraPreview.StopRecording(sender, e);
		}



		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Control.camera.Release();
			}
			base.Dispose(disposing);
		}
	}
}