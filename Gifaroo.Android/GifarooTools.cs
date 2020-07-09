using System;
//using HtmlAgilityPack;
using System.Collections.Generic;

using System.Net;
using System.Drawing;
using Android.Graphics;
using Java.Net;
using Java.IO;

namespace Gifaroo.Android
{
	public static class GifarooTools
	{
		static public string[] supportedExtensions = {
			".gif", ".jpg", ".png",
			".webm", ".mp4" ,".ogg"
		};
        //static public string[] supportedImageExtensions = {
        //    ".gif", ".jpg", ".png"
        //};

        static public string[] supportedImageExtensions = {
			".gif"
		};

		/*
		/// <summary>
		/// Gets all GIF urls from a passed HTML string.
		/// </summary>
		/// <returns>The GIF urls as a List of strings</c>.</returns>
		/// <param name="htmlString">Html string.</param>
		/// <param name="source">Source URI in case the urls are not complete in the HTML string.</param>
		public static List<string> GetAllGifUrls (string htmlString, Uri source){
			List<string> gifUrls = new List<string>();
			var document = new HtmlDocument ();

			try{
				document.LoadHtml(htmlString);
				var nodes = document.DocumentNode.Descendants("img");
				foreach (var img in nodes) {
					if (img.Attributes["src"].Value.EndsWith(".gif")) {
						if (!img.Attributes["src"].Value.StartsWith("http")) {
							gifUrls.Add(source.AbsoluteUri + img.Attributes["src"].Value.Substring(1));
						}
						else
							gifUrls.Add(img.Attributes["src"].Value);
					}
				}
			}
			catch(Exception){
			}
			return gifUrls;
		}
		*/

		/// <summary>
		/// Gets all GIF urls from a 1D Json response.
		/// </summary>
		/// <returns>all GIF urls in a List of strings.</c>.</returns>
		/// <param name="JSONresponse">JSON response.</param>
		/// <param name="OriginalUrl">Original URL.</param>
		public static List<string> GetAllGifUrls (string JSONResponse, string OriginalUrl){
			List<string> gifUrls = new List<String> ();
			string stringToMod = JSONResponse;
			Uri temp_uri;
			string baseURL = String.Empty;

			try{
				temp_uri = new Uri(OriginalUrl);
				baseURL = temp_uri.Scheme + "://" + temp_uri.Host;
				var charsToRemove = new string[] { "\\", "\"","[","]"};
				foreach (var c in charsToRemove){
					stringToMod = stringToMod.Replace(c, string.Empty);
				}
				string[] stringSeparators = new string[] {","};
				string [] urlArray = stringToMod.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

				gifUrls = new List<String> ();

				foreach (string url in urlArray){
					if (url.Contains(".gif")||url.Contains("googleusercontent"))
						gifUrls.Add(url);
				}

				if (gifUrls.Count >=0)
					for (int i = 0; i < gifUrls.Count; i++) {
						if (!gifUrls[i].StartsWith("http")) {
							if (gifUrls [i].StartsWith ("/"))
								gifUrls[i] = gifUrls [i].TrimStart ('/');
							string newString = baseURL + "/" + gifUrls[i];
							gifUrls.RemoveAt (i);
							gifUrls.Insert (i, newString);
						}
					};
			}
			catch(Exception){}
			return gifUrls;
		}

