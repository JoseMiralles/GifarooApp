using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Webkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Android.Views.InputMethods;
using Android.Graphics;

using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using Com.Github.Hiteshsondhi88.Libffmpeg;
using Gifaroo.Android.Classes;

namespace Gifaroo.Android {

	[
		Activity (
			Label = "Gifaroo",
			MainLauncher = true,
			ConfigurationChanges=global::Android.Content.PM.ConfigChanges.Orientation,
			ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait
		)
		/*,
		IntentFilter (
			new[]{Intent.ActionView, Intent.ActionSend},
			Categories = new[] {Intent.CategoryDefault, Intent.CategoryBrowsable},
			Label = "Gifaroo",
			DataMimeType = "text/plain"
		)*/
	]
		
	public class MainActivity : Activity
	{

		public static String PACKAGE_NAME;
        private static TrackingInterface trackingInterface;

		//JS script that returns all <*/> src from a document.
		string javaScriptString =
			"(function(){ var GIFAROO_items = document.getElementsByTagName('img');" +
			"    var GIFAROO_Array = new Array();" +
			"    for (var i = 0; i < GIFAROO_items.length; i++) {" +
			"        if(GIFAROO_items[i].getAttribute('src') != null){" +
			"            GIFAROO_Array.push(GIFAROO_items[i].getAttribute('src'));" +
			"        }" +
			"    }" +
			"    var GIFAROO_jsonResult = JSON.stringify(GIFAROO_Array);" +
			"    return GIFAROO_jsonResult;" +
			"})();";
        string homePage = "file:///android_asset/new_home_page/index.html";

		//Import stuff
		WebView webView;
		ProgressBar progressBar;
		TextView playingWarning;
		Button cancelDownloadButton;
		//ActionBar
		SearchView URLBar = null;
		IMenuItem homeButton = null;

		JavaScriptResult myResult;
		InputMethodManager manager;
		TextView tapAndHoldPrompt;
		RelativeLayout relativeLayout;
        Flurry.Ads.FlurryAdBanner adBanner;
		string lastUrl;

		public Context appContext = null;
		public Context activityContext = null;

		//used to enale and disable actionbar items
		bool mState = true;

		//used to prevent the device form sleeping
		PowerManager pm = null;
		PowerManager.WakeLock wl = null;

		FFmpeg ffmpeg;

		//used to cancel the process that downloads the gif
		CancellationTokenSource downloadAndSaveGifCancelationToken;
		Task downloadTask;

		//Ads fields


