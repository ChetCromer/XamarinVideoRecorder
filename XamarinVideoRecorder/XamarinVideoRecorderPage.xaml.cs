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
			System.Diagnostics.Debug.WriteLine("File: {0}",video.VideoFileName);
		}

	}
}