		/// <summary>
		/// returns query as a Google search URL or the same url if not a search query.
		/// </summary>
		/// <returns>The UR.</returns>
		/// <param name="query">Query.</param>
		public static string getURL(string query){
			string url = query;
			if (url == "") {
				url = "https://www.google.com";
				return url;
			}
			if (!url.Contains (".")) {
				/*
				url = "https://www.google.com/search?q=" + query + "&source=lnms&tbm=isch&sa=X&ei=yORsVNnqNfWTsQTB0IGgDw&ved=0CAcQ_AUoAQ&biw=375&bih=667&dpr=2#q=" + query + "&tbm=isch&tbs=itp:animated";
				string sssurl = "https://www.google.com/search?authuser=0&site=webhp&source=hp&ei=F7EpVJaNGYqoyASjyIG4Ag&q=" + query + "&oq=" + query + "&gs_l=mobile-gws-hp.3..0l4j5.2940.5524.0.5669.17.15.2.2.2.1.353.1888.4j10j0j1.15.0....0...1c.1.54.mobile-gws-hp..7.10.492.1.EYBl091DKOY";
				return url;
				*/
				url = "https://www.google.com/search?q=" + query;
				return url;
			}
			try{
				if(new Uri(url, UriKind.Absolute) != null){
					url = new Uri(url, UriKind.Absolute).AbsoluteUri;
					return url;
				}
			}
			catch (Exception){}
			try{
				if(new Uri("https://" + url,UriKind.Absolute) != null && url.Contains(".")) {
					url = new Uri("https://" + url,UriKind.Absolute).AbsoluteUri;
					return url;
				}
			}
			catch (Exception){}
			try{
				if(new Uri("http://" + url,UriKind.Absolute) != null && url.Contains(".")){
					url = new Uri("http://" + url,UriKind.Absolute).AbsoluteUri;
					return url;
				}
			}
			catch (Exception){}
			return url;
		}

		/// <summary>
		/// Downloads and stores a File from the provided URL.
		/// </summary>
		/// <returns><c>true</c>, if and store GIF from UR was downloaded, <c>false</c> otherwise.</returns>
		/// <param name="url">URL.</param>
		public static void DownloadFileFromURL(global::Android.Content.Context context, string url, string newFileNameWithExtension){

			URL address = new URL (url);
			URLConnection conn = null;

			conn = address.OpenConnection ();

			System.IO.Stream inputStream = conn.InputStream;
			BufferedInputStream bis = new BufferedInputStream (inputStream);
			Org.Apache.Http.Util.ByteArrayBuffer bab = new Org.Apache.Http.Util.ByteArrayBuffer (64);
			int current = 0;

			while ((current = bis.Read()) != -1){
				bab.Append ((byte) current);
			}

			var fos = context.OpenFileOutput (newFileNameWithExtension, global::Android.Content.FileCreationMode.Private);
			byte[] babByte = bab.ToByteArray();
			fos.Write (babByte, 0, babByte.Length);
			fos.Close ();
		}

		/// <summary>
		/// Downloads the file from URL but stops if keepDownloading becomes false.
		/// </summary>
		/// <returns><c>true</c>, if file from UR was downloaded, <c>false</c> otherwise.</returns>
		/// <param name="context">Context.</param>
		/// <param name="url">URL.</param>
		/// <param name="newFileNameWithExtension">New file name with extension.</param>
		/// <param name="keepDownloading">Keep downloading.</param>
		public static bool DownloadFileFromURL(global::Android.Content.Context context, string url, string newFileNameWithExtension, ref bool keepDownloading){

			URL address = new URL (url);
			var conn = address.OpenConnection ();
			System.IO.Stream inputStream = conn.InputStream;
			BufferedInputStream bis = new BufferedInputStream (inputStream);
			Org.Apache.Http.Util.ByteArrayBuffer bab = new Org.Apache.Http.Util.ByteArrayBuffer (64);
			int current = 0;

			while ((current = bis.Read()) != -1){
				if (keepDownloading == false){
					bab.Clear ();
					return false;
				}
				bab.Append ((byte) current);
			}

			var fos = context.OpenFileOutput (newFileNameWithExtension, global::Android.Content.FileCreationMode.Private);
			byte[] babByte = bab.ToByteArray();
			fos.Write (babByte, 0, babByte.Length);
			fos.Close ();
			return true;
		}

