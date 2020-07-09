
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System.Threading.Tasks;
using Android.Graphics;
using Java.Util;
using Java.IO;
using Android.Media;
using Android.Util;

using Com.Github.Hiteshsondhi88.Libffmpeg;
using Gifaroo.Android;

using Flurry.Ads;
using Gifaroo.Android.Classes;

namespace Gifaroo.Android
{
	[Activity (
	Label = "save",
	ScreenOrientation = global::Android.Content.PM.ScreenOrientation.Portrait
	)]

	public class SaveActivity : Activity
	{
		Activity activity;
		private FFmpeg ffmpeg = null;
		private string internalPath;
		private string rawFileName;
		private string outputName;
		private string commandDimensions;
		private string finalFileName;
		private ProgressDialog progressD;
		private bool shouldRunffmpeg = true;
		private bool threadsCanceled = false;
		private PowerManager pm;
        private PowerManager.WakeLock wl;
        private string _userPremiumStatus;
        private Flurry.Ads.FlurryAdNative nativeAd;
        private TrackingInterface trackingInterface;

		//UI fields
		private Button shareButton;
		private LinearLayout saveLayout;
		private LinearLayout mainSaveLayout;
        private LinearLayout adLinearLayout;
        private LinearLayout adLayout;
        private AdViewHolder adViewHolder;
        private IMenuItem homeButton;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Save);
            this._userPremiumStatus
                = Intent.GetStringExtra(PremiumInterface.UserPremiunStatus.StatusFlags.ACTIVITY_FLAG);

            //set up tracking and track this activity
            trackingInterface = new TrackingInterface(this);
            //trackingInterface.TrackScreen(TrackingInterface.ActivityNames.SaveActivity);

			shareButton = FindViewById<Button> (Resource.Id.shareButton);
			saveLayout = FindViewById<LinearLayout> (Resource.Id.saveLayout);
			mainSaveLayout = FindViewById<LinearLayout> (Resource.Id.mainSaveLayout);
            adLinearLayout = FindViewById<LinearLayout> (Resource.Id.adLinearLayout);
            adViewHolder = new AdViewHolder(this);

			pm = (PowerManager)GetSystemService (Context.PowerService);
			wl = pm.NewWakeLock (WakeLockFlags.ScreenDim, "My Tag");
			ActionBar.Title = "";
			internalPath = FilesDir.Path;
			rawFileName = "my_file";

            List<string> fileNamesArray = new List<string>();
            foreach (File file in this.FilesDir.ListFiles()) {
                fileNamesArray.Add(file.Name);
            }

            rawFileName = GetFullFileNameFromArray(rawFileName, fileNamesArray.ToArray());
			outputName = "final.mp4";
			finalFileName = "Gifaroo" + UUID.RandomUUID() + ".mp4";
			ffmpeg = FFmpeg.GetInstance(this);
			commandDimensions = FffmpegTools.GetResizeCommandDimensions (this, rawFileName);

			mainSaveLayout.RemoveView (saveLayout);
            mainSaveLayout.RemoveView (adViewHolder.adLinearLayout);

            //Load ad only if the user is not premium
            if (this._userPremiumStatus == PremiumInterface.UserPremiunStatus.StatusFlags.STATUS_FREE)
            {
                Flurry.Analytics.FlurryAgent.SetLogEnabled(false);
                Flurry.Analytics.FlurryAgent.Init(this, "ZTKSCWYBWHW9D6F28KY5");
                nativeAd = new Flurry.Ads.FlurryAdNative(this, "save_activity_native_ad");
                nativeAd.FetchAd();
            }

			#region ffmpeg load & execution
			CreateTxtFile(this);

			string[] commands = new string[]
			{
				"-y -i "+internalPath+"/"+rawFileName+" -vf scale="+ commandDimensions +" -preset superfast "+internalPath+"/output_mp4.mp4",
				"-y -i "+internalPath+"/background.jpg -vf scale=700:700 "+internalPath+"/background_scaled.jpg",
				"-y -i "+internalPath+"/top.png -vf scale=700:700 "+internalPath+"/top_scaled.png",
				"-y -loop 1 -i "+internalPath+"/background_scaled.jpg -i "+internalPath+"/output_mp4.mp4 -filter_complex overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2:shortest=1 -preset superfast -codec:a copy -movflags +faststart "+internalPath+"/output_1.mp4",
				"-y -i "+internalPath+"/output_1.mp4 -loop 1 -i "+internalPath+"/top_scaled.png -filter_complex overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2:shortest=1 -preset superfast -codec:a copy -movflags +faststart "+internalPath+"/final_unlooped.mp4",
				"-y -f concat -i "+internalPath+"/myList.txt -c copy "+internalPath+"/mute_final.mp4",
				"-y -i "+internalPath+"/mute_final.mp4 -i "+internalPath+"/10sec.mp3 -c:v libx264 -crf 19 -preset ultrafast -shortest -c:a aac -strict experimental -pix_fmt yuv420p -f mp4 -b:a 192k -y " +internalPath+ "/" +outputName
			};

