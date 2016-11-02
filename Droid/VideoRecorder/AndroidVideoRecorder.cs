﻿using System;
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
		CameraOptions CameraOption;
		OrientationOptions OrientationOption;
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


		public AndroidVideoRecorder(Context context, VideoRecorder CrossPlatformRecorder, CameraOptions cameraOption, OrientationOptions orientationOption)
			: base(context)
		{
			//Store references of recorder and options for later use
			XamRecorder = CrossPlatformRecorder;
			CameraOption = cameraOption;
			OrientationOption = orientationOption;

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


		private bool OpenCamera(int id)
		{
			//Optns a camera and return true/false if it worked.
			try
			{
				camera = Camera.Open(id);
				cameraId = id;
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		private void InitCamera()
		{
			bool success = false;

			//OPEN THE APPROPRIATE CAMERA
			if (CameraOption == CameraOptions.Front)
			{
				success = OpenCamera(1); //Try the face camera
				if (success == false)
				{
					success = OpenCamera(0); //Try the rear camera
				}
			}
			else {
				success = OpenCamera(0); //Try the rear camera
			}

			if (success = false)
			{
				throw new Exception("Unable to open camera.");
			}

			//GET THE CORRECT CAMERA ORIENTATION
			if (OrientationOption == OrientationOptions.Landscape)
			{
				//Landscape
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					camera.SetDisplayOrientation(0);
				}
				else {
					camera.SetDisplayOrientation(90);
				}
			}
			else {
				//Portrait
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					camera.SetDisplayOrientation(270);
				}
				else {
					camera.SetDisplayOrientation(0);
				}

			}

			//FIND BEST SIZE FOR RECORDER
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
			if (XamRecorder.IsRecording)
			{
				throw new Exception("You can't stop previewing while you're recording.");
			}

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
			////Make sure we're in a good state before we start recording
			//if (!XamRecorder.IsPreviewing)
			//{
			//	throw new Exception("You can't start recording until you are previewing.");
			//}

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
				camera.Unlock();
				recorder.SetCamera(camera);
				recorder.SetVideoSource(VideoSource.Camera);
				recorder.SetAudioSource(AudioSource.Mic);
				recorder.SetProfile(CamcorderProfile.Get(cameraId, CamcorderQuality.High));
				//recorder.SetVideoEncoder(VideoEncoder.Default); //Also tried default
				//recorder.SetAudioEncoder(AudioEncoder.Default); //Also tried default
				//recorder.SetOutputFormat(OutputFormat.Mpeg4);
				recorder.SetOutputFile(filename);

				recorder.SetPreviewDisplay(holder.Surface);

				recorder.Prepare();
				recorder.Start();
			}
			else {
				//Copy sample video to destiantion path just to have something there.
				FileInfo sample = new FileInfo("sample.mp4");
				sample.CopyTo(filename);
			}

			XamRecorder.IsPreviewing = true;
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

			FileInfo fil = new FileInfo(XamRecorder.VideoFileName);
			System.Diagnostics.Debug.WriteLine("File is ready: {0} {1} bytes", fil.FullName, fil.Length.ToString());

		}

		public void DoCleanup(object sender, EventArgs e)
		{

			if (XamRecorder.IsPreviewing || XamRecorder.IsRecording)
			{
				throw new Exception("You can't do cleanup while you are previewing or recording.");
			}

			try
			{
				//Clean things up so we don't have s surface stuck on top of other surfaces later.
				holder.RemoveCallback(this);
				holder = null;
				RemoveView(surfaceView);
				surfaceView = null;
			}
			catch (Exception ex)
			{

			}

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
			if (surfaceView != null)
			{
				System.Diagnostics.Debug.WriteLine("OnLayout");
				var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
				var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
				surfaceView.Measure(msw, msh);
				surfaceView.Layout(0, 0, r - l, b - t);
			}
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			try
			{
				if (disposing)
				{
					camera.Release();
				}
			}
			catch (Exception ex)
			{

			}

		}
	}
}