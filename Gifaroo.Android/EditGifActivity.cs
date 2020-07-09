
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

using Android.Webkit;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Android.Graphics;
using System.Net;
using Android.Media;
using ListViewLazyLoadingImages;
using Com.Androidquery;
using Com.Androidquery.Callback;

using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;

using Flurry;
using Gifaroo.Android.Classes;

using Editor;

namespace Gifaroo.Android
{
	[Activity (Label = "",
		ConfigurationChanges=global::Android.Content.PM.ConfigChanges.Orientation,
		ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]

	public class EditGifActivity : Activity
	{
		Context appContext;
		Activity activity;
		LinearLayout mainLayout;
		LinearLayout contentDisplayInside;
		RelativeLayout contentDisplay;
		WebView gifDisplay;
		VideoView videoDisplay;
		int gifDisplayDimension;
		InputMethodManager manager;
		string gifFileName;
		WebBrowserClient gifDisplayClient;
		IMenuItem saveButton;
		IMenuItem storeButton;
		RelativeLayout topRelativeLayout = null;
		ActionBar.Tab[] tabsArray = null;
		string htmlDataForWebView = "";
        private TrackingInterface trackingInterface;
        int actionBarHeight = 1;

		#region Premium interface fields
		private InAppBillingServiceConnection _serviceConnection = null;
		private RelativeLayout StoreWrapper = null;
		private LinearLayout storeLayout = null;
		private WebView[] storeWebViews = null;
		private PremiumInterface _premiumInterface = null;
		private Button purchaseFontsPackButton;
		private Button purchaseBlursPatternsPackButton;
		private Button purchaseGradientsNightPackButton;
		private Button freeUnlockBlursPatternsButton;
		private Button freeUnlockNightGradientButton;
		private TextView textViewFontsPackDescription;
		private TextView textViewBlursPatternsDescription;
		private TextView textViewgradientsNightDescription;
        ISharedPreferences sharedPreferencesPremium;

        Flurry.Ads.FlurryAdInterstitial rewardedStoreAd = null;
		#endregion

		#region Background interface Fields
		LinearLayout backgroundLayout;
		LinearLayout backgroundSetsLinearLayout;
		HorizontalScrollView backgroundSetsScrollView;
		GridView backgroundImagesGrid;
		ImageView backgroundDisplay;
		ImageButton bgColorsSetButton;
		ImageButton bgNightSetButton;
		ImageButton bgPatternsSetButton;
		ImageButton bgBlurSetButton;
		ImageButton bgGradientsSetButton;
		ImageButton bgGifarooSetButton;
		TextView rightSideBarProgressBar;
		String selectedBackground;
		private BGSetNames selectedBackgroundSet;
		private ImageButton[] imageButtons = null;
		private LinearLayout.LayoutParams BGSetNormalLayoutParams = null;
        private FontArrayAdapter fontsAdapter;
        LazyLoadAdapter myAdapter = null;

		enum BGSetNames{
			Night,
			Gradients,
			Colors,
			Gifaroo,
			Patterns,
			Blurs
		};

		/*The colors array is in the "text interface fields" region below this one.
		This is because it is used for text coloring too.*/
		int[] thumbIds_Night = {
			Resource.Drawable.bg_shapes_1,
			Resource.Drawable.bg_shapes_2,
			Resource.Drawable.bg_shapes_3,
			Resource.Drawable.bg_shapes_4,
			Resource.Drawable.bg_shapes_5,
			Resource.Drawable.bg_shapes_6,
			Resource.Drawable.bg_shapes_7,
			Resource.Drawable.bg_shapes_8,
			Resource.Drawable.bg_shapes_9,
			Resource.Drawable.bg_shapes_10,
			Resource.Drawable.bg_shapes_11,
			Resource.Drawable.bg_shapes_12,
			Resource.Drawable.bg_shapes_13,
			Resource.Drawable.bg_shapes_14,
			Resource.Drawable.bg_shapes_15,
			Resource.Drawable.bg_shapes_16,
			Resource.Drawable.bg_shapes_17,
			Resource.Drawable.bg_shapes_18,
			Resource.Drawable.bg_shapes_19,
			Resource.Drawable.bg_shapes_20,
			Resource.Drawable.bg_shapes_21,
			Resource.Drawable.bg_shapes_22,
			Resource.Drawable.bg_shapes_23,
			Resource.Drawable.bg_shapes_24
		};
		int[] thumbIds_Blurs = {
			Resource.Drawable.bg_blur_1,
			Resource.Drawable.bg_blur_2,
			Resource.Drawable.bg_blur_3,
			Resource.Drawable.bg_blur_4,
			Resource.Drawable.bg_blur_5,
			Resource.Drawable.bg_blur_6,
			Resource.Drawable.bg_blur_7,
			Resource.Drawable.bg_blur_8,
			Resource.Drawable.bg_blur_9,
			Resource.Drawable.bg_blur_10,
			Resource.Drawable.bg_blur_11,
			Resource.Drawable.bg_blur_12,
			Resource.Drawable.bg_blur_13,
			Resource.Drawable.bg_blur_14,
			Resource.Drawable.bg_blur_15,
			Resource.Drawable.bg_blur_16,
			Resource.Drawable.bg_blur_17
		};
		int[] thumbIds_Gradients = {
			Resource.Drawable.bg_Gradient_1,
			Resource.Drawable.bg_Gradient_2,
			Resource.Drawable.bg_Gradient_3,
			Resource.Drawable.bg_Gradient_4,
			Resource.Drawable.bg_Gradient_5,
			Resource.Drawable.bg_Gradient_6,
			Resource.Drawable.bg_Gradient_7,
			Resource.Drawable.bg_Gradient_8,
			Resource.Drawable.bg_Gradient_9,
			Resource.Drawable.bg_Gradient_10,
			Resource.Drawable.bg_Gradient_11,
			Resource.Drawable.bg_Gradient_12,
			Resource.Drawable.bg_Gradient_13,
			Resource.Drawable.bg_Gradient_14,
			Resource.Drawable.bg_Gradient_15,
			Resource.Drawable.bg_Gradient_16,
			Resource.Drawable.bg_Gradient_17,
			Resource.Drawable.bg_Gradient_18,
			Resource.Drawable.bg_Gradient_19,
			Resource.Drawable.bg_Gradient_20,
			Resource.Drawable.bg_Gradient_21
		};
		int[] thumbIds_Patterns = {
			Resource.Drawable.bg_pattern_1,
			Resource.Drawable.bg_pattern_2,
			Resource.Drawable.bg_pattern_3,
			Resource.Drawable.bg_pattern_4,
			Resource.Drawable.bg_pattern_5,
			Resource.Drawable.bg_pattern_6,
			Resource.Drawable.bg_pattern_7,
			Resource.Drawable.bg_pattern_8,
			Resource.Drawable.bg_pattern_9,
			Resource.Drawable.bg_pattern_10,
			Resource.Drawable.bg_pattern_11,
			Resource.Drawable.bg_pattern_12,
			Resource.Drawable.bg_pattern_13,
			Resource.Drawable.bg_pattern_14,
			Resource.Drawable.bg_pattern_15,
			Resource.Drawable.bg_pattern_16,
			Resource.Drawable.bg_pattern_17,
			Resource.Drawable.bg_pattern_18,
			Resource.Drawable.bg_pattern_19,
			Resource.Drawable.bg_pattern_20,
			Resource.Drawable.bg_pattern_21,
			Resource.Drawable.bg_pattern_22,
			Resource.Drawable.bg_pattern_23,
			Resource.Drawable.bg_pattern_24,
			Resource.Drawable.bg_pattern_25,
		};

		int[] thumbIds_gifaroo = {
			Resource.Drawable.bg_gifaroo_1,
			Resource.Drawable.bg_gifaroo_2,
			Resource.Drawable.bg_gifaroo_3,
			Resource.Drawable.bg_gifaroo_4,
			Resource.Drawable.bg_gifaroo_5,
			Resource.Drawable.bg_gifaroo_6,
			Resource.Drawable.bg_gifaroo_7,
			Resource.Drawable.bg_gifaroo_8,
			Resource.Drawable.bg_gifaroo_9,
			Resource.Drawable.bg_gifaroo_10,
			Resource.Drawable.bg_gifaroo_11,
			Resource.Drawable.bg_gifaroo_12,
			Resource.Drawable.bg_gifaroo_13,
			Resource.Drawable.bg_gifaroo_14,
			Resource.Drawable.bg_gifaroo_15,
			Resource.Drawable.bg_gifaroo_16,
			Resource.Drawable.bg_gifaroo_17,
			Resource.Drawable.bg_gifaroo_18,
			Resource.Drawable.bg_gifaroo_19,
			Resource.Drawable.bg_gifaroo_20,
			Resource.Drawable.bg_gifaroo_21,
		};

		#endregion
		#region Text interface fields
		LinearLayout textLayout;
		GridView textColorGrid;
		Spinner fontSpinner;
		Spinner sizeSpinner;
        TextStickerEditor textEditor;
		Button newTextButton;
		EditText textInput;
		LinearLayout linearLayout2;
		LinearLayout linearLayout1;
		TextView contentDisplayLoadingTextView;
		LinearLayout TextControlls1;
		private int _lastFontPositionSelected = 1;
		string[] colorHexCodes = {
			//black, white, grays
			"#5E5E5E","#333333","#FFFFFF","#8C8C8C","#C2C2C2","#000000",
			//Pastels
			"#FF7575", "#FF75AC", "#FF75CF",
			"#FF75FD", "#D875FF", "#A375FF",
			"#757AFF", "#75AAFF", "#75D8FF",
			"#75FFD8", "#75FF95", "#A8FF75",
			"#DDFF75", "#FFDD75", "#FFAF75",
			//dark solids
			"#750000", "#75002B", "#750068", "#000275",
			"#003375","#006675","#007531","#397500",
			"#756800","#753D00","#751900",
			//solids
			"#FF0000", "#FF0073", "#FF00AE",
			"#AA00FF", "#3700FF", "#004CFF",
			"#0099FF", "#00E5FF", "#00FFC3",
			"#00FF5E", "#00FF15", "#55FF00",
			"#CCFF00", "#FFCC00", "#FF8400",
		};
		string[] fontsArray={
			"Amatic",
			"Anton",
			"Bangers",
			"Chewy",
			//"CinzelDecorative",
			"CourgetteRegular",
			//"CraftyGirls",
			//"GloriaHallelujah",
			"OpenSans",
			//premium fonts start here
			"Righteous",
			"SixCaps",
			"FrederickatheGreat",
			"LeckerliOne",
			"IndieFlower",
			"KaushanScript",
			"Lobster",
			"Megrim",
			"Monoton",
			//"Orbitron",
			"Pacifico",
			"PoiretOne",
			//"Sacramento"
		};
		#endregion

		protected override void OnCreate (Bundle bundle) {
			appContext = this.BaseContext; 
			RequestWindowFeature (WindowFeatures.IndeterminateProgress);
			base.OnCreate (bundle);
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
			SetContentView (Resource.Layout.EditGif);
			ActionBar.SetBackgroundDrawable (Resources.GetDrawable( Resource.Drawable.GifarooActionbarBackgroundflatTop) );
			manager = (InputMethodManager)GetSystemService (InputMethodService);
            trackingInterface = new TrackingInterface(this);

            global::Android.Content.Res.TypedArray styledAttr = this.appContext.Theme.ObtainStyledAttributes
                (
                new int[] { global::Android.Resource.Attribute.ActionBarSize}
                );
            actionBarHeight = (int)styledAttr.GetDimension(0,0);
            styledAttr.Recycle();

            //trackingInterface.TrackScreen(TrackingInterface.ActivityNames.EditGif);

			#region Field Assigments
			//in app billing and store stuff
			topRelativeLayout = FindViewById<RelativeLayout> (Resource.Id.topRelativeLayout);
			StoreWrapper = FindViewById<RelativeLayout>(Resource.Id.StoreWrapper);
			storeLayout = FindViewById<LinearLayout>(Resource.Id.storeLayout);
			purchaseFontsPackButton = FindViewById<Button>(Resource.Id.purchaseFontsButton);
			purchaseBlursPatternsPackButton = FindViewById<Button>(Resource.Id.purchaseBlursPatternsButton);
			purchaseGradientsNightPackButton = FindViewById<Button>(Resource.Id.purchaseNightGradientButton);
			freeUnlockBlursPatternsButton = FindViewById<Button>(Resource.Id.freeUnlockBlursPatternsButton);
			freeUnlockNightGradientButton = FindViewById<Button>(Resource.Id.freeUnlockNightGradientButton);
			textViewFontsPackDescription = FindViewById<TextView> (Resource.Id.textViewFontsPackDescription);
			textViewBlursPatternsDescription = FindViewById<TextView> (Resource.Id.textViewBlursPatternsDescription);
			textViewgradientsNightDescription = FindViewById<TextView> (Resource.Id.textViewgradientsNightDescription);
            this.sharedPreferencesPremium = GetSharedPreferences("Premium", FileCreationMode.Private);
			RemoveStore();

			//Backgrounds Stuff
			backgroundLayout = FindViewById<LinearLayout> (Resource.Id.backgroundLayout);
			backgroundSetsLinearLayout = FindViewById<LinearLayout> (Resource.Id.backgroundSetsLinearLayout);
			backgroundSetsScrollView = FindViewById<HorizontalScrollView> (Resource.Id.backgroundSetsScrollView);
			backgroundImagesGrid = FindViewById<GridView> (Resource.Id.backgroundImagesGrid);
			backgroundDisplay = FindViewById<ImageView> (Resource.Id.backgroundDisplay);
			bgColorsSetButton = FindViewById<ImageButton> (Resource.Id.bgColorsSetButton);
			bgNightSetButton = FindViewById<ImageButton> (Resource.Id.bgNightSetButton);
			bgPatternsSetButton = FindViewById<ImageButton> (Resource.Id.bgPatternsSetButton);
			bgBlurSetButton = FindViewById<ImageButton> (Resource.Id.bgBlurSetButton);
			bgGradientsSetButton = FindViewById<ImageButton> (Resource.Id.bgGradientsSetButton);
			bgGifarooSetButton = FindViewById<ImageButton> (Resource.Id.bgGifarooSetButton);
			rightSideBarProgressBar = FindViewById<TextView> (Resource.Id.rightSideBarProgressBar);
			BGSetNormalLayoutParams = (LinearLayout.LayoutParams)bgColorsSetButton.LayoutParameters;
			imageButtons = new ImageButton[]{ //used to deselect teh buttons when necessary.
				bgColorsSetButton,
				bgNightSetButton,
				bgPatternsSetButton,
				bgBlurSetButton,
				bgGradientsSetButton,
				bgGifarooSetButton,
			};

			//Text stuff
			textLayout = FindViewById<LinearLayout> (Resource.Id.textLayout);
			textColorGrid = FindViewById<GridView> (Resource.Id.textColorGrid);
			fontSpinner = FindViewById<Spinner> (Resource.Id.fontSpinner);
			sizeSpinner = FindViewById<Spinner> (Resource.Id.sizeSpinner);
			textInput = FindViewById<EditText> (Resource.Id.editText1);
			newTextButton = FindViewById<Button>(Resource.Id.newTextButton);
			//deleteTextButton = FindViewById<ImageButton>(Resource.Id.deleteTextButton);
			linearLayout2 = FindViewById<LinearLayout> (Resource.Id.linearLayout2);
			linearLayout1 = FindViewById<LinearLayout> (Resource.Id.linearLayout1);
			contentDisplayLoadingTextView = FindViewById<TextView>(Resource.Id.contentDisplayLoadingTextView);
			TextControlls1 = FindViewById<LinearLayout>(Resource.Id.TextControlls1);

			//Displays
			contentDisplayInside = FindViewById<LinearLayout> (Resource.Id.ContentDisplayinside);
			contentDisplay = FindViewById<RelativeLayout> (Resource.Id.contentDisplay);
			mainLayout = FindViewById<LinearLayout> (Resource.Id.mainEditorLayout);
			videoDisplay = FindViewById<VideoView> (Resource.Id.videoDisplay);
			gifDisplay = FindViewById<WebView> (Resource.Id.gifDisplay);
			gifDisplayClient = new WebBrowserClient ();

			gifDisplay.ClearCache(true);

			//from main activity
			try {
				IList<String> _passedData = Intent.GetStringArrayListExtra ("gifUrl");
				gifFileName = _passedData.FirstOrDefault ();
			} catch (Exception ex) {}

			gifDisplayDimension = (int)(Resources.DisplayMetrics.HeightPixels / 2);
			LinearLayout.LayoutParams GDparams = new LinearLayout.LayoutParams (mainLayout.LayoutParameters);

			//Parameters for content display
			GDparams.Height = gifDisplayDimension;
			GDparams.Width = gifDisplayDimension;
			GDparams.Gravity = GravityFlags.Center;

			contentDisplay.LayoutParameters = (GDparams);

			int residue = Resources.DisplayMetrics.WidthPixels - gifDisplayDimension;
			if (false){
				GDparams.Gravity = GravityFlags.Right;
				GDparams.RightMargin = GifarooTools.dpsToPixels(50, Resources);
				GDparams.LeftMargin = GifarooTools.dpsToPixels(10, Resources);
				contentDisplay.LayoutParameters = (GDparams);
			}
            #endregion

			#region In app billing and premium content

            /*
            string obskey = Security.Unify(
                new string[] {
				"9qHS95Qc+e49PW+Xo3LcS6h6eu3esQv+o3iKfRPEKbPtJqG0PCy+npemGgEi4ZyqOO3DbWSqKZ2jPAWitK6WmMMNHXOqj/Nac/+fnJiG1ErlKyZlTKaYhKHa+wPTCH",
				"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjm6E4c7x1va5ug5cGp8yUNqh1VZIWrmaE2cunnHBuRje3UrpcZvie",
				"9ZQeLJEWx9WcJxL6iisEsdwvvMyWkxjnHwUrwIDAQAB",
				"95jn+m/tTzjppbg6T2EEgvqYR7WC9WxWxJ6qB5f5SBgIhclAJIsVVsUiZ7or5GLdydmN4M4NatbVNZe/89Gt5neBxO+O2G5WRCU8M4uZbz/FiNtiKplgvf8dEBpAk6"
			},
            new int[] {2,0,3,1}
            );
            */

            _serviceConnection = new InAppBillingServiceConnection(this, PremiumInterface.kewefawe);
            Flurry.Analytics.FlurryAgent.Init(this, "ZTKSCWYBWHW9D6F28KY5");
            rewardedStoreAd = new Flurry.Ads.FlurryAdInterstitial(this, "rewarded-store");
            rewardedStoreAd.FetchAd();

            //READ!!!!!!!! -> THIS WONT WORK ON DEVICES LOGGED IN TO THE MAIN GOOGLE ACCOUNT unless it is with the "Reservedproductids.purchased"
			_premiumInterface = new PremiumInterface(this, _serviceConnection);
			_serviceConnection.OnConnected += () => {

				#region Billing Listeners
                _serviceConnection.BillingHandler.QueryInventoryError += (int responseCode, Bundle skuDetails) => {
                    Console.WriteLine("[BILLING]: Error getting inventory");
                    var tt = skuDetails;
                    var ss = responseCode;
                };
				_serviceConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) => {
					Console.WriteLine("[BILLING]: Error getting products");
				};

				_serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) => {
					Console.WriteLine("[BILLING]: Invalid owned items bundle returned");
				};

				_serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) => {
					Console.WriteLine("[BILLING]: Error purchasing item {0}",sku);
				};

				_serviceConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) => {
					Console.WriteLine("[BILLING]: Error consuming previous purchase");
				};