		/// <summary>
		/// Draws the text to bitmap wich is then saved as a png in the internal directory.
		/// </summary>
		/// <returns>The text to bitmap.</returns>
		/// <param name="context">Context.</param>
		/// <param name="textList">GifarooTextView list.</param>
		/// <param name="DrawableId">Path to a drawable (invisible in this case).</param>
		public static Bitmap DrawTextToBitmap(global::Android.Content.Context context, List<GifarooTextview> textList, int DrawableId, int gifDisplayDimensions)
		{
			float scale = context.Resources.DisplayMetrics.Density;
			//Bitmap bitmap = BitmapFactory.DecodeResource (context.Resources, DrawableId);
			Bitmap bitmap = Bitmap.CreateBitmap(gifDisplayDimensions, gifDisplayDimensions, Bitmap.Config.Argb8888);//remove if fawefawe
			Bitmap.Config bitmapConfig = bitmap.GetConfig ();
			if (bitmapConfig == null)
				bitmapConfig = Bitmap.Config.Argb8888;
			bitmap = bitmap.Copy (bitmapConfig, true);
			Canvas canvas = new Canvas (bitmap);
			Paint paint = new Paint (PaintFlags.AntiAlias);
			//Rect newRect = canvas.ClipBounds;
			//newRect.Inset (-600,-600);
			//canvas.ClipRect (newRect, Region.Op.Replace);
			foreach(GifarooTextview text in textList){
				paint.Color = new global::Android.Graphics.Color (text.CurrentTextColor);
				//get the correct percentage multiplier (will be multiplied by the size of the drawable in which the text is drawn)
				float sizePercentage = 0f;
				switch(text.sizeChoiceString){
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
				//float textSize = (second * 600f)*scale;
				float textSize = second * gifDisplayDimensions;
				Typeface tf = GifarooTextview.CreateTypeface (context, text.FontNameString);
				paint.SetTypeface (tf);
				paint.TextSize = textSize;


				Rect bounds = new Rect ();
				paint.GetTextBounds (text.Text, 0, text.Length(), bounds);

				//x axis positioning
				float xDim = (float)(context.Resources.DisplayMetrics.HeightPixels/2);
				float xs = (float)text.Left;
				float xResultsa = xs / xDim;
				float textViewWidth = ((float)text.Width/(float)xDim)*(float)bitmap.Width;
				float otherWidth = (float)bounds.Width();
				float leftPadding = (textViewWidth - otherWidth) / 2f;
				//float leftPadding = (otherWidth - textViewWidth) / 2f;
				float x = (float)((float)xResultsa * (float)bitmap.Width);//removed -leftpadding

				//y axis positioning
				float yDim = (float)(context.Resources.DisplayMetrics.HeightPixels/2f); 	//the height of the gifdispay
				float ys = (float)text.Bottom;												//the amount fo pixels from the bottom to the top
				float yResultsa = (float)ys / (float)yDim;							//divide the above 2
				float textViewHeight = ((float)text.Height/(float)yDim)*(float)bitmap.Height; //((heightoftext/heigthofgifdisplay)*heigthof"canvas") what the height of the text should be on the new image
				float otherHeight = (float)bounds.Height();							//heigth of rectbounds
				float bottomPadding = (textViewHeight - otherHeight) / 2f;			//
				float y = (float)((float)yResultsa * (float)bitmap.Height)-bottomPadding;

				canvas.DrawText (text.Text,x,y,paint);
			}

			return bitmap;
		}

		public static Bitmap DrawTextToBitmap2(global::Android.Content.Context context, List<GifarooTextview> textList, int DrawableId, int gifDisplayDimensions)
		{
			float scale = context.Resources.DisplayMetrics.Density;
			//Bitmap bitmap = BitmapFactory.DecodeResource (context.Resources, DrawableId);
			Bitmap bitmap = Bitmap.CreateBitmap(gifDisplayDimensions, gifDisplayDimensions, Bitmap.Config.Argb8888);//remove if fawefawe
			Bitmap.Config bitmapConfig = bitmap.GetConfig ();
			if (bitmapConfig == null)
				bitmapConfig = Bitmap.Config.Argb8888;
			bitmap = bitmap.Copy (bitmapConfig, true);
			Canvas canvas = new Canvas (bitmap);
			Paint paint = new Paint (PaintFlags.AntiAlias);
			//Rect newRect = canvas.ClipBounds;
			//newRect.Inset (-600,-600);
			//canvas.ClipRect (newRect, Region.Op.Replace);
			foreach(GifarooTextview text in textList){
				paint.Color = new global::Android.Graphics.Color (text.CurrentTextColor);
				//get the correct percentage multiplier (will be multiplied by the size of the drawable in which the text is drawn)
				float sizePercentage = 0f;
				switch(text.sizeChoiceString){
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
				//float textSize = (second * 600f)*scale;
				float textSize = second * gifDisplayDimensions;
				Typeface tf = GifarooTextview.CreateTypeface (context, text.FontNameString);
				paint.SetTypeface (tf);
				paint.TextSize = text.TextSize;
				//paint.ElegantTextHeight = true;	//Throws exception because this method does not exist or something.

				Rect bounds = new Rect ();
				paint.GetTextBounds (text.Text, 0, text.Length(), bounds);

				//x axis positioning
				float canvasSize = (float)(context.Resources.DisplayMetrics.HeightPixels/2);          //The size of the relativelayout and the canvas
				float xs = (float)text.Left + (float)text.PaddingLeft;                                                    //number of pixels to the left of the text view
				float xResultsa = xs / canvasSize;                                                    //
				float textViewWidth = ((float)text.Width/(float)canvasSize)*(float)bitmap.Width;
				float otherWidth = (float)bounds.Width();
				float leftPadding = (textViewWidth - otherWidth) / 2f;
				//float leftPadding = (otherWidth - textViewWidth) / 2f;
				float x = (float)((float)xResultsa * (float)bitmap.Width);						//removed -leftpadding

				//y axis positioning
				float ys = (float)text.Bottom;												//the amount of pixels from the bottom of the textview, to the bottom of the relativelayout.
                float yResultsa = (float)ys / (float)canvasSize;								//divide the above 2

				//bounds.Height(); returns the height of the text to be drawm ignoring the padding on the top and bottom of the text.
				//float bottomPadding = ((float)text.Height - (float)bounds.Height()) / 2f;		//TODO: this is where the y position problem comes from
				//bottomPadding = 0; //TODO: test, remove
				float y = (float)((float)yResultsa * (float)bitmap.Height) - text.PaddingBottom;

				//global::Android.Views.ViewGroup.MarginLayoutParams parameters = 
				//	(global::Android.Views.ViewGroup.MarginLayoutParams) text.LayoutParameters;
				//bottomPadding = parameters.BottomMargin;
				//float fff = (float)text.LineHeight/2f;
                //var RealDescent = text.Height - paint.Ascent() - bounds.Height();
				y = text.Bottom - paint.Descent() - text.PaddingBottom;
				canvas.DrawText (text.Text,x,y,paint);
			}

			return bitmap;
		}

		public static bool saveBitmapToDir(global::Android.Content.Context context, Bitmap bitmap, string pngName){
			global::Android.Content.ContextWrapper cw = new global::Android.Content.ContextWrapper (context);
			System.IO.Stream fos = null;
			try{
				fos = cw.OpenFileOutput (pngName, global::Android.Content.FileCreationMode.Private);
				bitmap.Compress(Bitmap.CompressFormat.Png, 100, fos);
				fos.Close();
			}catch(Exception e){
				throw e;
			}
			return true;
		}

		public static bool SavefileToInternalDir(File source, File targetFolder){
			InputStream inStream = new FileInputStream(source);
			OutputStream outStream = new FileOutputStream(targetFolder);
			byte[] buffer = new byte[1024];
			int len;
			while ((len = inStream.Read (buffer)) > 0) {
				outStream.Write (buffer, 0, len);
			}
			inStream.Close ();
			outStream.Close ();
			return true;
		}

        /// <summary>
        /// returns the provided dps in pixels.
        /// </summary>
        /// <param name="px">Pixels.</param>
        public static int dpsToPixels(int dps, global::Android.Content.Res.Resources r)
        {
            float scale = r.DisplayMetrics.Density;
            int pixels = (int)(dps * scale + 0.5f);
            return pixels;
        }

        public static int pixelsToDps(int px, global::Android.Content.Res.Resources r){
            int dps = (int)Math.Ceiling((r.DisplayMetrics.Xdpi / r.DisplayMetrics.Density) * px);
            return dps;
        }

	}
}