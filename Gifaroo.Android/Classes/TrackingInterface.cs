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
using Android.Gms.Analytics;
using Android.Gms.Analytics.Ecommerce;

namespace Gifaroo.Android.Classes
{
    public class TrackingInterface
    {
        //public fields
        public string trackingId = "UA-74094728-1";
#if Debug
        trackingId = "none";
#endif

        //private fields
        private static GoogleAnalytics _GAInstance;
        private static Tracker _GATracker;

        //Constructor
        public TrackingInterface (Context appContext) {
            _GAInstance = GoogleAnalytics.GetInstance(appContext);
            _GATracker = _GAInstance.NewTracker(trackingId);
            _GATracker.EnableExceptionReporting(true);
            _GATracker.EnableAdvertisingIdCollection(true);
            _GATracker.EnableAutoActivityTracking(true);
            _GATracker.EnableAdvertisingIdCollection(true);
        }

        #region Methods
        public void TrackScreen(String pageName)
        {
            _GATracker.SetScreenName(pageName);
            _GATracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        public static class ActivityNames {
            public static string MainActivity = "Web Browser Activity";
            public static string EditGif = "Video File Editor Activity";
            public static string SaveActivity = "Save File Activity";
        }

        public void TrackEditorItemsUsed(string BackGroundPackName, int position, List<GifarooTextview> TextViewList) {
            var eventBuilder = new HitBuilders.EventBuilder()
                .SetCategory("Editor")
                .SetLabel(BackGroundPackName);

            foreach (GifarooTextview textView in TextViewList) {
                eventBuilder.SetLabel(textView.FontNameString);
            }

            _GATracker.Send(eventBuilder.Build());
        }

        public void TrackStoreVisit(string source) {
            _GATracker.Send(
                new HitBuilders.EventBuilder()
                .SetCategory(Categories.Advertising)
                .SetAction("Store opened")
                .SetLabel(source)
                .Build());
        }

        public void TrackRewardedVideoPrompt(string productToBeUnlocked, bool accepted) {
            _GATracker.Send(
                new HitBuilders.EventBuilder()
                .SetCategory(Categories.Advertising)
                .SetAction("Responded to video prompt.")
                .SetLabel( (accepted) ? "Accepted" : "Declined" )
                .Build());
        }
        public void TrackRewardedVideoResult(bool visitedAdvertiser){
            _GATracker.Send(
                new HitBuilders.EventBuilder()
                .SetCategory(Categories.Advertising)
                .SetAction((visitedAdvertiser) ? "Visited Advertiser." : "Did not Visit Advertiser.")
                .SetLabel("Rewarded Video Finished")
                .Build());
        }

        public void TrackItemPurchase( string productId, string category, string name, double price) {
            Product p = new Product()
            .SetId(productId)
            .SetCategory(category)
            .SetName(name)
            .SetPrice(price);

            HitBuilders.ScreenViewBuilder builder = new HitBuilders.ScreenViewBuilder();
            builder.AddProduct(p);
            builder.SetProductAction(new ProductAction(ProductAction.ActionPurchase));
            _GATracker.Send(builder.Build());
        }

        public void TrackException(String message, bool fatal) {
            if(false) //manually disabled handled exception reporting
            _GATracker.Send(
                new HitBuilders.ExceptionBuilder()
                .SetDescription(message)
                .SetFatal(fatal)
                .Build());
        }

        public void TrackFffmgFailure(
            bool loadFailure,
            int commandNumber = 0){

            //set device and os info here
            HitBuilders.EventBuilder eventBuilder = new HitBuilders.EventBuilder();
            eventBuilder.SetCategory(Categories.ffmpeg);

            if (loadFailure){ //load failure
                eventBuilder.SetLabel("ffmpeg load attempt failed");
            } else { //command execution failure
                eventBuilder.SetLabel("ffmpeg execution failed");
                eventBuilder.SetValue(commandNumber);
            }
            _GATracker.Send(eventBuilder.Build());
        }

        public void TrackSharingApp(String Appname) {
            _GATracker.Send(
                new HitBuilders.EventBuilder(
                    "Sharing", "File sent to")
                    .SetLabel(Appname)
                    .Build());
        }

        /// <summary>
        /// Sends source URL as tracking data to Google Analytics.
        /// </summary>
        /// <param name="domain">Source Domain</param>
        public void TrackFileSorceDomain(String domain){
            _GATracker.Send(new HitBuilders.EventBuilder()
                .SetCategory(Categories.userFlow)
                .SetAction("File Downloaded")
                .SetLabel(domain)
                .Build());
        }
        #endregion

        private static class Categories {
            public static string Billing = "Billing";
            public static string Advertising = "Advertising";
            public static string ffmpeg = "ffmpeg";
            public static string userFlow = "userFlow";
        }

    }
}