				_serviceConnection.BillingHandler.InAppBillingProcesingError += (message) => {
					Console.WriteLine("[BILLING]: In app billing processing error {0}",message);
				};

				_serviceConnection.BillingHandler.OnProductPurchased += (int response, Purchase purchase, string purchaseData, string purchaseSignature) => {
					//sync all purchases
					_premiumInterface.SyncPurchases();
                    bool isBackgroundPack = true;

					//remove backgroundsgrid so that the user has to reopen it
					if(backgroundImagesGrid.Parent != null) {
                        backgroundImagesGrid.Visibility = ViewStates.Gone;
					}
					if (purchase.ProductId == PremiumInterface.SKUs.Fonts_Pack_SKU){
                        isBackgroundPack = false;
						var newfontSizes = new List<string> (){"Small", "Medium", "Large", "Largest"};
						var newAdapter = new ArrayAdapter<string> (this, global::Android.Resource.Layout.SimpleSpinnerDropDownItem, newfontSizes);
						sizeSpinner.Adapter = newAdapter;
					}
					RemoveStore();

                    //report to purchase analytics
                    trackingInterface.TrackItemPurchase(
                        purchase.ProductId,
                        (isBackgroundPack) ? "Background Pack" : "Fonts Pack",
                        _premiumInterface.ProductsDictionary[purchase.ProductId].itemName,
                        Convert.ToDouble(_premiumInterface.ProductsDictionary[purchase.ProductId].product.Price));
				};
				#endregion

