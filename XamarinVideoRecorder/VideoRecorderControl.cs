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
		public EventHandler OnStartPreviewing;
		public EventHandler OnStopPreviewing;

		public bool IsPreviewing { get; set; }
		public bool IsRecording { get; set; }

		//Start recording
		public void StartRecording()
		{
			if (OnStartRecording != null)
			{
				OnStartRecording(this, new EventArgs());
			}
		}

		//Stop recording
		public void StopRecording()
		{
			if (OnStopRecording != null)
			{
				OnStopRecording(this, new EventArgs());
			}
		}

		//Start previewing
		public void StartPreviewing()
		{
			if (OnStartPreviewing != null)
			{
				OnStartPreviewing(this, new EventArgs());
			}
		}

		//Stop previewing
		public void StopPreviewing()
		{
			if (OnStopPreviewing != null)
			{
				OnStopPreviewing(this, new EventArgs());
			}
		}
	}
}
