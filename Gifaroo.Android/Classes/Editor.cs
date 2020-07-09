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
using Com.Stickerview;
using Android.Graphics;

namespace Editor
{
    public class TextStickerEditor
    {

        private List<XTextStickerView> _stickerTextViewList;
        public List<XTextStickerView> stickerTextViewList
        {
            get { return _stickerTextViewList; }
            set { _stickerTextViewList = value; }
        }

        private XTextStickerView _newTextView;
        public XTextStickerView newTextView
        {
            get {return _newTextView; }
            set { _newTextView = value; }
        }

        private XTextStickerView _textViewToEdit;
        public XTextStickerView textViewToEdit
        {
            get { return _textViewToEdit; }
            set { _textViewToEdit = value; }
        }

        public Bitmap deleteButtonBitamp;
        public Bitmap scalePanButtonBitmap;


        public TextStickerEditor()
        {
            _stickerTextViewList = new List<XTextStickerView>();
        }

        public Bitmap DrawTextToBitmap(global::Android.Content.Context context, int DrawableId, int gifDisplayDimensions, int actionBarHeight)
        {
            float scale = context.Resources.DisplayMetrics.Density;
            //Bitmap bitmap = BitmapFactory.DecodeResource (context.Resources, DrawableId);
            Bitmap bitmap = Bitmap.CreateBitmap(gifDisplayDimensions, gifDisplayDimensions, Bitmap.Config.Argb8888);//remove if fawefawe
            Bitmap.Config bitmapConfig = bitmap.GetConfig();
            if (bitmapConfig == null)
                bitmapConfig = Bitmap.Config.Argb8888;
            bitmap = bitmap.Copy(bitmapConfig, true);
            Canvas canvas = new Canvas(bitmap);
            Paint paint = new Paint(PaintFlags.AntiAlias);
            //Rect newRect = canvas.ClipBounds;
            //newRect.Inset (-600,-600);
            //canvas.ClipRect (newRect, Region.Op.Replace);
            foreach (XTextStickerView textSticker in this._stickerTextViewList)
            {
                RelativeLayout parent = (RelativeLayout)textSticker.Parent;

                paint.Color = new global::Android.Graphics.Color(textSticker.TvMain.CurrentTextColor);
                paint.ClearShadowLayer();

                Typeface tf = XTextStickerView.CreateTypeface(context, textSticker.fontStringName);
                paint.SetTypeface(tf);
                paint.TextSize = textSticker.TvMain.TextSize;
                //paint.ElegantTextHeight = true;	//Throws exception because this method does not exist or something.

                Rect bounds = new Rect();
                paint.GetTextBounds(textSticker.Text, 0, textSticker.TvMain.Length(), bounds);

                //x axis positioning
                float canvasSize = (float)(context.Resources.DisplayMetrics.HeightPixels / 2);          //The size of the relativelayout and the canvas
                
                float xs = (float)textSticker.TvMain.Left + (float)textSticker.TvMain.PaddingLeft;                                                    //number of pixels to the left of the text view
                float xResultsa = xs / canvasSize;                                                    //
                float textViewWidth = ((float)textSticker.Width / (float)canvasSize) * (float)bitmap.Width;
                float otherWidth = (float)bounds.Width();
                float leftPadding = (textViewWidth - otherWidth) / 2f;
                //float leftPadding = (otherWidth - textViewWidth) / 2f;
                //float x = (float)((float)xResultsa * (float)bitmap.Width);						//removed -leftpadding
                float x = (xs + textSticker.Left);

                x
                    = textSticker.TvMain.GetX()
                    + parent.GetX()
                    + ((RelativeLayout.LayoutParams)textSticker.LayoutParameters).LeftMargin;

                //y axis positioning
                //float ys = (float)textSticker.Bottom;												//the amount of pixels from the bottom of the textview, to the bottom of the relativelayout.
                //float yResultsa = (float)ys / (float)canvasSize;								//divide the above 2

                //bounds.Height(); returns the height of the text to be drawm ignoring the padding on the top and bottom of the text.
                //float bottomPadding = ((float)text.Height - (float)bounds.Height()) / 2f;		//TODO: this is where the y position problem comes from
                //bottomPadding = 0; //TODO: test, remove
                //float y = (float)((float)yResultsa * (float)bitmap.Height) - textSticker.PaddingBottom;

                //global::Android.Views.ViewGroup.MarginLayoutParams parameters = 
                //	(global::Android.Views.ViewGroup.MarginLayoutParams) text.LayoutParameters;
                //bottomPadding = parameters.BottomMargin;
                //float fff = (float)text.LineHeight/2f;
                //var RealDescent = text.Height - paint.Ascent() - bounds.Height();
                /*
                float y = (textSticker.Bottom
                    + textSticker.TvMain.Bottom
                    + textSticker.PaddingBottom)
                    + textSticker.TvMain.Top
                    - paint.Descent();
                 */

                float stickerY = textSticker.TvMain.GetY();
                float parentY = parent.GetY();
                float added = stickerY + parentY;
                RelativeLayout.LayoutParams lp = (RelativeLayout.LayoutParams)textSticker.LayoutParameters;

                float y
                    = textSticker.TvMain.GetY()
                    + parent.GetY()
                    + actionBarHeight
                    + textSticker.Height
                    //+ textSticker.TvMain.Top
                    - paint.Descent();
                y = context.Resources.DisplayMetrics.HeightPixels - y;
                canvas.DrawText(textSticker.TvMain.Text, x, y, paint);
            }

            return bitmap;
        }