            string[] progressMessages = new string[]
			{
				"Doing some work...",
				"Making progress...",
				"Still working...",
				"Just a little bit more...",
				"Adding some final touches..."
			};


			//attempt to load ffmpeg and then execute the commands.
			FffmpegTools.XLoadBinaryResponseHandler loadResponseHandler
                = new FffmpegTools.XLoadBinaryResponseHandler ();
			FffmpegTools.XExecuteBinaryResponseHandler executeResponseHandler
                = new FffmpegTools.XExecuteBinaryResponseHandler ();
			
			//copy the ute audio file to the apps internal directory if it does not exist yet.
			if (new File(FilesDir.Path, "10sec.mp3").Exists() == false)
			{
				try
				{
					CopyFromAssetsToStorage(this, "10sec.mp3", "10sec.mp3");
				}catch(Exception){
					Log.Debug("GIFAROO", "10sec.mp3 copy failed!"); //TODO: Remove for release
				}
			}

			if (shouldRunffmpeg == true)
			{
				wl.Acquire();
				progressD = new ProgressDialog(this);
				progressD.SetMessage(progressMessages[0]);
				progressD.SetCanceledOnTouchOutside(false);
				XOnCancelListener cancelListener = new XOnCancelListener();
				progressD.SetOnCancelListener(cancelListener);

				cancelListener.EOnCanceled += (object cancelSender, EventArgs eCancel) => {
					AlertDialog.Builder builder = new AlertDialog.Builder (this);
					builder.SetMessage ("Are you sure that you want to cancel?").SetTitle("Cancel?");
					builder.SetPositiveButton ("Yes",
						((sender, e) => {
							try{
								threadsCanceled = true;
								Finish();
							}
							catch(Exception){}
						}));
					builder.SetNegativeButton ("No",
						((sender, e) => {
							progressD.Show();
						}));
					AlertDialog aDialog = builder.Create();
					aDialog.Show();
				};

				//Launched when the attempt to load the ffmpeg is finished
				loadResponseHandler.OnFfmpegLoadindAttemptFinished += (object sender, EventArgs e) => {
                    if (loadResponseHandler.failed == false)
                    {
                        new TaskFactory().StartNew(() =>
                        {
                            //var commandArray = new[] { commands[executeResponseHandler.commandsPosition] };
                            ffmpeg.Execute(
                                commands[executeResponseHandler.commandsPosition].Split(),
                                executeResponseHandler);
                        });
                    }
                    else if (loadResponseHandler.failed == true)
                        trackingInterface.TrackFffmgFailure(true);
				};

				executeResponseHandler.OnExecutionFinished += (object sender, EventArgs e) => {
					if (executeResponseHandler.commandsPosition < commands.Length
						&& threadsCanceled == false
                        && executeResponseHandler.failed == false){ //run only if the threads hav not been canceled
						try{
							progressD.SetMessage(progressMessages[executeResponseHandler.commandsPosition]);
						}catch(Exception){}
						new TaskFactory().StartNew(()=>{
                            ffmpeg.Execute(
                                commands[executeResponseHandler.commandsPosition].Split(),
                                executeResponseHandler);
						});

						//TODO: find a better implementation
						if (executeResponseHandler.failed == true) {
                            //Report failure to analytics
                            trackingInterface.TrackFffmgFailure(false, executeResponseHandler.commandsPosition);
							//One of the commands failed
							progressD.Dismiss();
							if (wl.IsHeld) //release the wakelock if it is held
                                wl.Release();
							System.Console.WriteLine("FAILED: ");
						}

					} else {
						//all commands were executed, share and show ad if the user is not premium.
						if (progressD != null && progressD.IsShowing)
                            progressD.Dismiss();
						wl.Release();
						if (executeResponseHandler.failed == false){
							//all commands succeded
							mainSaveLayout.AddView(saveLayout);

                            //show ad
                            if (_userPremiumStatus == PremiumInterface.UserPremiunStatus.StatusFlags.STATUS_FREE
                                && nativeAd.IsReady == true)
                            {
                                nativeAd.SetTrackingView(adLinearLayout);

                                if (nativeAd.GetAsset("summary") != null)
                                    nativeAd.GetAsset("summary").LoadAssetIntoView(adViewHolder.summary);
                                if (nativeAd.GetAsset("headline") != null)
                                    nativeAd.GetAsset("headline").LoadAssetIntoView(adViewHolder.headline);
                                if (nativeAd.GetAsset("source") != null)
                                    nativeAd.GetAsset("source").LoadAssetIntoView(adViewHolder.source);

                                if (nativeAd.GetAsset("secBrandingLogo") != null)
                                    nativeAd.GetAsset("secBrandingLogo").LoadAssetIntoView(adViewHolder.secBrandingLogo);
                                if (nativeAd.GetAsset("secHqImage") != null)
                                    nativeAd.GetAsset("secHqImage").LoadAssetIntoView(adViewHolder.secHqImage);
                                if (nativeAd.GetAsset("secImage") != null)
                                    nativeAd.GetAsset("secImage").LoadAssetIntoView(adViewHolder.secImage);
                                if (nativeAd.GetAsset("headline") != null)
                                    nativeAd.GetAsset("headline").LoadAssetIntoView(adViewHolder.headline);
                                if (nativeAd.GetAsset("callToAction") != null)
									adViewHolder.callToAction.Text = nativeAd.GetAsset("callToAction").Value;
								
                                mainSaveLayout.AddView(adViewHolder.adLinearLayout);
                            }
                            else { //make it all non-visible 
                            
                            }

						}
					}
				};

				progressD.Show();
				//new TaskFactory ().StartNew(()=>{
				try{
					ffmpeg.LoadBinary (loadResponseHandler);
				}catch(Exception){}
				//});
			}

