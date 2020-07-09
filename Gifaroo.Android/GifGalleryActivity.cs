
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

namespace Gifaroo.Android
{
	[Activity (Label = "",
		ConfigurationChanges=global::Android.Content.PM.ConfigChanges.Orientation,
		ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]

	public class GifGalleryActivity : Activity
	{
		GridView gridview;
		WebView WebViewDisplay;
		LinearLayout linearLayoutTarget;
		LinearLayout mainLayout;
		TextView countText;

		IMenuItem editButton;

		string selectedGifUrl = string.Empty;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.GifGallery);
			ActionBar.SetDisplayHomeAsUpEnabled (true);

			//List of strings from main activity
			var _Returned_Thang = Intent.GetStringArrayListExtra ("gifUrlsList");

			List<String> gifUrlList = new List<string> (_Returned_Thang.ToList());

			//control/widgets assigments
			gridview = FindViewById<GridView> (Resource.Id.gridview);
			WebViewDisplay = new WebView (this);
			linearLayoutTarget = FindViewById<LinearLayout> (Resource.Id.linearLayoutTarget);
			mainLayout = FindViewById<LinearLayout> (Resource.Id.MainLayout);
			countText = FindViewById<TextView> (Resource.Id.countText);

			WebViewDisplay.SetBackgroundColor(Color.Transparent);
			int webViewDim = Resources.DisplayMetrics.HeightPixels / 3;
			LinearLayout.LayoutParams WVParams = new LinearLayout.LayoutParams (mainLayout.LayoutParameters);
			WVParams.Height = webViewDim;
			WVParams.Width = webViewDim;
			WVParams.Gravity = GravityFlags.Center;
			WebViewDisplay.LayoutParameters = (WVParams);
			WebViewDisplay.Settings.JavaScriptEnabled = true;

			linearLayoutTarget.AddView (WebViewDisplay);

			ImageAdapter imageAdapt = new ImageAdapter (this, Resources.DisplayMetrics.WidthPixels);
			List<Bitmap> bitmapList = new List<Bitmap> ();

			foreach (string url in gifUrlList) {
				Bitmap image = GetImageBitmapFromUrl (url);
				if (image != null)
					bitmapList.Add (image);
			}

			imageAdapt.thumbIds = bitmapList.ToArray ();
			gridview.Adapter = imageAdapt;
			countText.Text = bitmapList.Count + " gifs found";

			//loads text in webview
			string initialData = "<h1 style='color:white; font-family:Impact, Charcoal, sans-serif; text-align:center;'>Select a gif.</h1>";
			WebViewDisplay.LoadData (initialData,"text/html",null);

			gridview.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args) {

				WebViewDisplay.StopLoading();
				WebViewDisplay.ClearView();
				selectedGifUrl = gifUrlList[args.Position];
				//string rule = "";
				var bitMaps = gridview.Adapter.GetItem (args.Position);


				//Navigate here so that the string is already assigned

				//stuff used to know wether to stretch the image vertically or horizontally in css in the webview
				ImageView imagen = (ImageView)args.View;
				int drawableHeight = imagen.Drawable.IntrinsicHeight;
				int drawableWidth = imagen.Drawable.IntrinsicWidth;
				//rule = drawableHeight > drawableWidth ? "height" : "width";

				//This laysout the image as supossed in the webview
				string heightData = "<html><head><style type='text/css'>body{ margin:auto auto;text-align:center;} img{height:100%; } </style></head>" +
					"<body><img src='"+ gifUrlList[args.Position] +"'/></body></html>";
				string widthData = "<html><head><style type='text/css'> " +
					"body{ background-image: url('"+ gifUrlList[args.Position] +"'); background-repeat: no-repeat; background-attachment: fixed; background-position: center; background-size: 100%; } </style></head><body></body></html>";
				string data = drawableHeight > drawableWidth ? heightData : widthData;

				WebViewDisplay.LoadData(data, "text/html", null);
			};

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

		/// <param name="menu">The options menu in which you place your items.</param>
		/// <summary>
		/// HANDLE MENU ACTIONBAR HERE!!!!
		/// </summary>
		/// <returns>To be added.</returns>
		public override bool OnCreateOptionsMenu(IMenu menu){
			MenuInflater inflater = this.MenuInflater;
			inflater.Inflate (Resource.Menu.menu, menu);

			editButton = (IMenuItem)menu.FindItem (Resource.Id.editButton).ActionView;

			//Get rid of the unecessary views
			menu.FindItem (Resource.Id.downloadButton).SetVisible(false);
			menu.FindItem (Resource.Id.URLBar).SetVisible(false);

			gridview.ItemClick += delegate {
				menu.FindItem (Resource.Id.editButton).SetEnabled(true);
			};

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item){
			switch(item.ItemId){

			case Android.Resource.Id.editButton:
				if (selectedGifUrl != string.Empty) {
					string fileName = "myFile.gif";
					Task taskA = Task.Factory.StartNew(()=>{
						GifarooTools.DownloadFileFromURL(this,selectedGifUrl,fileName);
					});
					taskA.Wait();

					var editGifActivity = new Intent (this, typeof(EditGifActivity));
					List<string> myStringList = new List<string> ();
					myStringList.Add (fileName);
					editGifActivity.PutStringArrayListExtra ("gifUrl", myStringList);
					StartActivity (editGifActivity);
				} else 
					Toast.MakeText (this, "Select a gif First.", ToastLength.Short).Show();
				return true;

			case 16908332:
				Intent intent = new Intent ();
				intent.SetClass (BaseContext, typeof(MainActivity));
				intent.SetFlags (ActivityFlags.ReorderToFront);
				StartActivity (intent);
				return true;

			}
			return base.OnOptionsItemSelected(item);
		}

	}


	public class ImageAdapter : BaseAdapter{
		private readonly Context context;
		private int dimensionsForImages;

		public Bitmap[] thumbIds;

		public ImageAdapter(Context c, int displayWidth){
			context = c;
			this.dimensionsForImages = displayWidth/4;
		}
		public override int Count{
			get { return thumbIds.Length;}
		}
		public override Java.Lang.Object GetItem(int position)
		{
			return null;
		}

		public override long GetItemId(int position)
		{
			return 0;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			ImageView imageView;

			if (convertView == null)
			{
				// if it's not recycled, initialize some attributes
				imageView = new ImageView(context);
				imageView.LayoutParameters = new AbsListView.LayoutParams(dimensionsForImages, dimensionsForImages);
				//imageView.SetAdjustViewBounds(true);
				imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
				//imageView.SetPadding(3, 3, 3, 3);
				//imageView.SetBackgroundColor (Color.DimGray);
			}
			else
			{
				imageView = (ImageView) convertView;
			}
			imageView.SetImageBitmap(thumbIds[position]);
			return imageView;
		}
	}

}