        public void MoveItemToFrontOfList(XTextStickerView item)
        {
            stickerTextViewList.Add(item);
            stickerTextViewList.RemoveAt(stickerTextViewList.FindIndex(x => x == item));
        }
    }

    /// <summary>
    /// Awesome TextView that can be resized, spanned, scaled, and deleted.
    /// </summary>
    public class XTextStickerView : StickerTextView
    {

        private string _fontStringName;
        public string fontStringName
        {
            get { return _fontStringName; }
            set { _fontStringName = value; }
        }
        public int backgroundResource;

        public XTextStickerView(Context context)
            : base(context)
        {
            //this.RemoveView(IvFlip);
            this.ActionDownLaunchedListener = new XActionDownLaunchedListener(this);
            this.IvScale.BringToFront();
        }


        public void setTypefaceAndSaveString(Context context, string typefaceValue)
        {
            this._fontStringName = typefaceValue;
            this.TvMain.Typeface = CreateTypeface(context, typefaceValue);
        }

        public override void SetBackgroundResource(int resid)
        {
            this.backgroundResource = resid;
            base.SetBackgroundResource(resid);
        }
        public override void HideControls()
        {
            SetBackgroundResource(0);
            base.HideControls();
        }
        public override void ShowControlls()
        {
            SetBackgroundResource(backgroundResource);
            base.ShowControlls();
        }

        public static Typeface CreateTypeface(Context context, string typefaceValue)
        {
            //TypefaceStyle m_Style = TypefaceStyle.Normal;
            Typeface typeface = null;
            try
            {
                typeface = Typeface.CreateFromAsset(context.Assets, "fonts/" + typefaceValue + ".ttf");
            }
            catch (Exception e)
            {
                throw e;
            }
            return typeface;
        }

        public class XActionDownLaunchedListener : Java.Lang.Object, IActionDownLaunchedListener
        {
            public event EventHandler EActionDownLaunched;
            public XTextStickerView parentStickerTextview;

            public XActionDownLaunchedListener(XTextStickerView parent) : base()
            {
                this.parentStickerTextview = parent;
            }

            public void ActionDownLaunched()
            {
                if (EActionDownLaunched != null)
                    EActionDownLaunched(this, EventArgs.Empty);
            }
        }

    }

}