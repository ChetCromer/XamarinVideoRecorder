using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XamarinVideoRecorder
{
	public partial class XamarinVideoPlaybackPage : ContentPage
	{
		public XamarinVideoPlaybackPage( string VideoFile)
		{
			InitializeComponent();

			//Set the video player up to play the file.
			VideoPlayer.Source = VideoFile;
		}
	}
}
