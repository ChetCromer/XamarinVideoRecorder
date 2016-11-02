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
using XamarinVideoRecorder;

namespace XamarinVideoRecorder.Droid
{
	public sealed class AndroidVideoRecorder : ViewGroup, ISurfaceHolderCallback
	{
		VideoRecorder XamRecorder;
		SurfaceView surfaceView;
		ISurfaceHolder holder;
		Camera.Size previewSize;
		IList<Camera.Size> supportedPreviewSizes;
		public Camera camera;
		MediaRecorder recorder;
		IWindowManager windowManager;
		int cameraId = 1;
		bool SurfacePrepared = false;


		public bool IsCameraAvailable
		{
			//Check if the camera is available (for now we're just checking for a simulator)
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


		public AndroidVideoRecorder(Context context, VideoRecorder CrossPlatformRecorder)
			: base(context)
		{

			XamRecorder = CrossPlatformRecorder;

			if (IsCameraAvailable)
			{

				//Create the surface for drawing on
				surfaceView = new SurfaceView(context);
				AddView(surfaceView);
				holder = surfaceView.Holder;
				holder.AddCallback(this);

				windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

				InitCamera();

				XamRecorder.IsPreviewing = false;
				XamRecorder.IsRecording = false;
			}

		}

		private void InitCamera()
		{
			//Get a reference for the camera
			//Open  the camera
			try
			{
				//Try to open camera 1 (assume face camera for now)
				cameraId = 1;
				//Don't forget to add your permissions to the manifest or this will cause an excepiotion
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

			//Get the camera's supported preview sizesnow.
			var parameters = camera.GetParameters();
			supportedPreviewSizes = parameters.SupportedPreviewSizes;
			RequestLayout();
		}

		public void StartPreviewing(object sender, EventArgs e)
		{
			if (XamRecorder.IsPreviewing)
			{
				throw new Exception("Preview has already started.");
			}

			if (!SurfacePrepared)
			{
				throw new Exception("The surface is not ready yet.");
			}

			//Start the preview
			var parameters = camera.GetParameters();
			parameters.SetPreviewSize(previewSize.Width, previewSize.Height);
			camera.SetPreviewDisplay(holder);
			//camera.Unlock();

			camera.StartPreview();
			XamRecorder.IsPreviewing = true;
		}

		public void StopPreviewing(object sender, EventArgs e)
		{
			if (XamRecorder.IsPreviewing)
			{
				//Stop the preview
				camera.StopPreview();

				//TODO: release anything with the surface we may need to?
				//RemoveView(surfaceView);

				XamRecorder.IsPreviewing = false;
			} //else nothing to do if 

		}

		public void StartRecording(object sender, EventArgs e)
		{
			//Make sure we're in a good state before we start recording
			if (!XamRecorder.IsPreviewing)
			{
				throw new Exception("You can't start recording until you are previewing.");
			}

			if (XamRecorder.IsRecording)
			{
				throw new Exception("You can't start recording because you are already recording.");
			}

			//Set path for the video file
			string filepath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string filename = Path.Combine(filepath, "video.mp4");
			//string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/video.mp4";
			XamRecorder.VideoFileName = filename;

			if (IsCameraAvailable)
			{
				//Delete the file if it already exists
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}


				//Start recording
				recorder = new MediaRecorder();
				recorder.SetVideoSource(VideoSource.Camera);
				recorder.SetAudioSource(AudioSource.Mic);
				//recorder.SetProfile(CamcorderProfile.Get(cameraId, CamcorderQuality.High));
				//recorder.SetPreviewDisplay(holder.Surface); //Not sure if we want this or not since preview is already going.
				recorder.SetOutputFormat(OutputFormat.Mpeg4);
				recorder.SetVideoEncoder(VideoEncoder.Default);
				recorder.SetAudioEncoder(AudioEncoder.Default);
				recorder.SetOutputFile(filename);

				camera.Unlock();

				recorder.Prepare();
				recorder.Start();
			}
			else {
				//Copy sample video to destiantion path just to have something there.
				FileInfo sample = new FileInfo("sample.mp4");
				sample.CopyTo(filename);
			}

			XamRecorder.IsRecording = true;
		}

		public void StopRecording(object sender, EventArgs e)
		{

			if (!XamRecorder.IsRecording)
			{
				throw new Exception("You can't stop recording because it's not started yet.");
			}

			if (IsCameraAvailable)
			{
				//Stop recording
				recorder.Stop();
				recorder.Release();
			}
			XamRecorder.IsRecording = false;

		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			System.Diagnostics.Debug.WriteLine("OnMeasure");
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
			System.Diagnostics.Debug.WriteLine("OnLayout");
			var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
			var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
			surfaceView.Measure(msw, msh);
			surfaceView.Layout(0, 0, r - l, b - t);
		}

		public void SurfaceCreated(ISurfaceHolder holder)
		{
			System.Diagnostics.Debug.WriteLine("SurfaceCreated");
			SurfacePrepared = true;
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			System.Diagnostics.Debug.WriteLine("SurfaceDestroyed");
			SurfacePrepared = false;
			if (XamRecorder.IsRecording)
			{
				StopRecording(null, null);
			}

			if (XamRecorder.IsPreviewing)
			{
				StopPreviewing(null, null);
			}
		}

		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			System.Diagnostics.Debug.WriteLine("SurfaceDestroyed");
			if (width > height)
			{
				//TODO: flag that we're landscape
			}
			else {
				//TODO: flag that we're portrait
			}
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

			System.Diagnostics.Debug.WriteLine("GetOptimalPreviewSize: W{0}/H{0}", optimalSize.Height, optimalSize.Width);
			return optimalSize;
		}
	}
}