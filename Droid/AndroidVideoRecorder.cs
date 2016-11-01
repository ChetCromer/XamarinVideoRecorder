using System;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using System.IO;
using Xamarin.Forms;

namespace XamarinVideoRecorder.Droid
{
	public sealed class AndroidVideoRecorder : ViewGroup, ISurfaceHolderCallback
	{
		SurfaceView surfaceView;
		ISurfaceHolder holder;
		Camera.Size previewSize;
		IList<Camera.Size> supportedPreviewSizes;
		public Camera camera;
		MediaRecorder recorder;
		IWindowManager windowManager;
		int cameraId = 1;

		public bool IsPreviewing { get; set; }

		public bool IsCameraAvailable
		{
			//Check if the camera is available
			get
			{
				string fing = Build.Fingerprint;
				if (fing != null)
				{
					if (fing.Contains("vbox") || fing.Contains("generic"))
					{
						return false;
					}
					else return true;
				}
				else return true;
			}
		}

		public void InitCameraPreview()
		{
			//Open  the camera
			try
			{
				//Try to open camera 1 (assume face camera)
				cameraId = 1;
				camera = Camera.Open(cameraId);
			}
			catch (Exception exNoCamera1)
			{
				try
				{
					//Try to open camera 0 (assume rear camera)
					cameraId = 0;
					camera = Camera.Open(cameraId);
				}
				catch (Exception exNoCamera0)
				{
					//Other exception - can't open a camera
					throw (exNoCamera0);
				}
			}

			//Force the camera orientation (may need a different value here)
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				//Don't rotate image - assume landscape on tablets
				camera.SetDisplayOrientation(0);
			}
			else {
				//Rotate camera - assume portrait
				camera.SetDisplayOrientation(90);
			}
			camera.StartPreview();
			IsPreviewing = true;
		}

		public VideoRecorder(Context context)
			: base(context)
		{
			if (IsCameraAvailable)
			{

				recorder = new MediaRecorder();
				surfaceView = new SurfaceView(context);
				AddView(surfaceView);

				windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

				IsPreviewing = false;

				holder = surfaceView.Holder;
				holder.AddCallback(this);
				try
				{
					recorder.SetVideoSource(VideoSource.Camera);
					recorder.SetAudioSource(AudioSource.Mic);
					recorder.SetProfile(CamcorderProfile.Get(cameraId, CamcorderQuality.High));
					//recorder.SetOutputFormat(OutputFormat.Mpeg4);
					//recorder.SetVideoEncoder(VideoEncoder.Default);
					//recorder.SetAudioEncoder(AudioEncoder.Default);
					//recorder.SetPreviewDisplay(holder.Surface);
				}
				catch (Exception ex)
				{
					return;
				}
			}

		}

		public void StartRecording(object sender, EventArgs e)
		{
			//Set path for the video file
			string filepath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string filename = Path.Combine(filepath, "video.mp4");
			//string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/video.mp4";
			GlobalResources.VideoFileName = filename;

			if (IsCameraAvailable)
			{

				if (File.Exists(filename))
				{
					File.Delete(filename);
				}


				//Start recording
				recorder.SetOutputFile(filename);
				recorder.Prepare();
				recorder.Start();
			}
			else {
				//Copy sample video to destiantion path
				FileInfo sample = new FileInfo("sample.mov");
				sample.CopyTo(filename);
			}
		}

		public void StopRecording(object sender, EventArgs e)
		{
			//Stoip recording
			if (IsCameraAvailable)
			{
				recorder.Stop();
				recorder.Release();
				//Do a few other things to clean up
				RemoveView(surfaceView);
			}

		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
			int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
			SetMeasuredDimension(width, height);

			if (supportedPreviewSizes != null)
			{
				previewSize = GetOptimalPreviewSize(supportedPreviewSizes, width, height);
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
			var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

			surfaceView.Measure(msw, msh);
			surfaceView.Layout(0, 0, r - l, b - t);
		}

		public void SurfaceCreated(ISurfaceHolder holder)
		{
			try
			{
				if (camera != null)
				{
					camera.SetPreviewDisplay(holder);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(@"			ERROR: ", ex.Message);
			}
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			if (camera != null)
			{
				camera.StopPreview();
			}
		}

		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			if (width > height)
			{
				GlobalResources.CurrentOrientation = Orientation.LandscapeRight;
			}
			else {
				GlobalResources.CurrentOrientation = Orientation.Portrait;
			}
			//var parameters = camera.GetParameters();
			//parameters.SetPreviewSize(previewSize.Width, previewSize.Height);
			//RequestLayout();

			//switch (windowManager.DefaultDisplay.Rotation)
			//{
			//	case SurfaceOrientation.Rotation0:
			//		camera.SetDisplayOrientation(90);
			//		break;
			//	case SurfaceOrientation.Rotation90:
			//		camera.SetDisplayOrientation(0);
			//		break;
			//	case SurfaceOrientation.Rotation270:
			//		camera.SetDisplayOrientation(180);
			//		break;
			//}

			//camera.SetParameters(parameters);
			//camera.StartPreview();
			//IsPreviewing = true;
		}

		Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
		{
			const double AspectTolerance = 0.1;
			double targetRatio = (double)w / h;

			if (sizes == null)
			{
				return null;
			}

			Camera.Size optimalSize = null;
			double minDiff = double.MaxValue;

			int targetHeight = h;
			foreach (Camera.Size size in sizes)
			{
				double ratio = (double)size.Width / size.Height;

				if (Math.Abs(ratio - targetRatio) > AspectTolerance)
					continue;
				if (Math.Abs(size.Height - targetHeight) < minDiff)
				{
					optimalSize = size;
					minDiff = Math.Abs(size.Height - targetHeight);
				}
			}

			if (optimalSize == null)
			{
				minDiff = double.MaxValue;
				foreach (Camera.Size size in sizes)
				{
					if (Math.Abs(size.Height - targetHeight) < minDiff)
					{
						optimalSize = size;
						minDiff = Math.Abs(size.Height - targetHeight);
					}
				}
			}

			return optimalSize;
		}
	}
}