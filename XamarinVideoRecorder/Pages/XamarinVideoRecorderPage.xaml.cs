using Xamarin.Forms;

namespace XamarinVideoRecorder
{
	public partial class XamarinVideoRecorderPage : ContentPage
	{
		public XamarinVideoRecorderPage()
		{
			InitializeComponent();
		}

		void Handle_StartPreviewing(object sender, System.EventArgs e)
		{
			video.StartPreviewing();
		}
		void Handle_StopPreviewing(object sender, System.EventArgs e)
		{
			video.StopPreviewing();
		}

		void Handle_StartRecording(object sender, System.EventArgs e)
		{
			video.StartRecording();
		}

		void Handle_StopRecording(object sender, System.EventArgs e)
		{
			video.StopRecording();
			System.Diagnostics.Debug.WriteLine("File: {0}", video.VideoFileName);
		}

		void Handle_PlayVideo(object sender, System.EventArgs e)
		{
			video.DoCleanup();
			Navigation.PushAsync(new XamarinVideoPlaybackPage(video.VideoFileName));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Device.StartTimer(new System.TimeSpan(0, 0, 0, 0,250), () =>
			 {
				 if (! video.IsPreviewing)
				 {
					 try
					 {
						 System.Diagnostics.Debug.WriteLine("Trying to start preview...");
						 video.StartPreviewing();
						 System.Diagnostics.Debug.WriteLine("Preview started...");
						 return false;
					 }
					 catch (System.Exception ex)
					 {
						 System.Diagnostics.Debug.WriteLine("Preview start failed... will try again.");
						 return true;
					 }
				} else {
					 System.Diagnostics.Debug.WriteLine("Preview already started");
					 return false;
					}

			 });

		}



	}
}
