using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Com.Androidquery;
using Com.Androidquery.Callback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gifaroo.Android;

namespace ListViewLazyLoadingImages
{
	/// <summary>
	/// Awesome adapter that allows for lazy loading.
	/// </summary>
    public class LazyLoadAdapter : BaseAdapter
    {
        private	Activity _activity;
		private global::Android.Content.Context _context;
		private int _resource;
		private bool _packageOwned;
		private int _parentWidth;
        private Bitmap imgLoading = null;

		public int[] thumbIds;

		public LazyLoadAdapter(
			global::Android.Content.Context context,
			Activity activity,
			int[] ids,
			bool packageOwned,
			int resource){

            _activity = activity;
			this.thumbIds = ids;
			_context = context;
			_resource = resource;
			_packageOwned = packageOwned;
			_parentWidth = dps (40);
		}

        public override int Count
        {
			get { return thumbIds.Count(); }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

		private int dps(int dps){
			float scale = _context.Resources.DisplayMetrics.Density;
			int pixels = (int) (dps * scale + 0.5f);
			return pixels;
		}

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
            View listItem = convertView;
            ImageView mainImage;
            ImageView secondaryImage;

			if (listItem == null)
            {
				LayoutInflater inflater = LayoutInflater.From(_context);
				listItem = inflater.Inflate (_resource, null);
			}

			//string drawableUrl = Android.Net.Uri.Parse("android.resource://"+MainActivity.PACKAGE_NAME+"/" + thumbIds[position]).ToString();
			string drawableUrl = Android.Net.Uri.Parse("drawable://" + thumbIds[position]).ToString();

			mainImage = listItem.FindViewById <ImageView>(Resource.Id.mainImage);
			secondaryImage = listItem.FindViewById <ImageView>(Resource.Id.secondaryImage);
			secondaryImage.SetImageBitmap(null);
			mainImage.SetImageBitmap(null);

			RelativeLayout.LayoutParams imageViewParams = new RelativeLayout.LayoutParams (
				dps(40), dps(40));
			mainImage.LayoutParameters = imageViewParams;

			AQuery aq = new AQuery(mainImage);
			imgLoading = aq.GetCachedImage(Resource.Drawable.loading_placeholder);

			//show the lock if the premium pack is not owned and this is a premium bg
			if (_packageOwned == false
				&& position >= 5) {
				//secondaryImage = listItem.FindViewById <ImageView>(Resource.Id.secondaryImage);
				AQuery aqSecondary = new AQuery (secondaryImage);
				Bitmap lockImage = aqSecondary.GetCachedImage (Resource.Drawable.ic_action_gifaroo_plus_stamp);
				secondaryImage.SetImageBitmap (lockImage);
			}

			//will use "imgloading" as a place holder while the appropiate image is loaded
            if (aq.ShouldDelay(position, listItem, parent, drawableUrl)) {
				((AQuery)aq.Id(mainImage)).Image(imgLoading, 0.75f);
            }
            else {
				try{
					BitmapFactory.Options bmOptions = new BitmapFactory.Options();
					bmOptions.InSampleSize = 4;
					Bitmap BitmapInSampe = BitmapFactory.DecodeResourceAsync ( _context.Resources, thumbIds [position], bmOptions).Result;

					((AQuery)aq.Id(mainImage)).Image(new GifarooAjaxCallback(
						parent.Width,
						ref BitmapInSampe
					));
				}
				catch(Exception)
                {
					BitmapFactory.Options bmOptions = new BitmapFactory.Options();
					bmOptions.InSampleSize = 4;
                    try 
                    {
                        mainImage.SetImageBitmap(BitmapFactory.DecodeResourceAsync(_context.Resources, thumbIds[position], bmOptions).Result);
                    } catch (Exception)
                    {
                        //TODO:toast mesage
                    }
				}
			}
			return listItem;
        }

		/// <summary>
		/// Used by the LazyLaodAdapter in it's "GetView()" method.
		/// </summary>
		public class GifarooAjaxCallback : BitmapAjaxCallback {

			int _width;
			Bitmap _bm;
			ImageView imageViewOriginial;

			public GifarooAjaxCallback(int width, ref Bitmap bm){
				_width = width;
				_bm = bm;
			}

			/// <summary>
			/// Modifies the view before adding it to the gridview.
			/// </summary>
			protected override void InvokeCallback (string url, Android.Widget.ImageView iv, Android.Graphics.Bitmap bitmap, AjaxStatus status)
			{
				imageViewOriginial = iv;
				iv.LayoutParameters = new RelativeLayout.LayoutParams (WindowManagerLayoutParams.MatchParent, _width);
				iv.SetScaleType (Android.Widget.ImageView.ScaleType.FitXy);
				//iv.SetPadding (2, 2, 2, 2);

				//Matrix used to crop image
				Matrix matrix = new Matrix ();
				matrix.PostScale (0.5f, 0.5f);
				Bitmap finalBitmap = null;

				int size = _bm.Width;
				if (size >= 40) {
					size = 40;
				}

				finalBitmap = global::Android.Graphics.Bitmap.CreateBitmap (_bm, 0, 0, size, size, matrix, true);
				iv.SetImageBitmap (finalBitmap);
                _bm.Recycle();
			}
		}

    }

}