		//TODO: clear the ram use from this activity.
		protected override void OnCreate (Bundle bundle)
        {
            //Attempts to load ffmpeg to see if it is supported.
            //StartActivity(new Intent(this, Java.Lang.Class.ForName("android.loadffmpeg")));

            

			//RequestWindowFeature (WindowFeatures.IndeterminateProgress);
			RequestWindowFeature (WindowFeatures.Progress);
			SetContentView (Resource.Layout.Main);
			base.OnCreate (bundle);
			//ActionBar.SetLogo (Resource.Drawable.inappicon);
			this.Title = string.Empty;
            trackingInterface = new TrackingInterface(this);
            //trackingInterface.TrackScreen(TrackingInterface.ActivityNames.MainActivity);
			
			manager = (InputMethodManager)GetSystemService (InputMethodService);

			//used to control sleeping
			pm = (PowerManager)this.GetSystemService (Service.PowerService);
			wl = pm.NewWakeLock (WakeLockFlags.ScreenDim, "My Tag");

			this.appContext = this.ApplicationContext;
			//this.activityContext = this.activityContext;

			//assign controllers/widgets
			webView = FindViewById<WebView> (Resource.Id.webView1);
			progressBar = FindViewById<ProgressBar> (Resource.Id.progressBar1);
			playingWarning = FindViewById<TextView> (Resource.Id.playingWarning);
			tapAndHoldPrompt = FindViewById<TextView> (Resource.Id.tapAndHoldPrompt);
			relativeLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			cancelDownloadButton = FindViewById<Button> (Resource.Id.cancelDownloadButton);

            //Flurry
            //Flurry.Analytics.FlurryAgent.Init(this, "ZTKSCWYBWHW9D6F28KY5");
            //adBanner = new Flurry.Ads.FlurryAdBanner(this, relativeLayout, "Main_activity_banner");
            //adBanner.FetchAndDisplayAd();

			relativeLayout.RemoveView (tapAndHoldPrompt);
			relativeLayout.RemoveView (playingWarning);
			relativeLayout.RemoveView (cancelDownloadButton);
			relativeLayout.RemoveView (progressBar);

			//used when the GetGifs button is pressed, contains ValueReceived event
            //myResult = new JavaScriptResult ();

            /*WEBVIEW: 	load default URL,
			 * 			enable JavaScript,
			 * 			prevent Android's default browser from opening	*/

			WebBrowserChromeClient myClient = new WebBrowserChromeClient (this);
			WebBrowserClient viewClient = new WebBrowserClient ();
			webView.SetWebChromeClient (myClient);
			webView.SetWebViewClient (viewClient);
			webView.ClearCache (true);
			webView.LoadUrl (homePage);
            webView.Settings.DomStorageEnabled = true;

			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.LoadWithOverviewMode = true;
			webView.Settings.UseWideViewPort = true;
            webView.Settings.BuiltInZoomControls = true;
			webView.SetBackgroundColor(Color.White);

            #region ffmpeg loading
            //load ffmpeg
            ffmpeg = FFmpeg.GetInstance(this);
            FffmpegTools.XLoadBinaryResponseHandler loadBinaryHandler = new FffmpegTools.XLoadBinaryResponseHandler();
            FffmpegTools.XExecuteBinaryResponseHandler executeBinaryHandler = new FffmpegTools.XExecuteBinaryResponseHandler();
            //new TaskFactory ().StartNew (()=>{
            //	ffmpeg.LoadBinary(loadBinaryHandler);
            //});
            loadBinaryHandler.OnFfmpegLoadindAttemptFinished += (object sender, EventArgs e) =>
            {
                if (loadBinaryHandler.failed)
                {

                }
            }; 
            #endregion

            viewClient.EOnPageStarted += delegate(object sender, EventArgs e){

                manager.HideSoftInputFromWindow(webView.WindowToken, 0);
				if (URLBar != null){
                    if (webView.Url != homePage)
                    {
                        URLBar.SetQuery(webView.Url, false);
                    }
                     else if (webView.Url == homePage)
                    {
                         URLBar.SetQuery("", false);
                    }
                }
			};

			//Fired when the download button is pressed and the JavaScript interface got the value
			if (myResult != null)
				myResult.ValueReceived += delegate(object sender, EventArgs e) {

				List<string> gifUrlList =
					GifarooTools.GetAllGifUrls(
						myResult.ResponseString, webView.OriginalUrl);

				//Navigate to GifGallery with GifUrls as data
				if (gifUrlList.Count != 0) {
					var gifGallery = new Intent (this, typeof(GifGalleryActivity));
					gifGallery.PutStringArrayListExtra("gifUrlsList", gifUrlList);
					StartActivity(gifGallery);
				}
				else
					Toast.MakeText (this, "No animated gifs found... (Not all animated images are gif files...)", ToastLength.Short).Show();
			};

			//Long click will go directly to the EditGif activity
			Toast notAGifToast = Toast.MakeText(this, "That's not a supported animated image.", ToastLength.Short);
			webView.LongClick +=  (object sender, View.LongClickEventArgs e) => {

				//URLBar.ClearFocus();
				webView.StopLoading();	//TODO: break point this to allow asus to reach the next activity
				WebView eventWebView = (WebView)sender;
				var myResultza = eventWebView.GetHitTestResult();
				var myResultzaType = myResultza.Type;
				var extra = myResultza.Extra;
				bool formatsupported = false;

                bool isHomepageGif = true;

#if !DEBUG      
                isHomepageGif = (webView.Url != homePage);  //This only runs on release, prevents the hoemsreen gif from being longclicked.
#endif

                foreach (string extension in GifarooTools.supportedImageExtensions) {
                    if (isHomepageGif
                        && extra != null
						&& extra.Contains(extension)){

						downloadTask = null;
						formatsupported = true;

						downloadAndSaveGifCancelationToken = new CancellationTokenSource();

						try {
							toggleLoadingControlls(false);
							string fileName = "my_file" + extension;
                            //string newFileName = "my_file.mp4";
							//string dimensionsString = FffmpegTools.GetResizeCommandDimensions(this, fileName);
							//string ffmpegCommand = "-y -i "+FilesDir.Path+"/"+fileName+" -vf scale="+ dimensionsString +" -preset superfast "+FilesDir.Path+"/"+newFileName;
							//string ffmpegCommand = "-y -i "+FilesDir.Path+"/"+fileName+" -c:v libx264 -preset superfast -crf 22 -c:a copy " +FilesDir.Path+"/"+newFileName;
							//string ffmpegCommand = "-y -i "+FilesDir.Path+"/"+fileName+" -c:v libx264 -vprofile baseline -c:a libfaac -ar 44100 -ac 2 -b:a 128k -movflags faststart "+FilesDir.Path+"/"+newFileName;
                            //string ffmpegCommand = "-y -i " + FilesDir.Path + "/" + fileName + " -c:v libx264 -profile:v baseline -c:a libfaac -ar 44100 -ac 2 -b:a 128k -movflags faststart " + FilesDir.Path + "/" + newFileName;
                            //string ffmpegCommand = "-y -i " + FilesDir.Path + "/" + fileName + " -s 480x320 -vcodec mpeg4 -acodec aac -strict -2 -ac 1 -ar 16000 -r 13 -ab 32000 -aspect 3:2 " + FilesDir.Path + "/" + newFileName;

                            downloadTask = Task.Factory.StartNew(()=>{
								try{
									GifarooTools.DownloadFileFromURL(this,extra,fileName);
								}catch(Exception se){
                                    trackingInterface.TrackException(se.Message, false);
                                }

                                //ffmpeg.Execute(ffmpegCommand.Split(), executeBinaryHandler);

							}, downloadAndSaveGifCancelationToken.Token);
							downloadTask.ContinueWith((antecedent)=> {

                                var editGifActivity = new Intent(this, typeof(EditGifActivity));
                                //var editGifActivity = new Intent (this, typeof(SaveActivity)); //goes to java version of save activity
                                List<string> myStringList = new List<string>();
                                myStringList.Add(fileName);//TODO: change to newFilename when ffmpeg is implemented
                                editGifActivity.PutStringArrayListExtra("gifUrl", myStringList);
                                trackingInterface.TrackFileSorceDomain(new Java.Net.URL(extra).Host);

                                StartActivity (editGifActivity); //TODO: remove this line when ffmpeg converison is implemented correctly

                                //executeBinaryHandler.OnExecutionFinished += (object exeSender, EventArgs eexe) =>
                                //{
                                //    if (executeBinaryHandler.failed == false)
                                //        StartActivity(editGifActivity);
                                //};

							}, downloadAndSaveGifCancelationToken.Token);
						}
						catch (Exception es) {
							Toast.MakeText(this,"Something went wrong, try again.", ToastLength.Long).Show();
							toggleLoadingControlls(true);
							downloadTask = null;
                            trackingInterface.TrackException(es.Message, false);
						}

						cancelDownloadButton.Click += (object cancelSender, EventArgs cancelEA) => {
							downloadAndSaveGifCancelationToken.Cancel();
							toggleLoadingControlls(true);
							SetProgressBarIndeterminateVisibility(false);
							downloadTask = null;
						};
					}

				}
				if (formatsupported == false){
					SetProgressBarIndeterminateVisibility(false);
					notAGifToast.Show();
				}
				
     			};

		}

