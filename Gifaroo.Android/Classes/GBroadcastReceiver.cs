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

namespace Gifaroo.Android.Classes
{
    [BroadcastReceiver(Enabled=true)]
    [IntentFilter(new[] { "com.android.vending.billing.PURCHASES_UPDATED" })]
    class GBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //Toast.MakeText(context, "Content should now be unlocked =)", ToastLength.Long);
            var nMgr = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var notification = new Notification(Resource.Mipmap.ic_launcher, "Content unlocked!");
            var pendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0);
            notification.SetLatestEventInfo(context, "Content unlocked", "New content is now available!", pendingIntent);
            nMgr.Notify(0, notification);
        }
    }
}