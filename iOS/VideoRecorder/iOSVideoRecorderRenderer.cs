﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinVideoRecorder;
using XamarinVideoRecorder.iOS;

[assembly: ExportRenderer(typeof(VideoRecorder), typeof(iOSVideoRecorderRenderer))]
namespace XamarinVideoRecorder.iOS
{
	public class iOSVideoRecorderRenderer : ViewRenderer<VideoRecorder, iOSVideoRecorder>
	{
		iOSVideoRecorder recorder;

		protected override void OnElementChanged(ElementChangedEventArgs<VideoRecorder> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				recorder = new iOSVideoRecorder( e.NewElement, e.NewElement.Camera, e.NewElement.Orientation);
				SetNativeControl(recorder);
			}

			if (e.OldElement != null)
			{
				// Unsubscribe
				e.OldElement.OnStartRecording -= OnStartRecording; //unsubscribe from start recording event in xamarin.forms control
				e.OldElement.OnStopRecording -= OnStopRecording; //unsubscribe from stop recording event in xamarin.forms control
				e.OldElement.OnStopPreviewing -= OnStopPreviewing;
				e.OldElement.OnStartPreviewing -= OnStartPreviewing;

			}
			if (e.NewElement != null)
			{
				// Subscribe
				e.NewElement.OnStartRecording += OnStartRecording; //subscribe from start recording event in xamarin.forms control
				e.NewElement.OnStopRecording += OnStopRecording;//subscribe from stop recording event in xamarin.forms control
				e.NewElement.OnStartPreviewing += OnStartPreviewing;
				e.NewElement.OnStopPreviewing += OnStopPreviewing;

				//cameraPreview.Click += OnCameraPreviewClicked;
			}
		}

		void OnStartRecording(object sender, EventArgs e)
		{
			recorder.StartRecording(sender, e);
		}
		void OnStopRecording(object sender, EventArgs e)
		{
			recorder.StopRecording(sender, e);
		}
		void OnStartPreviewing(object sender, EventArgs e)
		{
			recorder.StartPreviewing(sender, e);
		}
		void OnStopPreviewing(object sender, EventArgs e)
		{
			recorder.StopPreviewing(sender, e);
		}



		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}