			shareButton.Click += (object sender, EventArgs e) => {
				CreateVideoShareIntent(this, outputName);
			};
			#endregion

		}

		public override void OnBackPressed ()
		{
			Finish ();
		}
		protected override void OnPause ()
		{
			shouldRunffmpeg = false;
			base.OnPause ();
		}
        protected override void OnStop(){
            base.OnStop();
            Flurry.Analytics.FlurryAgent.OnEndSession(this);
        }
        protected override void OnDestroy()
        {
            nativeAd.Destroy();
            base.OnDestroy();
        }

		/// <summary>
		/// Creates a txt file that can then be used by the ffmpeg binaries to know how many times it should loop the final video.
		/// </summary>
		/// <returns><c>true</c>, if text file was created, <c>false</c> otherwise.</returns>
		/// <param name="context">Context.</param>
		private bool CreateTxtFile(Context context){
			long duration = 1;
			int amountOfLoops = 5;

			try{
				System.IO.Stream fis = OpenFileInput(rawFileName);
				Movie movie = Movie.DecodeStream(fis);
				duration = movie.Duration();
			}
			catch(Exception ex){
				Log.Debug ("GIFAROO","Failed to create txt file! -- \n" + ex); //TODO: Remove
			}
			if (duration < 15000 && duration > 0)
				amountOfLoops = (int) (15000/duration);

			string fileString = "";
			string unloopedFileName = "final_unlooped.mp4";
			for (int i = 1; i <= amountOfLoops; i++)
				fileString += "file '" + unloopedFileName + "'\n";

			try{
				System.IO.Stream fileout = OpenFileOutput("myList.txt", FileCreationMode.Private);
				OutputStreamWriter outputWriter = new OutputStreamWriter(fileout);
				outputWriter.Write(fileString);
				outputWriter.Close();

				File file = context.GetFileStreamPath("myList.txt");
				if(file.Exists())
					return true;
			}
			catch(Exception ex){
				Log.Debug ("GIFAROO","Failed to create txt file! -- \n" + ex); //TODO: Remove
			}
				
			return false;
		}

		/// <summary>
		/// used to get the correct file.
		/// </summary>
		/// <returns>The full file name from array.</returns>
		/// <param name="startsWith">Starts with.</param>
		/// <param name="fileNameList">File name list.</param>
		private string GetFullFileNameFromArray(String startsWith, String[] fileNameList){
			for (int x = 0; x < fileNameList.Length; x++){
				if (fileNameList [x].StartsWith (startsWith))
					return fileNameList [x];
			}
			return null;
		}

		/// <summary>
		/// saves and shares the final video.
		/// </summary>
		/// <param name="c">C.</param>
		/// <param name="fileName">File name.</param>
		private void CreateVideoShareIntent(Context c, string fileName)
        {
			File publicDir = new File (
				global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryMovies).Path,
				File.Separator + "Gifaroo");
			publicDir.Mkdir ();
			File externalFile = new File (publicDir.Path + File.Separator, finalFileName);

			try{
				CopyFileFromInternalToExternalDirectory(c, fileName, externalFile);
			}catch(Exception ex){
				Log.Debug ("GIFAROO","Failed to copy from internal to external! -- \n" + ex); //TODO: Remove
                Toast.MakeText(c, "Error saving file! Make sure that you have enough space.", ToastLength.Long).Show();
			}

			xMediaScannerConnection meddiaScannerConnection = new xMediaScannerConnection ();
			meddiaScannerConnection.EOnScanCompleted += (object sender, EventArgs e) => {
				global::Android.Net.Uri vidUri = global::Android.Net.Uri.FromFile(externalFile);
				Intent shareIntent = new Intent();
				shareIntent.SetAction(Intent.ActionSend);
				shareIntent.PutExtra(Intent.ExtraStream, vidUri);
				shareIntent.SetType("video/*");

                

				StartActivity(Intent.CreateChooser(shareIntent, "Open with..."));
			};
			MediaScannerConnection.ScanFile (c,
				new string[] { externalFile.ToString () },
				null,
				meddiaScannerConnection);
		}
		private bool CopyFileFromInternalToExternalDirectory(Context context, string sourceFile, File destinationFile)
        {
			bool success = false;
			InputStream IS = new FileInputStream (context.FilesDir.Path + File.Separator + sourceFile);
			OutputStream OS = new FileOutputStream (destinationFile);
			success = CopyStream (IS, OS);
			OS.Flush ();
			OS.Close ();
			IS.Close ();

            if (destinationFile.Exists())
            {
                Toast.MakeText(context, "Saved to Gallery", ToastLength.Short).Show();
            }
            else
            {
                throw new Exception();
            }
                return success;
		}
		private bool CopyStream(InputStream input, OutputStream output){
			byte[] buffer = new byte[5120];
			int length = input.Read (buffer);
			while(length > 0){
				output.Write (buffer, 0, length);
				length = input.Read (buffer);
			}
			return true;
		}

		private bool CopyFromAssetsToStorage(Context context, string sourceFile, String destinationFile){
			bool success = false;
			var IS = context.Assets.Open (sourceFile);
			//var OS = new FileOutputStream (context.FilesDir.Path + File.Separator + destinationFile);
			var OS  = context.OpenFileOutput(destinationFile, global::Android.Content.FileCreationMode.Private);

			IS.CopyTo (OS);

			OS.Flush ();
			OS.Close ();
			IS.Close ();
			return success;
   		}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = this.MenuInflater;
            inflater.Inflate(Resource.Menu.menu, menu);
            
            homeButton = (IMenuItem)menu.FindItem(Resource.Id.homePageButton).ActionView;

            menu.FindItem(Resource.Id.editButton).SetVisible(false);
            menu.FindItem(Resource.Id.saveButton).SetVisible(false);
            menu.FindItem(Resource.Id.downloadButton).SetVisible(false);
            menu.FindItem(Resource.Id.storeButton).SetVisible(false);
            menu.FindItem(Resource.Id.URLBar).SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId) { 
                case Android.Resource.Id.homePageButton:

                    var mainActivityIntent = new Intent(this, typeof(MainActivity));
                    mainActivityIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(mainActivityIntent);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

		class XOnCancelListener: Java.Lang.Object, IDialogInterfaceOnCancelListener{
			public event EventHandler EOnCanceled;
			public void OnCancel(IDialogInterface dialog){
				if (EOnCanceled != null)
					EOnCanceled (this, EventArgs.Empty);
			}
		}
		class xMediaScannerConnection: Java.Lang.Object, MediaScannerConnection.IOnScanCompletedListener{
			public event EventHandler EOnScanCompleted;
			public void OnScanCompleted(string path, global::Android.Net.Uri uri){
				if (EOnScanCompleted != null) {
					EOnScanCompleted (this, EventArgs.Empty);
				}
			}
		}

        class AdViewHolder {
            public LinearLayout adLinearLayout;
            public ImageView secHqImage;
            public TextView headline;
            public ImageView secBrandingLogo;
            public TextView source;
            public ImageView mainImage;
            public TextView summary;
            public Button callToAction;
            public ImageView secImage;

            public AdViewHolder (Activity activity) {
                adLinearLayout = activity.FindViewById<LinearLayout>(Resource.Id.adLinearLayout);
                secHqImage = activity.FindViewById<ImageView>(Resource.Id.secHqImage);
                secBrandingLogo = activity.FindViewById<ImageView>(Resource.Id.secBrandingLogo);
                mainImage = activity.FindViewById<ImageView>(Resource.Id.mainImage);
                headline = activity.FindViewById<TextView>(Resource.Id.headline);
                source = activity.FindViewById<TextView>(Resource.Id.source);
                summary = activity.FindViewById<TextView>(Resource.Id.summary);
                callToAction = activity.FindViewById<Button>(Resource.Id.callToAction);
                secImage = activity.FindViewById<ImageView>(Resource.Id.secImage);
            }
        }

	}
}

