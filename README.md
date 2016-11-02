# XamarinVideoRecorder
A simple Xamarin video recorder using custom rendererers for Android and iOS.

I made this project because I've been fighting the MediaRecorder in the Android side of a larger Xamarin.Forms project. 
This project was made to get the custom renderer working as I wanted to with the granularity I needed.

This isn't a production-ready project, but it does do the basics:

* Start Preview
* Start Recording (you can skip the start preview on Android but not iOS)
* Stop Recording
* Stop Preview

After recording a video, you can proceed on to play that video using the Octane Video Player found at
https://www.nuget.org/packages/Octane.Xam.VideoPlayer/ We've had some issues with this player, especially
on the Android side of things, but does do a lot of the heavy lifting and hasn't required me to make my own 
video PLAYER custom renderer just yet, although we have extended it a bit for some features we needed, 
specifically seeking to a particular point in the video.

If you get some use out of this project or have other ideas for it, let me know!