				//get available products
				//this updates the _billingInventroy.ProductsDictionary to tell us wich products are owned by the user.
				_premiumInterface.SyncInventory();

				//load purchased items
				//fill _products with all the products so that the user can then purchase them.
				_premiumInterface.SyncPurchases();

				updateWidgetsAfterConnection();
			};
            _serviceConnection.OnInAppBillingError += (InAppBillingErrorType error, string message) => 
            {
                //TODO: error;
                string mmmm = message;
                bool ruined = true;
            };

			_serviceConnection.Connect();

			SetStoreListenersAndAddReferenceToWidgets();

			//_serviceConnection.Disconnect();

			#endregion

			#region Action bar Tabs
			//Background tab
			ActionBar.Tab backgroundTab = ActionBar.NewTab ();
			//backgroundTab.SetText (Resources.GetString (Resource.String.backgroundTab));
			backgroundTab.SetIcon (Resource.Drawable.ic_format_color_fill_white_24dp);
			backgroundTab.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
				backgroundLayout.Visibility = ViewStates.Visible;
				backgroundSetsScrollView.Visibility = ViewStates.Visible;
			};
			backgroundTab.TabUnselected += (object sender, ActionBar.TabEventArgs e) => {
				DeselectBackgroundButtons();
				backgroundLayout.Visibility = ViewStates.Gone;
				backgroundImagesGrid.Visibility = ViewStates.Gone;
			};
			ActionBar.AddTab (backgroundTab);

			//Text tab
			ActionBar.Tab textTab = ActionBar.NewTab ();
			//textTab.SetText (Resources.GetString (Resource.String.textTab));
			textTab.SetIcon (Resource.Drawable.ic_text_fields_white_24dp);
			textTab.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
				textLayout.Visibility = ViewStates.Visible;
				if (textEditor.stickerTextViewList.Count > 0){
					textColorGrid.Visibility = ViewStates.Visible;
				}
			};
			textTab.TabUnselected += (object sender, ActionBar.TabEventArgs e) => {
				try{textEditor.textViewToEdit.HideControls();}
				catch(Exception){}
				textLayout.Visibility = ViewStates.Gone;
				textColorGrid.Visibility = ViewStates.Gone;
			};
			ActionBar.AddTab (textTab);

			//Watermark Tab
			ActionBar.Tab watermarkTab = ActionBar.NewTab ();
			//watermarkTab.SetText (Resources.GetString (Resource.String.watermarkTab));
			//watermarkTab.SetIcon (Resource.Drawable.signature);
			watermarkTab.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
			};
			watermarkTab.TabUnselected += (object sender, ActionBar.TabEventArgs e) => {
			};
			//ActionBar.AddTab (watermarkTab);

			//Effects tab
			ActionBar.Tab effectsTab = ActionBar.NewTab ();
			//effectsTab.SetText ("EFFECTS");
			effectsTab.SetIcon (Resource.Drawable.effects);
			effectsTab.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
			};
			effectsTab.TabUnselected += (object sender, ActionBar.TabEventArgs e) => {
			};
			//ActionBar.AddTab (effectsTab);

			tabsArray = new ActionBar.Tab[]{
				backgroundTab,
				textTab
			};

			#endregion

			#region gif/video Display
			//"img" if image and "video" if video
			string contentType = getContentType(gifFileName);

			//display gifs, jpg, png...
			if (contentType == "img") {
				contentDisplay.RemoveView(videoDisplay);
				gifDisplay.SetWebViewClient(gifDisplayClient);
				gifDisplay.Settings.JavaScriptEnabled = true;
				gifDisplay.SetBackgroundColor (Color.Transparent);
				gifDisplay.SetInitialScale(0);
				gifDisplay.SetOnTouchListener(
					new DisableTouchListener()
				);
				//getDimensions for webView display
				bool checkfile = new Java.IO.File(FilesDir.AbsolutePath + "/" + gifFileName).Exists();
				Bitmap gifBitmap = GetImageBitmapFromUrl (FilesDir.AbsolutePath + "/" + gifFileName);

                int imageHeight = 0;
                int imageWidth = 0;

                try
                {
                    imageHeight = gifBitmap.Height;
                    imageWidth = gifBitmap.Width;
                }
                catch (Exception) {
                    Toast.MakeText(this, "Something went wrong, this is probably not a supported file.", ToastLength.Long);
                    Finish();
                }

                string heightData = "<html><head><style type='text/css'>body{ margin:auto auto;text-align:center;} img{height:100%; } </style></head>" +
					"<body><img src='"+ gifFileName +"'/></body></html>";
				string widthData = "<html><head><style type='text/css'> " +
					"body{ width:100%; background-image: url('"+ gifFileName +"'); background-repeat: no-repeat;  background-position: center; background-size: 100%; } </style></head><body></body></html>";
				string widthData2 = "<html><head><style type='text/css'>body{ margin:auto auto;text-align:center;} img{width:100%; } </style></head>" +
					"<body><img src='"+ gifFileName +"'/></body></html>";
				htmlDataForWebView = imageHeight > imageWidth ? heightData : widthData;

				gifDisplayClient.EOnPageStarted += (object sender, EventArgs e) => {
					loadingContentDisplay(true);
				};
				gifDisplayClient.EOnPageFinished += (object sender, EventArgs e) => {
					loadingContentDisplay(false);
				};

				//Add/load stuff to main display
				gifDisplay.LoadDataWithBaseURL ("file://"+FilesDir.AbsolutePath+"/" ,htmlDataForWebView, "text/html", "utf-8", null);

			}
			//display mp4, avi...
			else if (contentType == "video"){
				contentDisplay.RemoveView(gifDisplay);
				string videoPath = FilesDir.Path + Java.IO.File.Separator + gifFileName;
				videoDisplay.SetVideoPath(videoPath);
				videoDisplay.SetBackgroundColor(Color.Transparent);
				videoDisplay.SetOnPreparedListener(new VideoPreparedListener());
				videoDisplay.Start();
			}

			#endregion

			#region background images interface

            //set the initial gif background
            backgroundDisplay.SetImageBitmap(BitmapFactory.DecodeResource(this.Resources,
                Resource.Drawable.bg_gifaroo_21));

			bgColorsSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgColorsSetButton);

				backgroundImagesGrid.Visibility = ViewStates.Visible;
				backgroundImagesGrid.Adapter = new EditorColorAdapter (this, colorHexCodes);
				selectedBackgroundSet = BGSetNames.Colors;
			};
			bgNightSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgNightSetButton);

				bool shouldUnlock = false;
				if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].owned == true
					||_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].temporarilyOwned == true){
					shouldUnlock = true;
				}
				BackgroundSetClicked (thumbIds_Night ,shouldUnlock);
				
				selectedBackgroundSet = BGSetNames.Night;
			};
			bgBlurSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgBlurSetButton);

				bool shouldUnlock = false;
				if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].owned == true
					||_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].temporarilyOwned == true){
					shouldUnlock = true;
				}

				BackgroundSetClicked (thumbIds_Blurs, shouldUnlock);
				
				selectedBackgroundSet = BGSetNames.Blurs;
			};
			bgGradientsSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgGradientsSetButton);

				bool shouldUnlock = false;
				if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].owned == true
					||_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].temporarilyOwned == true){
					shouldUnlock = true;
				}

				BackgroundSetClicked (thumbIds_Gradients
					,shouldUnlock);
				
				selectedBackgroundSet = BGSetNames.Gradients;
			};
			bgPatternsSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgPatternsSetButton);

				bool shouldUnlock = false;
				if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].owned == true
					||_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].temporarilyOwned == true){
					shouldUnlock = true;
				}

				BackgroundSetClicked (thumbIds_Patterns, shouldUnlock);
				
				selectedBackgroundSet = BGSetNames.Patterns;
			};
			bgGifarooSetButton.Click += (object sender, EventArgs e) => {

				DeselectBackgroundButtons();
				SelectBackgroundButton(ref bgGifarooSetButton);

				BackgroundSetClicked (thumbIds_gifaroo);
				selectedBackgroundSet = BGSetNames.Gifaroo;
			};
			
            BGSetNames[] bgSetNames = new BGSetNames[] { //array of "premium" pack names
						BGSetNames.Night,
						BGSetNames.Blurs,
						BGSetNames.Gradients,
						BGSetNames.Patterns
					};
            LazyLoadAdapter adapter = null;
            Bitmap colorBitmap;
			backgroundImagesGrid.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

                if (selectedBackgroundSet == BGSetNames.Colors)
                {
                    selectedBackground = colorHexCodes[e.Position];
                    Rect rect = new Rect(0, 0, 1, 1);
                    colorBitmap = Bitmap.CreateBitmap(rect.Width(), rect.Height(), Bitmap.Config.Argb8888);
                    Canvas canvas = new Canvas(colorBitmap);

                    Color color = Color.ParseColor(colorHexCodes[e.Position]);
                    Paint paint = new Paint();
                    paint.Color = color;
                    canvas.DrawRect(rect, paint);

                    backgroundDisplay.SetImageBitmap(colorBitmap);
                    return;
                }
				
				if (bgSetNames.Contains(selectedBackgroundSet)
					&& e.Position >= 5 // only 5 first items are free
				){
					switch(selectedBackgroundSet){
					case BGSetNames.Blurs:
					case BGSetNames.Patterns:
						if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].owned == false
							&&_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].temporarilyOwned == false){
							ShowStore(this, PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU);
							return;
						}
						break;

					case BGSetNames.Gradients:
					case BGSetNames.Night:
						if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].owned == false
							&&_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].temporarilyOwned == false){
							ShowStore(this, PremiumInterface.SKUs.Gradients_Night_BGPack_SKU);
							return;
						}
						break;
					}
				}

				//add background to preview, this only happens if the bakcground selected is owned
				loadingContentDisplay(true);
				//backgroundImagesGrid.SmoothScrollToPosition (e.Position);

                //recyclebitmaps to prevent outOfMemory exceptions
                if (backgroundDisplay != null) {
                    ((global::Android.Graphics.Drawables.BitmapDrawable)backgroundDisplay.Drawable).Bitmap.Recycle();
                    ((global::Android.Graphics.Drawables.BitmapDrawable)backgroundDisplay.Drawable).Bitmap.Dispose();
                }
                GC.Collect();
                
				try{
                    if (adapter != (LazyLoadAdapter)backgroundImagesGrid.Adapter)
					    adapter = (LazyLoadAdapter)backgroundImagesGrid.Adapter;

                    using (Bitmap bmap = BitmapFactory.DecodeResource(
                            this.Resources, adapter.thumbIds[e.Position]))
                    {
                        backgroundDisplay.SetImageBitmap(bmap);
                    }

					selectedBackground = adapter.thumbIds[e.Position].ToString();

				} catch(Exception)
                {
                    //TODO CONSUME STATUS WHEN SAVE BUTTON IS PRESSED
                    SavePremiumStatusToSolidState(); //The app is about to crash so save the temporary premium status of the user
                    try {
                        ActionBar.SelectTab(null); // the app is out of memory.
                    }
                    catch (Exception) { }
                }
				loadingContentDisplay(false);
			};
            #endregion

			#region Text Interface
			ToggleTextControls(false);
			//Color selectedTextItemBackground = Color.Argb(70,100,100,100);
			float initialTextSize = (25f/100) * gifDisplayDimension;
            textEditor = new TextStickerEditor();

			//Font size spinner
			//var fontSizes = new List<string> (){"Small", "Medium", "Large", "Largest"};
			//var sizeAdapter = new ArrayAdapter<string> (this, global::Android.Resource.Layout.SimpleSpinnerDropDownItem, fontSizes);
			//sizeSpinner.Adapter = sizeAdapter;

            //add bitmaps to editor so that they can be recycled
            textEditor.deleteButtonBitamp = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.deleteX);
            textEditor.scalePanButtonBitmap = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.spanscale);

			updateWidgetsAfterConnection();

			//Add a new textView wenever the "new" button is clicked;
			newTextButton.Click += (object sender, EventArgs e) => {

                textEditor.newTextView = new XTextStickerView(this);
                textEditor.newTextView.IvDelete.SetImageBitmap(textEditor.deleteButtonBitamp);
                textEditor.newTextView.IvScale.SetImageBitmap(textEditor.scalePanButtonBitmap);
                
                textEditor.newTextView.SetBackgroundResource(Resource.Drawable.customBorder);

				//newTextView.SetHeight();
				

				//newTextView.SetWidth(WindowManagerLayoutParams.WrapContent);
				//var touchListener = new CustomTouchListener();
				//touchListener.mRrootLayout = contentDisplay;
				//newTextView.SetOnTouchListener(touchListener);

				ToggleTextControls (true);
				try{
					textEditor.textViewToEdit.HideControls();
				}catch(Exception){}
				try {
                    textEditor.newTextView.TvMain.SetIncludeFontPadding(false);
                    textEditor.newTextView.TvMain.SetTextSize(global::Android.Util.ComplexUnitType.Px, 1000);
					textEditor.newTextView.TvMain.Text = "New...";
					//textEditor.newTextView.SetTextSize (global::Android.Util.ComplexUnitType.Px, initialTextSize, "Medium");
					textEditor.newTextView.setTypefaceAndSaveString (this, "Anton");
					//sizeSpinner.SetSelection(fontSizes.IndexOf(textEditor.newTextView.TvMain.sizeChoiceString));
					fontSpinner.SetSelection(Array.IndexOf(fontsArray,"Anton"));
					textEditor.newTextView.TvMain.SetTextColor (Color.White);
					//textEditor.newTextView.TvMain.Gravity = GravityFlags.Center;
					//newTextView.SetBackgroundColor(selectedTextItemBackground);
                    //newTextView.SetBackgroundResource(Resource.Drawable.customBorder);
				} 
				catch (Exception ex) {
					throw ex;
				}
                textEditor.textViewToEdit = textEditor.newTextView;

                /*For some reason, the library taht allows fro text rotation needs to know the amount of pixels 
                 from the top of the screen all the way to the top of the parent of the stickertextview
                 (In this case, the parent is content display)
                 The library itself gets the height of the status bar, and the action bar, we then need to provide
                 the height of any other items in the way to (textEditor layout)*/
                textEditor.textViewToEdit.ExtraTopSpace = textLayout.Height + actionBarHeight;

                textEditor.stickerTextViewList.Add(textEditor.newTextView);
				contentDisplay.AddView (textEditor.newTextView);
                
                /*
				var lp = (RelativeLayout.LayoutParams)textEditor.newTextView.TvMain.LayoutParameters;
				lp.Width = WindowManagerLayoutParams.WrapContent;
				newTextView.LayoutParameters = lp;
                newTextView.InputType = global::Android.Text.InputTypes.ClassText;
                */

				try{
					textInput.Text = "";
				}catch(Exception){}
				textInput.Hint = "New...";
				textEditor.newTextView.TvMain.Text = "New...";

                //Touched is fired de moment the user's finguer touches the view.
                ((Editor.XTextStickerView.XActionDownLaunchedListener)
                    (textEditor.newTextView.ActionDownLaunchedListener))
                    .EActionDownLaunched += delegate(object sender11, EventArgs e2)
                {

                    Editor.XTextStickerView.XActionDownLaunchedListener thisListener =
                        (Editor.XTextStickerView.XActionDownLaunchedListener)sender11;
                    XTextStickerView touchedTextView = thisListener.parentStickerTextview;
                    
                    textTab.Select();

                    if (textEditor.textViewToEdit != touchedTextView)
                        textEditor.textViewToEdit.HideControls();

                    touchedTextView.ShowControlls();
                    touchedTextView.SetBackgroundResource(Resource.Drawable.customBorder);
					//newTextView.SetBackgroundColor(selectedTextItemBackground);
                    textEditor.textViewToEdit = touchedTextView;
					//bring to front in UI and List
					textEditor.textViewToEdit.BringToFront();
					textEditor.MoveItemToFrontOfList(textEditor.textViewToEdit);

					if(textEditor.textViewToEdit.TvMain.Text != "New...")
                        textInput.Text = textEditor.textViewToEdit.TvMain.Text;
					else{
						textInput.Text = "";
                        textEditor.textViewToEdit.TvMain.Text = "New...";
					}
					
					fontSpinner.SetSelection (Array.IndexOf(fontsArray, textEditor.textViewToEdit.fontStringName));
					//sizeSpinner.SetSelection (fontSizes.IndexOf(textEditor.textViewToEdit.sizeChoiceString));
					ToggleTextControls (true);
                };

                textEditor.newTextView.IvDelete.Click += delegate (object sender2, EventArgs aaaarfs)
                {
                    ImageView button = (ImageView)sender2;
                    XTextStickerView textView = (XTextStickerView)button.Parent;
                    RelativeLayout parent = (RelativeLayout)textView.Parent;
                    parent.RemoveView(textView);
                    textEditor.stickerTextViewList.Remove(textEditor.newTextView);

				    textInput.Text = "";
				    ToggleTextControls (false);
                };

                /* old implementation before stickertextview
				newTextView.Click += (object sender1, EventArgs e1) => {
					textTab.Select();
					textViewToEdit.SetBackgroundResource(0);
                    newTextView.SetBackgroundResource(Resource.Drawable.customBorder);
					//newTextView.SetBackgroundColor(selectedTextItemBackground);
					textViewToEdit = newTextView;
					//bring to front in UI and List
					textViewToEdit.BringToFront();
					MoveItemToFrontOfList(textViewToEdit);

					if(newTextView.Text != "New...")
						textInput.Text = newTextView.Text;
					else{
						textInput.Text = "";
						newTextView.Text = "New...";
					}
					
					fontSpinner.SetSelection (Array.IndexOf(fontsArray, textViewToEdit.FontNameString));
					sizeSpinner.SetSelection (fontSizes.IndexOf(textViewToEdit.sizeChoiceString));
					ToggleTextControls (true);
				};
                 */

				textInput.FocusChange += (object sender2, View.FocusChangeEventArgs e2) => {
					if (textEditor.newTextView.TvMain.Text == "" && textInput.HasFocus == false) {
						textEditor.stickerTextViewList.Remove (textEditor.newTextView);
						contentDisplay.RemoveView (textEditor.newTextView);
						if (textEditor.stickerTextViewList.Count == 0) {
							ToggleTextControls (false);
						}
					}
					//For ease of use, auto select all of the text if the textview is new.
					if (textInput.HasFocus == true
						&& textInput.Text == "New"){
						textInput.SelectAll();
					}
					if (!textInput.IsFocused) {
						manager.HideSoftInputFromWindow (textInput.WindowToken, 0);
					}
				};
				textInput.RequestFocus();
				textInput.SetSelection(textInput.Text.Length);
			};
			textInput.Click += (object sender, EventArgs e) => {
				if(textInput.IsFocused && textInput.Text == "New"){
					var textview = (EditText)sender;
					textview.SetSelection(0, textInput.Text.Length - 1);
				}
			};

            /* old implementation to delete textview before stickertextview was implemented.
             * It was left here for reference.
			deleteTextButton.Click += (object sender, EventArgs e) => {
				textInput.Text = "";
				textViewList.Remove (textViewToEdit);
				contentDisplay.RemoveView (textViewToEdit);
				ToggleTextControls (false);
			};
            */

			fontSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
				//open the store if the fonts pack is not owned and the selected font is premium
				//else, apply the font to the textview being edited
				if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Fonts_Pack_SKU].owned == false
					&& FontArrayAdapter.premiumFonts.Contains(
						(String)fontSpinner.GetItemAtPosition (e.Position) )){
					ShowStore(this, PremiumInterface.SKUs.Fonts_Pack_SKU);
					fontSpinner.SetSelection(_lastFontPositionSelected); //select the last free font selected
				} else {
					_lastFontPositionSelected = e.Position;
					textEditor.textViewToEdit.setTypefaceAndSaveString (this, (string)fontSpinner.GetItemAtPosition (e.Position));
				}
			};

            /*
			sizeSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
				string selected = (string)sizeSpinner.GetItemAtPosition (e.Position);
				//by default set to 13.2% of the total gif display size
				float sizePercentage = 13f;
				switch (selected) {
				case "Small":
					sizePercentage = 8f;
					break;
				case "Medium":
					sizePercentage = 13f;
					break;
				case "Large":
					sizePercentage = 21f;
					break;
				case "Largest":
					sizePercentage = 34f;
					break;
				}
				float second = (sizePercentage / 100f);
				float textSize = second * (float)gifDisplayDimension;
				textViewToEdit.SetTextSize (global::Android.Util.ComplexUnitType.Px, textSize, selected);
				//textViewToEdit.SetHeight((int)textSize);
	
			};
            */

			textInput.TextChanged += (object sender, global::Android.Text.TextChangedEventArgs e) => {
				textEditor.textViewToEdit.TvMain.Text = textInput.Text;
			};
			textColorGrid.Adapter = new EditorColorAdapter (this, colorHexCodes);
			textColorGrid.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				textColorGrid.SmoothScrollToPosition (e.Position);
				textEditor.textViewToEdit.TvMain.SetTextColor (Color.ParseColor (colorHexCodes [e.Position]));
			};
             			#endregion

   		}

        /// <summary>
        /// used to save wether the user temporarily unlocked premium content before a crash.
        /// </summary>
        private void SavePremiumStatusToSolidState() {
            sharedPreferencesPremium.Edit().PutBoolean(
                PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU,
                _premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].temporarilyOwned);
            sharedPreferencesPremium.Edit().PutBoolean(
                PremiumInterface.SKUs.Gradients_Night_BGPack_SKU,
                _premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].temporarilyOwned);
        }
        private void SetPremiumStatusInSolidStateToFalse() {
            sharedPreferencesPremium.Edit().PutBoolean( PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU, false);
            sharedPreferencesPremium.Edit().PutBoolean( PremiumInterface.SKUs.Gradients_Night_BGPack_SKU, false);
            
        }

		/// <summary>
		/// loads the in app purchase store.
                		/// </summary>
		private void ShowStore(Context context, string PackSKU = "none"){
			if (_premiumInterface.InventorySynched == false
				&& _serviceConnection != null) {
				Toast.MakeText (this, "Connecting to the play Store.", ToastLength.Short);
				try{
					_premiumInterface.SyncInventory ();
				}catch(Exception){
					AlertDialog.Builder builder = new AlertDialog.Builder (context);
					AlertDialog dialog = null;
					builder.SetMessage ("Please check your Internet connection and make sure that you are logged-in to your Google account.")
						.SetCancelable (false)
						.SetPositiveButton ("ok", ((sender, e) => {
							
						}));
					dialog = builder.Create ();
					dialog.Show ();
				}
			}

			//The store wrapper is not vissible and not in topRelativeLayout from the beginning.
			if (StoreWrapper.Visibility == ViewStates.Gone)
				StoreWrapper.Visibility = ViewStates.Visible;
			if (StoreWrapper.Parent == null)
				topRelativeLayout.AddView (StoreWrapper);

			ActionBar.Hide ();

            //track this visit
            trackingInterface.TrackStoreVisit( (PackSKU == "none") ? "Store Button" : PackSKU );

			gifDisplay.LoadUrl ("about:blank");

			//this will throw an exception if there was a problem retreiving product info from the play server earlier.
			try{ updateStoreInfo(); }
			catch(Exception ex){
				purchaseFontsPackButton.Text = "get";
				purchaseGradientsNightPackButton.Text = "get";
				purchaseBlursPatternsPackButton.Text = "get";
			}

			//store items
			RelativeLayout fontsStoreItem = FindViewById<RelativeLayout> (Resource.Id.fontsStoreItem);
			RelativeLayout BlurryPatternsStoreItem = FindViewById<RelativeLayout> (Resource.Id.BlurryPatternsStoreItem);
			RelativeLayout gradientNightsStoreItem = FindViewById<RelativeLayout> (Resource.Id.gradientNightsStoreItem);
			ImageButton closeStoreButton = FindViewById<ImageButton> (Resource.Id.closeStoreButton);
			HorizontalScrollView storeItemsScrollView = FindViewById<HorizontalScrollView>(Resource.Id.storeItemsScrollView);

			//set store closing listerners
			closeStoreButton.Click += delegate { RemoveStore();	};
			StoreWrapper.Click += delegate { RemoveStore(); };
			storeItemsScrollView.Click += delegate { RemoveStore(); };

			//fonts store item setup
			WebView fonts_store_item_webView = FindViewById<WebView> (Resource.Id.fonts_store_item_webView);
			//fonts_store_item_webView.Settings.JavaScriptEnabled = true;
			fonts_store_item_webView.SetOnTouchListener(
					new DisableTouchListener()
				);
			fonts_store_item_webView.LoadUrl("file:///android_asset/fonts_animation_homepage/index.html");
			fonts_store_item_webView.SetBackgroundColor(Color.Transparent);

			//blurs_night backgrounds items setup
			WebView gradientnights_store_item_webView = FindViewById<WebView> (Resource.Id.gradientnights_store_item_webView);
			gradientnights_store_item_webView.SetOnTouchListener(
				new DisableTouchListener()
			);
			gradientnights_store_item_webView.LoadUrl("file:///android_asset/gradient_nights_animation_homepage/index.html");

			//blurs_night backgrounds items setup
			WebView blursPatterns_store_item_webView = FindViewById<WebView> (Resource.Id.blursPatterns_store_item_webView);
			blursPatterns_store_item_webView.SetOnTouchListener(
				new DisableTouchListener()
			);
			WebBrowserClient lastWebViewsClient = new WebBrowserClient ();
			blursPatterns_store_item_webView.SetWebViewClient (lastWebViewsClient);
			lastWebViewsClient.EOnPageFinished += (object sender, EventArgs e) => {
				//scroll to the relevant store item
				if (PackSKU == PremiumInterface.SKUs.Fonts_Pack_SKU) {
					storeItemsScrollView.SmoothScrollTo (
						fontsStoreItem.Left - ((Resources.DisplayMetrics.WidthPixels/2) - (fontsStoreItem.Width/2)),
						0
					);
				} 
                else if (PackSKU == PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU) {
					storeItemsScrollView.SmoothScrollTo (
						BlurryPatternsStoreItem.Left - ((Resources.DisplayMetrics.WidthPixels/2) - (BlurryPatternsStoreItem.Width/2)),
						0
					);
                }
                else if (PackSKU == PremiumInterface.SKUs.Gradients_Night_BGPack_SKU
                  || PackSKU == "none")
                {
					storeItemsScrollView.SmoothScrollTo (
						gradientNightsStoreItem.Left - ((Resources.DisplayMetrics.WidthPixels/2) - (gradientNightsStoreItem.Width/2)),
						0
					);
				}
			};
			blursPatterns_store_item_webView.LoadUrl("file:///android_asset/blur_patters_animation_homepage/index.html");

			//enter webview references to array so that they can be unloaded from other methods
			storeWebViews = new WebView[] {
				fonts_store_item_webView,
				gradientnights_store_item_webView,
				blursPatterns_store_item_webView
			};

			//set the margin for the linearlayout so that all items can be centered when scrolled
			LinearLayout StoreItemsLinearLayout = FindViewById<LinearLayout> (Resource.Id.StoreItemsLinearLayout);
			int SidePadding = (Resources.DisplayMetrics.WidthPixels / 4);
			if (StoreItemsLinearLayout.PaddingLeft != SidePadding)
			//int SidePadding = (Resources.DisplayMetrics.WidthPixels / 2) - (fontsStoreItem.Width / 2);
			StoreItemsLinearLayout.SetPadding(
				SidePadding,
				0,
				SidePadding,
				0
			);

		}
		private void RemoveStore(){
			try{
			gifDisplay.LoadDataWithBaseURL ("file://"+FilesDir.AbsolutePath+"/" ,htmlDataForWebView, "text/html", "utf-8", null);
			} catch(Exception){}

			//remove loaded stuff from webviews
			if (storeWebViews != null)
				foreach (WebView view in storeWebViews) {
					view.LoadUrl ("about:blank");
				}
			
			ActionBar.Show ();

			if (StoreWrapper.Parent != null)
				topRelativeLayout.RemoveView (StoreWrapper);
   		}
		private void SetStoreListenersAndAddReferenceToWidgets(){
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Fonts_Pack_SKU].purchaseButton = purchaseFontsPackButton;
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].purchaseButton = purchaseBlursPatternsPackButton;
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].purchaseButton = purchaseGradientsNightPackButton;

			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].freeUnlockButton = freeUnlockBlursPatternsButton;
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].freeUnlockButton = freeUnlockNightGradientButton;

			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Fonts_Pack_SKU].descriptiontext = textViewFontsPackDescription;
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].descriptiontext = textViewBlursPatternsDescription;
			_premiumInterface.ProductsDictionary [PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].descriptiontext = textViewgradientsNightDescription;

			foreach (KeyValuePair<string, PremiumInterface.StoreItem> entry in _premiumInterface.ProductsDictionary) {

				entry.Value.purchaseButton.Click += entry.Value.PurchaseButtonCLicked;  //activate the purchase button listeners
				if (entry.Value.freeUnlockButton != null)                               //activate the free unlock listener
                {
                    try {
                        entry.Value.temporarilyOwned = sharedPreferencesPremium.GetBoolean(entry.Value.product.ProductId, false); //check if the user unlocked something before a crash and unlock it
                    }
                    catch (Exception)
                    {}
                    entry.Value.freeUnlockButton.Click += (object sender, EventArgs e) =>
                    {
					if (entry.Value.temporarilyOwned == false
                        && entry.Value.owned == false){ //display only if the user doesnt own this item already

                        //Ask the user if they want to watch video to unlock the content
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Unlock for free");
                        alert.SetMessage("Watch the following video and unlock this item once.");
                        alert.SetPositiveButton("Sure", (senderAlert, args) => {
                            //Display ad
                            if (rewardedStoreAd != null
                                && rewardedStoreAd.IsReady)
                            {
                                rewardedStoreAd.DisplayAd();
                                bool visited = false;
                                trackingInterface.TrackRewardedVideoPrompt(entry.Value.itemName, true);
                                rewardedStoreAd.Error += delegate(object errorSender, Flurry.Ads.ErrorEventArgs eError)
                                {
                                    Toast.MakeText(this
                                    , "An error ocurred, please try again later."
                                    , ToastLength.Long).Show();
                                };
                                rewardedStoreAd.VideoCompleted += delegate(object senderff, EventArgs eff)
                                {   //The video was completed, unlock content and remove store.
                                    entry.Value.temporarilyOwned = true;
                                    backgroundImagesGrid.Visibility = ViewStates.Gone;
                                    RemoveStore();
                                };
                                rewardedStoreAd.Clicked += delegate(object senders, EventArgs er)
                                {
                                    visited = true;
                                    trackingInterface.TrackRewardedVideoResult(true);
                                };
                                rewardedStoreAd.Close += (object closersender, EventArgs closere) =>
                                {
                                    if (!visited)
                                        trackingInterface.TrackRewardedVideoResult(false);
                                    //reload a video add if there is anything else to unlock.
                                    if (_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU].temporarilyOwned == false ||
                                        _premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Gradients_Night_BGPack_SKU].temporarilyOwned == false)
                                    {
                                        rewardedStoreAd = new Flurry.Ads.FlurryAdInterstitial(this, "rewarded-store");
                                        rewardedStoreAd.FetchAd();
                                    }

                                    Toast.MakeText(this
                                        , entry.Value.itemName + " Unlocked!"
                                        , ToastLength.Long).Show();
                                };
                            }
                            else {
                                Toast.MakeText(this, "Please check your internet connection", ToastLength.Long).Show();
                            }
                        });
                        alert.SetNegativeButton("Maybe Later", (senderAlert, args) =>{
                            trackingInterface.TrackRewardedVideoPrompt(entry.Value.itemName, false);
                        });
                        //run the alert in UI thread to display in the screen
                        RunOnUiThread(() =>
                        {
                            alert.Show();
                        });

                    }else if (entry.Value.temporarilyOwned == true){
						Toast.MakeText(this, "You already unlocked this item!", ToastLength.Long).Show();
                    }
					//this is so that we know wich pack to unlock when completed.
                        _premiumInterface.SKUForTheLastFreeUnlockButtonSelected = entry.Key;
                    };
                }
            }

        }
		private void updateStoreInfo(){
			foreach (KeyValuePair<string, PremiumInterface.StoreItem> entry in _premiumInterface.ProductsDictionary) {
				entry.Value.purchaseButton.Visibility = ViewStates.Visible;
				if (entry.Value.owned == true) {
					if (entry.Value.freeUnlockButton != null) {
						entry.Value.freeUnlockButton.Text = "=)";
					}
					entry.Value.purchaseButton.Click -= entry.Value.PurchaseButtonCLicked;
					//entry.Value.purchaseButton.SetBackgroundColor (Color.DimGray);
					entry.Value.purchaseButton.Text = "Owned";
				} else {
					if (entry.Value.temporarilyOwned == true) {
						entry.Value.freeUnlockButton.Text = "Unlocked";
					}
					entry.Value.purchaseButton.Text = entry.Value.product.Price;
				}
				entry.Value.descriptiontext.Text = entry.Value.product.Description;
			}

		}
		/// <summary>
		/// Updates the widgets that require that the purchase data is already retreived.
		/// </summary>
		private void updateWidgetsAfterConnection(){
			fontsAdapter = new FontArrayAdapter (this,
				global::Android.Resource.Layout.ActivityListItem,
				fontsArray,
				_premiumInterface.ProductsDictionary[PremiumInterface.SKUs.Fonts_Pack_SKU].owned);
			fontsAdapter.SetDropDownViewResource (global::Android.Resource.Layout.ActivityListItem);
			fontSpinner.Adapter = fontsAdapter;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data){
			_serviceConnection.BillingHandler.HandleActivityResult (requestCode, resultCode, data);
		}

		protected override void OnPause ()
		{
			SetProgressBarIndeterminateVisibility (false);
			base.OnPause ();
		}

		//saves the effects (text, etc...) that go on front of the gif as a png with transparency.It then saves the background image and then launches the "save" activity.
		public bool SaveContentAndOpenSaveActivity(){
			try{
				//delete any file that starts with "my_file"
				//FindAndDeleteFileFromInternal("my_file");
			}catch(Exception e){}

			try
            {
				//SetProgressBarIndeterminateVisibility(true);
				Bitmap textBitmap = textEditor.DrawTextToBitmap (this,
                    Resource.Drawable.InvisibleDrawable, gifDisplayDimension,
                    actionBarHeight);

				GifarooTools.saveBitmapToDir (this, textBitmap, "top.png");
				GifarooTools.saveBitmapToDir (
                    this,
                    ((global::Android.Graphics.Drawables.BitmapDrawable)backgroundDisplay.Drawable).Bitmap,
                    "background.jpg");

                //This used to launch the save activity written in java
				//StartActivity (new Intent (this, Java.Lang.Class.ForName ("android.save")));

                //var SaveActivity = new Intent(this, typeof(SaveActivity));
                //SaveActivity.PutExtra( // Tell the save activity wether the user has premium status
                //    PremiumInterface.UserPremiunStatus.StatusFlags.ACTIVITY_FLAG,
                //    this._premiumInterface.userPremiumStatus.status);
                //StartActivity(SaveActivity);

				//used see the created text bitmap and compare sizes with the text fields
				backgroundDisplay.SetImageBitmap(textBitmap);
            }
            catch (Exception e){
				Toast.MakeText (this.appContext, "Something went wrong, please try again.", ToastLength.Long);
				SetProgressBarIndeterminateVisibility(false);
			}

			return true;
		}

		public bool FindAndDeleteFileFromInternal(string startsWith){
			foreach (string fileName in GetDir("*", FileCreationMode.Private).List()) {
				if (fileName.StartsWith (startsWith)) {
					GetDir (fileName, FileCreationMode.Private).Delete ();
					return true;
				}
			}
			return false;
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater inflater = this.MenuInflater;
			inflater.Inflate (Resource.Menu.menu, menu);

			saveButton = (IMenuItem)menu.FindItem (Resource.Id.downloadButton).ActionView;
			storeButton = (IMenuItem)menu.FindItem (Resource.Id.storeButton).ActionView;

			menu.FindItem (Resource.Id.editButton).SetVisible (false);
			menu.FindItem (Resource.Id.downloadButton).SetVisible (false);
			menu.FindItem (Resource.Id.URLBar).SetVisible (false);
			menu.FindItem (Resource.Id.homePageButton).SetVisible (false);

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {

			case Android.Resource.Id.saveButton:
				AlertDialog.Builder alertBuilder = new AlertDialog.Builder (this);
				alertBuilder.SetTitle ("Done?");
				alertBuilder.SetMessage ("You wont be able to make changes to your project after saving.");
				alertBuilder.SetPositiveButton ("Yes",
					((sender, e) => {
						try {
							SaveContentAndOpenSaveActivity ();
						} catch (Exception ex) {
							Toast.MakeText (this, "Something went wrong", ToastLength.Long);
						}
						Finish ();
					})
				);
				alertBuilder.SetNegativeButton ("No",
					((sender, e) => {
					})
				);
				RunOnUiThread (() => {
					//alertBuilder.Show ();

				});

                SetPremiumStatusInSolidStateToFalse();
				SaveContentAndOpenSaveActivity ();
				return true;

			case Android.Resource.Id.storeButton:

				ShowStore (this);

				return true;
			}

			return base.OnOptionsItemSelected (item);
		}

		//Toggles the "loading" view while gif is loading
		public void loadingContentDisplay(bool loading){
			if (loading == true) {
				contentDisplayLoadingTextView.Visibility = ViewStates.Visible;
			} else if (loading != true) {
				contentDisplayLoadingTextView.Visibility = ViewStates.Gone;
			}
			SetProgressBarIndeterminateVisibility(loading);
		}

		public override void OnBackPressed ()
		{
			//close the store if it is up
			if (StoreWrapper.Parent != null) {
				RemoveStore ();
			}
			//else, promp the user if they want to return
			else {
				AlertDialog.Builder alertBuilder = new AlertDialog.Builder (this);
				alertBuilder.SetMessage ("Are you sure?");
				alertBuilder.SetTitle ("Cancel");
				alertBuilder.SetPositiveButton ("Yes",
					((sender, e)=>{
						try {
							string filePath = FilesDir.AbsolutePath + Java.IO.File.Separator + gifFileName;
							Java.IO.File fileToDelete = new Java.IO.File (filePath);
							bool existent = fileToDelete.Exists ();
							bool delted = fileToDelete.Delete ();
							var list = FilesDir.List ();
						}
						catch (Exception ex) {
							
						}
						finally {
							
							Finish();
						}
					})
				);
				alertBuilder.SetNegativeButton ("No",
					((sender, e) => {})
				);
				RunOnUiThread (()=>{
					alertBuilder.Show ();
				});
			}
		
		}
		protected override void OnDestroy ()
		{
			if (_serviceConnection != null && _serviceConnection.Connected)
				_serviceConnection.Disconnect ();

			base.OnDestroy ();
		}

		/// <summary>
		/// Enables or disables the text controls
		/// </summary>
		/// <param name="count">Count.</param>
		private void ToggleTextControls (bool show){
			if (show == false) {
				/*fontSpinner.Visibility = ViewStates.Gone;
				sizeSpinner.Visibility = ViewStates.Gone;
				textInput.Visibility = ViewStates.Gone;*/
				//deleteTextButton.Visibility = ViewStates.Gone;
				textColorGrid.Visibility = ViewStates.Invisible;
				TextControlls1.Visibility = ViewStates.Gone;

				if ( TextControlls1.Parent != null) {
					textLayout.RemoveView(TextControlls1);
				}
			} else if (show){
				//fontSpinner.Visibility = ViewStates.Visible;
				//sizeSpinner.Visibility = ViewStates.Visible;
				//textInput.Visibility = ViewStates.Visible;
				textColorGrid.Visibility = ViewStates.Visible;
				//deleteTextButton.Visibility = ViewStates.Visible;
				TextControlls1.Visibility = ViewStates.Visible;

				if (TextControlls1.Parent == null) {
					textLayout.AddView (TextControlls1);
				}
			}
		}

		/// <summary>
		/// makes all of the background button the same size again.
		/// </summary>
		private void DeselectBackgroundButtons(){
			if (imageButtons.Length > 0)
			foreach (ImageButton button in imageButtons) {
				button.LayoutParameters = BGSetNormalLayoutParams;
			}
		}
		private void SelectBackgroundButton(ref ImageButton ButtonToHighlight){
			LinearLayout.LayoutParams selectedBGButtonParams = new LinearLayout.LayoutParams(
				BGSetNormalLayoutParams.Width + 20,
				BGSetNormalLayoutParams.Height + 20
			);
			ButtonToHighlight.LayoutParameters = selectedBGButtonParams;
		}

		public void BackgroundSetClicked(int[] thumbIds, bool packageIsOwned = true){

            try
            {
                for (int i = 0; i < myAdapter.Count; i++)
                {
                    ImageView view = (ImageView)myAdapter.GetItem(i);
                    ((global::Android.Graphics.Drawables.BitmapDrawable)view.Drawable).Bitmap.Recycle();
                }
            }
            catch (Exception)
            { }

			SetProgressBarIndeterminateVisibility(true);
			rightSideBarProgressBar.Visibility = ViewStates.Visible;
			backgroundImagesGrid.Visibility = ViewStates.Visible;
			int resourceLayout = Android.Resource.Layout.BackgroundItemsLayout;
			try {
				Task.Factory.StartNew (() => {
					myAdapter = new ListViewLazyLoadingImages.LazyLoadAdapter (
						this,
						this,
						thumbIds,
						packageIsOwned,
						resourceLayout);
				}).ContinueWith (t => {
					backgroundImagesGrid.Adapter = myAdapter;
					rightSideBarProgressBar.Visibility = ViewStates.Gone;
					SetProgressBarIndeterminateVisibility(false);
				}, System.Threading.CancellationToken.None,
					TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext ()
				).Start ();
			} catch (InvalidOperationException) {}
   		}

		public Bitmap GetImageBitmapFromUrl(string url){
			Bitmap imageBitmap = null;

			try {
				using (var webClient = new WebClient ()) {
					var imageBytes = webClient.DownloadData (url);
					if (imageBytes != null && imageBytes.Length > 0) {
						imageBitmap = BitmapFactory.DecodeByteArray (imageBytes, 0, imageBytes.Length);
					}
				}
			} catch (Exception) {

			}

			return imageBitmap;
   		}
			

		/// <summary>
		/// Used to control the dragging of the text on top of the gif.
		/// TODO: remove the middle gravity and the match parent width parameter from the text view when created. then uncomment the x stuff.
		/// </summary>
		public class CustomTouchListenerl : Java.Lang.Object, View.IOnTouchListener{
			public ViewGroup mRrootLayout;
			private int _yDelta;
			private int _xDelta;

			public Boolean OnTouch(View view, MotionEvent Mevent){
				int x = (int)Mevent.RawX;
				int y = (int)Mevent.RawY;
				switch (Mevent.Action & MotionEventActions.Mask) {
				case MotionEventActions.Down:
					RelativeLayout.LayoutParams lparams = (RelativeLayout.LayoutParams)view.LayoutParameters;
					_xDelta = x - lparams.LeftMargin;
					_yDelta = y - lparams.TopMargin;
					view.PerformClick ();
					break;
				case MotionEventActions.Up:
					break;
				case MotionEventActions.PointerDown:
					break;
				case MotionEventActions.PointerUp:
					break;
				case MotionEventActions.Move:
					RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)view.LayoutParameters;
					layoutParams.LeftMargin= x - _xDelta; //used to be leftmargin
					layoutParams.TopMargin = y - _yDelta;
					layoutParams.RightMargin = -1000; //TODO try increasing this number
					layoutParams.BottomMargin = -250;
					view.LayoutParameters = layoutParams;
					break;
				}
				mRrootLayout.Invalidate ();
				return true;
			}
		}

		/// <summary>
		/// used to disable all touching on a view.
		/// </summary>
		public class DisableTouchListener : Java.Lang.Object, View.IOnTouchListener {
			public bool OnTouch(View v, MotionEvent Mevent){
				return (Mevent.Action == MotionEventActions.Move);
			}
		}

		public string getContentType(string fileName){
			foreach (string imageExtension in GifarooTools.supportedImageExtensions) {
				if (fileName.Contains (imageExtension)) {
					return "img";
				}
			}
			return "video";
   		}

	}

	/// <summary>
	/// Stores necessary items that TextView cannot
	/// </summary>
	public class GifarooTextview : TextView{
		private String _fontStringName;
		private String _sizeChoiceString;

		public String FontNameString{
			get{ return _fontStringName; }
		}
		public String sizeChoiceString{
			get{ return _sizeChoiceString; }
		}

		public GifarooTextview(Context context): base(context){}

		public void setTypefaceAndSaveString(Context context, string typefaceValue){
			this._fontStringName = typefaceValue;
			this.Typeface = CreateTypeface (context, typefaceValue);
		}

		public void SetTextSize (global::Android.Util.ComplexUnitType unit, float size, string sizeChoice)
		{
			this._sizeChoiceString = sizeChoice;
			base.SetTextSize (unit, size);
		}

		public static Typeface CreateTypeface(Context context, string typefaceValue)
		{
			//TypefaceStyle m_Style = TypefaceStyle.Normal;
			Typeface typeface = null;
			try {
				typeface = Typeface.CreateFromAsset(context.Assets, "fonts/"+typefaceValue+".ttf");
			} catch (Exception e){
				throw e;
			}
			return typeface;
		}
  	}

	public class EditorColorAdapter : BaseAdapter
	{
		Context context;
		string[] colorHexCodes;

		public EditorColorAdapter (Context c, string[] colorsInHex)
		{
			context = c;
			colorHexCodes = colorsInHex;
		}

		public override int Count {
			get { return colorHexCodes.Length; }
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return 0;
		}

		// create a new ImageView for each item referenced by the Adapter
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			ImageView imageView;

			try {
				if (convertView == null) {  // if it's not recycled, initialize some attributes
					imageView = new ImageView (context);
					imageView.LayoutParameters = new GridView.LayoutParams (
						WindowManagerLayoutParams.MatchParent,
						parent.Width
					);
					imageView.SetPadding (2, 2, 2, 2);
				} else {
					imageView = (ImageView)convertView;
				}
			} catch (Exception) {
				imageView = new ImageView (context);
				imageView.LayoutParameters = new GridView.LayoutParams (0, 0);
			}
			try{
				imageView.SetBackgroundColor (
					Color.ParseColor(colorHexCodes[position])
				);
			}catch (Exception){
			}
			return imageView;
		}
  	}

	public class FontArrayAdapter: ArrayAdapter{
		string[] objects;
		int resource;
		Context context;

		/// <summary>
		/// contains the names of the premium fonts to cross reference.
		/// </summary>
		static public string[] premiumFonts = {
			"FrederickatheGreat",
			"LeckerliOne",
			"IndieFlower",
			"KaushanScript",
			"Lobster",
			"Megrim",
			"Monoton",
			"Orbitron",
			"Pacifico",
			"PoiretOne",
			"Sacramento",
			"Righteous",
			"SixCaps"
		};
		private bool _purchased;

		/// <summary>
		/// Initializes a new instance of the <see cref="Gifaroo.Android.FontArrayAdapter"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="resource">Resource.</param>
		/// <param name="objects">Objects.</param>
		/// <param name="purhcased">If set to <c>true</c> do not show premium items as locked.</param>
		public FontArrayAdapter(Context context, int resource, string[] objects, bool purchased):base(context, resource, objects){
			this.context = context;
			this.resource = resource;
			this.objects = objects;

			this._purchased = purchased;
		}

		public override View GetDropDownView (int position, View convertView, ViewGroup parent)
		{
			View listItem = null;
			AQuery aq = new AQuery (context);

			if (listItem == null) {
				var inflater = Application.Context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				listItem = inflater.Inflate (resource, null);
			}

			TextView textView = listItem.FindViewById <TextView>(global::Android.Resource.Id.Text1);
			ImageView imageView = listItem.FindViewById <ImageView> (global::Android.Resource.Id.Icon);
			LinearLayout.LayoutParams imageparams = new LinearLayout.LayoutParams (
				LinearLayout.LayoutParams.WrapContent,
				LinearLayout.LayoutParams.WrapContent);
			imageparams.Gravity = GravityFlags.Center;
			imageView.LayoutParameters = imageparams;
			try {
				textView.Typeface = XTextStickerView.CreateTypeface (context, objects [position]);
				textView.Text = objects[position];
			} catch (Exception) {}

			//create a relative layout if the font is premium and the fonts pack is not unlocked
			if (_purchased == false
				&& premiumFonts.Contains (objects[position])) {
				Bitmap lockImage = aq.GetCachedImage (Resource.Drawable.ic_action_gifaroo_plus_stamp);
				imageView.SetImageBitmap (lockImage);
				imageView.Visibility = ViewStates.Visible;
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			return listItem;
		}
			
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
			var listItem = inflater.Inflate (resource, null);

			TextView textView = listItem.FindViewById <TextView>(global::Android.Resource.Id.Text1);
			ImageView imageView = listItem.FindViewById <ImageView> (global::Android.Resource.Id.Icon);

			try {
				textView.Typeface = XTextStickerView.CreateTypeface (context, objects [position]);
				textView.Text = objects[position];
			} catch (Exception) {}

			//create a relative layout if the font is premium and the fonts pack is not unlocked
			if (_purchased == false
				&& premiumFonts.Contains (objects[position])) {
				imageView.SetImageDrawable (
					context.Resources.GetDrawable(global::Android.Resource.Drawable.IcLockLock)
				);
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			return listItem;
		}

	}

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
			base.OnPageStarted (view, url, favicon);
			if (EOnPageStarted != null)
				EOnPageStarted (this, EventArgs.Empty);
		}
	}
		
	public class VideoPreparedListener : Java.Lang.Object ,MediaPlayer.IOnPreparedListener{
		public void OnPrepared(MediaPlayer player)
		{
			player.Looping = true;
		}
	}
}

