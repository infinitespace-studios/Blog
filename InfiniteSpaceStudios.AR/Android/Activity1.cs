using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;

using Microsoft.Xna.Framework;
using InfiniteSpaceStudios.AR.Droid;
using Android.Hardware;

namespace InfiniteSpaceStudios.AR.Droid.Droid
{
	[Activity (Label = "InfiniteSpaceStudios.AR.Droid", 
		MainLauncher = true,
		Icon = "@drawable/icon",
		Theme = "@style/Theme.Splash",
		AlwaysRetainTaskState = true,
		LaunchMode = LaunchMode.SingleInstance,
		ScreenOrientation = ScreenOrientation.Landscape,
		ConfigurationChanges = ConfigChanges.Orientation |
		ConfigChanges.KeyboardHidden |
		ConfigChanges.Keyboard)]
	public class Activity1 : AndroidGameActivity
	{

		CameraView cameraView;
		Camera camera;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create our OpenGL view, and display it
			Game1.Activity = this;
			var g = new Game1 ();
			FrameLayout frameLayout = new FrameLayout(this);
			var view = g.Services.GetService<View> ();
			view.JavaCast<OpenTK.Platform.Android.AndroidGameView> ().SurfaceFormat = Android.Graphics.Format.Rgba8888;
			frameLayout.AddView (g);  
			try {
				camera = Camera.Open ();
				cameraView = new CameraView (this, camera);
				frameLayout.AddView (cameraView);
			} catch (Exception e) {
				// oops no camera
				Android.Util.Log.Debug ("CameraView", e.ToString ());
			}
			SetContentView (frameLayout);
			g.Run ();
		}
		
	}

	public class CameraView : SurfaceView, ISurfaceHolderCallback {
		Camera camera;

		public CameraView (Context context, Camera camera) : base(context)
		{
			this.camera = camera;
			Holder.AddCallback(this);
			// deprecated setting, but required on Android versions prior to 3.0
			Holder.SetType(SurfaceType.PushBuffers);
		}

		public void SurfaceChanged (ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			// If your preview can change or rotate, take care of those events here.
			// Make sure to stop the preview before resizing or reformatting it.

			if (Holder.Surface == null){
				// preview surface does not exist
				return;
			}

			// stop preview before making changes
			try {
				camera.StopPreview();
			} catch (Exception e){
				// ignore: tried to stop a non-existent preview
			}

			// set preview size and make any resize, rotate or
			// reformatting changes here

			// start preview with new settings
			try {
				camera.SetPreviewDisplay(Holder);
				camera.StartPreview();

			} catch (Exception e){
				Android.Util.Log.Debug ("CameraView", e.ToString ());
			}
		}

		public void SurfaceCreated (ISurfaceHolder holder)
		{
			// The Surface has been created, now tell the camera where to draw the preview.
			try {
				camera.SetPreviewDisplay(holder);
				camera.StartPreview();
			} catch (Exception e) {
				Android.Util.Log.Debug ("CameraView", e.ToString ());
			}
		}

		public void SurfaceDestroyed (ISurfaceHolder holder)
		{

		}
	}
}