        private bool LoadFfmpeg(Context context) {
            return false;
        }

		/// <summary>
		/// Toggles the loading controlls and sleep locks.
		/// </summary>
		/// <param name="enable">If set to <c>true</c> enable.</param>
		public void toggleLoadingControlls(bool enable){
			if (enable == false) {
				if(playingWarning.Parent == null)
					relativeLayout.AddView(playingWarning);
				relativeLayout.AddView (cancelDownloadButton);
				relativeLayout.AddView (progressBar);
				mState = false;
				try { wl.Acquire (); } catch (Exception e) {
                    trackingInterface.TrackException(e.Message, false);
                }
			} else if (enable == true) {
				if(playingWarning.Parent != null)
					relativeLayout.RemoveView(playingWarning);
				relativeLayout.RemoveView (cancelDownloadButton);
				relativeLayout.RemoveView (progressBar);
				mState = true;
				try { wl.Release (); } catch (Exception ex) {
                    trackingInterface.TrackException(ex.Message, false);
                }
			}
			InvalidateOptionsMenu ();
		}

		//launched when the activity resumes
		protected override void OnPause(){
			toggleLoadingControlls (true);
			SetProgressBarIndeterminateVisibility(false);
			base.OnPause ();
		}

		/// <summary>
		/// Used to call WebView.EvaluateJavascript, contains the JSON response in "ResponseString".
		/// </summary>
		class JavaScriptResult : Java.Lang.Object, IValueCallback{

			/// <summary>
			/// Called when OnReceiveValue is finished.
			/// </summary>
			public event EventHandler ValueReceived;
			public Java.Lang.String JavaResponseString;
			public String ResponseString;

