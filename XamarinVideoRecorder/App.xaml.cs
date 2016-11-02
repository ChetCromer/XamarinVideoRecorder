using Xamarin.Forms;

namespace XamarinVideoRecorder
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			//MainPage = new ContentPage();
			MainPage = new XamarinVideoRecorderPage();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
