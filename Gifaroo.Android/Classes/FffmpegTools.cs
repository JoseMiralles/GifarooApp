using System;

using Com.Github.Hiteshsondhi88.Libffmpeg;
using Android.Graphics;
using Android.Content;

namespace Gifaroo.Android
{
	static public class FffmpegTools{

		/// <summary>
		/// checks if the height or the width of the gif is the biggest and returns a piece of the command.
		/// </summary>
		/// <returns>The resize command dimensions.</returns>
		/// <param name="context">Context.</param>
		public static string GetResizeCommandDimensions(Context context, string fileName){
			BitmapFactory.Options options = new BitmapFactory.Options ();
			options.InJustDecodeBounds = true;
			string dim = "";
			
			BitmapFactory.DecodeFile (context.FilesDir +"/"+ fileName, options);
			if (options.OutWidth > options.OutHeight)
				dim = "700:-1";
			else
				dim = "-1:700";
			return dim;
		}

		public class XLoadBinaryResponseHandler: LoadBinaryResponseHandler{
			public event EventHandler OnFfmpegLoadindAttemptFinished;
			public bool failed = false;

			public override void OnFailure (){
				failed = true;
			}
			public override void OnSuccess (){
				failed = false;
			}
			public override void OnFinish (){
				if (OnFfmpegLoadindAttemptFinished != null) {
					OnFfmpegLoadindAttemptFinished (this, EventArgs.Empty);
				}
			}

			public override void OnStart ()
			{
				bool started = true;
			}
		}
		public class XExecuteBinaryResponseHandler: ExecuteBinaryResponseHandler{
			public int commandsPosition = 0;
			public bool failed = false;
			/// <summary>
			/// Occurs when an ffmpeg execution is finished regardles of wether it succedeed or failed.
			/// </summary>
			public event EventHandler OnExecutionFinished;

			public override void OnFailure (string p0){
				failed = true;
				//TODO remove on release
				System.Console.WriteLine ("FFMPEG BINARY EXECUTE:\t FAILED\n{0}",p0);
			}
			public override void OnSuccess (string p0){
				failed = false;
				commandsPosition++;
				//TODO remove on release
				System.Console.WriteLine ("FFMPEG BINARY EXECUTE:\t SUCCESS\n{0}",p0);
			}
			public override void OnFinish (){
				if (OnExecutionFinished != null)
					OnExecutionFinished (this, EventArgs.Empty);
			}
		}
	}
}