			public void OnReceiveValue(Java.Lang.Object result){
				JavaResponseString = result as Java.Lang.String;
				ResponseString = JavaResponseString.ToString();

				if (ValueReceived != null)
					ValueReceived (this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Extending the WebViewClient class so that I can prevent the OS from opening the device's default browser.
		/// And to add the onPageFinished event.
		/// </summary>
		public class WebBrowserClient : WebViewClient{
			public override bool ShouldOverrideUrlLoading (WebView view, string url){
				view.LoadUrl (url);
				return true;
			}

			public event EventHandler EOnPageFinished;
			public override void OnPageFinished (WebView view, string url){
				base.OnPageFinished (view, url);
				if (EOnPageFinished != null)
					EOnPageFinished (this, EventArgs.Empty);
			}

			public event EventHandler EOnPageStarted;
			public override void OnPageStarted (WebView view, string url, Bitmap favicon){
				base.OnPageStarted (view, url, null);
				if (EOnPageStarted != null)
					EOnPageStarted (this, EventArgs.Empty);
			}

		}

		public class WebBrowserChromeClient: WebChromeClient{

			Activity _activityContext;

			public WebBrowserChromeClient(Activity activityContext){
				this._activityContext = activityContext;
			}
			public override void OnProgressChanged (WebView view, int newProgress){
				_activityContext.SetProgress (newProgress * 100);
				Console.WriteLine ("Current progress:\t");
			}

		}

		public override void OnBackPressed ()
		{
			//cancel the task if it is running.
			if (downloadTask != null
				&& downloadTask.Status == TaskStatus.Running) {
				downloadAndSaveGifCancelationToken.Cancel ();
				toggleLoadingControlls(true);
				SetProgressBarIndeterminateVisibility(false);
				downloadTask = null;
				return;
			}

			if (webView.CanGoBack ()) {
				webView.GoBack ();
			}
			else
				base.OnBackPressed ();
		}

		protected override void OnStop ()
		{
			//TODO: clear contents or free up ram
			base.OnStop ();
		}

		public override bool OnCreateOptionsMenu(IMenu menu){
			MenuInflater inflater = this.MenuInflater;
			inflater.Inflate(Resource.Menu.menu, menu);

			var URLBarUncasted = (SearchView)menu.FindItem (Resource.Id.URLBar).ActionView;
			URLBar = URLBarUncasted.JavaCast<SearchView> ();
			//URLBar.LayoutParameters = new LinearLayout.LayoutParams (SearchView.LayoutParams.MatchParent, SearchView.LayoutParams.WrapContent);
			URLBar.SetQueryHint ("Search Google or enter a URL");
			//downloadButton = (IMenuItem)menu.FindItem (Resource.Id.downloadButton).ActionView;

			homeButton = (IMenuItem)menu.FindItem (Resource.Id.homePageButton).ActionView;

			URLBar.QueryTextFocusChange += delegate {
				if(!URLBar.IsFocused){
					manager.HideSoftInputFromWindow(URLBar.WindowToken, 0);
					if (URLBar.Query == "" && webView.Url != homePage)
						URLBar.SetQuery(webView.Url,false);
				}
				else if (URLBar.IsFocused
					&& webView.Url != null
					&& webView.Url == homePage){
					URLBar.SetQuery("", false);
				}
			};

			URLBar.QueryTextSubmit += delegate {
				manager.HideSoftInputFromWindow(URLBar.WindowToken, 0);
				string urlModed = GifarooTools.getURL(URLBar.Query);
				webView.StopLoading();

				URLBar.SetQuery(urlModed,false);
				webView.LoadUrl (urlModed);
				//webView.RequestFocus();
			};

			if (mState == false)
				URLBar.Visibility = ViewStates.Gone;
			else
				URLBar.Visibility = ViewStates.Visible;
			for (int i = 0; i < menu.Size (); i++)
				menu.GetItem (i).SetVisible (mState);

			//Get rid of the unecessary views
			menu.FindItem (Resource.Id.editButton).SetVisible(false);
			menu.FindItem (Resource.Id.saveButton).SetVisible(false);
			menu.FindItem (Resource.Id.downloadButton).SetVisible(false);
			menu.FindItem (Resource.Id.storeButton).SetVisible (false);

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item){
			switch (item.ItemId) {

			case Android.Resource.Id.downloadButton:
				//webView.EvaluateJavascript (javaScriptString, myResult);
				return true;

			case Android.Resource.Id.homePageButton:
				try{
					webView.LoadUrl (homePage);
				} catch(Exception){}
				return true;

			//Other cases go here
			}
			return base.OnOptionsItemSelected(item);
		}

	}
}

