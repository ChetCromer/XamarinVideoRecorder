using System;
using Xamarin.Forms;

namespace XamarinVideoRecorder
{

	public class GetVideoFileNameArgs : EventArgs
	{
		public string Name { get; internal set; }
		public GetVideoFileNameArgs(string Name)
		{ this.Name = Name; }

		public GetVideoFileNameArgs() { }

	}

	public enum CameraOptions
	{
		Rear,
		Front
	}

	public class VideoRecorder : View
	{
		public static readonly BindableProperty CameraProperty =
			BindableProperty.Create<VideoRecorder, CameraOptions>(p => p.Camera, CameraOptions.Rear);

		public string VideoFileName { get; set; }

		public CameraOptions Camera
		{
			get { return (CameraOptions)GetValue(CameraProperty); }
			set { SetValue(CameraProperty, value); }
		}

		public EventHandler OnStartRecording;
		public EventHandler OnStopRecording;

		public void StartRecording()
		{
			if (OnStartRecording != null)
			{
				OnStartRecording(this, new EventArgs());
			}
		}

		public void StopRecording()
		{
			if (OnStopRecording != null)
			{
				OnStopRecording(this, new EventArgs());
			}
		}

	}
}
