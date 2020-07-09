using System;
using System.Collections.Generic;
using Android.Widget;

using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;

namespace Gifaroo.Android
{
	/// <summary>
	/// Stores products and skus
	/// </summary>
	public class PremiumInterface: Java.Lang.Object{
		//private fields
		private Dictionary<string, StoreItem> _ProductsDictionary = null;
		private global::Android.Content.Context _context = null;

		#region fields
		/// <summary>
		/// A dictionary that stores the products using the skus as key values.
		/// The products are intially null and need to be assigned.
		/// ProductsDictionary[skuString] = productForThatSKU;
		/// </summary>
		public Dictionary <string, StoreItem> ProductsDictionary {
			get{ return this._ProductsDictionary; }
			set{ this._ProductsDictionary = value; }
		}
		public UserPremiunStatus userPremiumStatus = new UserPremiunStatus();
        public bool InventorySynched = false;
		public InAppBillingServiceConnection serviceConnection = null;
		public string SKUForTheLastFreeUnlockButtonSelected = null;

        ///TODO: Add the real keys for testing and deployment.
        static public string kewefawe = "FALSE KEY";
            //obfuscated key
        static public string ObsPlayKey = "FALSE KEY";

		static public string leadboltKey = "FALSE KEY";
		#endregion

		//cosntructor
		public PremiumInterface(
			global::Android.Content.Context context,
			InAppBillingServiceConnection serviceConnection){
			this.serviceConnection = serviceConnection;
			this._context = context;
			_ProductsDictionary = new Dictionary<string, StoreItem> (){
				{PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU,	new StoreItem(context, serviceConnection, "Blurs & Patterns Background Bundle")},
				{PremiumInterface.SKUs.Gradients_Night_BGPack_SKU,	new StoreItem(context, serviceConnection, "Gradient Nights Background Bundle")},
				{PremiumInterface.SKUs.Fonts_Pack_SKU,				new StoreItem(context, serviceConnection, "All Fonts Pack")}
			};
		}

		#region methods
		/// <summary>
		/// Fills the dictionary's StoreItems with the Purchase items owned by the user as well as setting the
		/// owned bool as true if the StoreItem is owned.
		/// </summary>
		/// <param name="_serviceConnection">A connected service connection.</param>
		public void SyncPurchases (){
			IList<Purchase> _purchases = serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
			try{
				foreach (Purchase purchase in _purchases){
                    //The following only runs if one of the products is owned.
                    //SyncPurhcases() is called whne the activity starts and when a store item is purchased from the store.
					_ProductsDictionary[purchase.ProductId].purchase = purchase;
					_ProductsDictionary[purchase.ProductId].owned = true;

                    //Give the user premium status only if they have bought a background pack
                    //TODO: save this to solid sate
                    if(purchase.ProductId != PremiumInterface.SKUs.Fonts_Pack_SKU)
                        this.userPremiumStatus.status = UserPremiunStatus.StatusFlags.STATUS_PREMIUM;
				}
			} catch (Exception ex) {}
		}
		/// <summary>
		/// Fills the dictionary's StoreItems with the Google Play products.
		/// </summary>
		/// <param name="_serviceConnection">Service connection.</param>
		public void SyncInventory (){
			IList<Product> _products = serviceConnection.BillingHandler.QueryInventoryAsync(
				new List<string>{
					PremiumInterface.SKUs.Fonts_Pack_SKU,
					PremiumInterface.SKUs.Blurs_patterns_BGPack_SKU,
					PremiumInterface.SKUs.Gradients_Night_BGPack_SKU,
				},
				ItemType.Product
			).Result;

			//turn Synched into true if the inventory was synched properly.
			if (_products.Count != 0) {
				InventorySynched = true;
			}

			//add pruchases to the diccitonary for easy access
			try {
				foreach(Product product in _products){
					_ProductsDictionary[product.ProductId].product = product;
				}
			} catch (Exception ex) {}
		}
		#endregion

		//classes
		/// <summary>
		/// used to store a product with a purchase, initialized in the dictionary above
		/// </summary>
		public class StoreItem{
			public Product	product = null;
			public Purchase	purchase = null;
			public bool 	owned = false;
			public bool 	temporarilyOwned = false;
            public string   itemName = "";

			public Button	purchaseButton = null;
			public Button	freeUnlockButton = null;
			public TextView	descriptiontext = null;
			private InAppBillingServiceConnection _serviceConnection = null;
			private global::Android.Content.Context _context;

			/// <summary>
			/// Initializes a new instance of the <see cref="Gifaroo.Android.BillingInventory+StoreItem"/> class.
			/// </summary>
			/// <param name="itemProduct">Item product.</param>
			/// <param name="itemPurchase">Item purchase.</param>
			public StoreItem(
				global::Android.Content.Context context,
				InAppBillingServiceConnection serviceConnection,
                String itemName){
				this._serviceConnection = serviceConnection;
				this._context = context;
                this.itemName = itemName;
			}

			public void PurchaseButtonCLicked (object sender, EventArgs e){
				try{
					_serviceConnection.BillingHandler.BuyProduct(product);
				}
				catch(Exception){
					Toast.MakeText (_context,
						"There was a problem trying to connect to the Google Play store, please check your connection and log in to your Google account.",
						ToastLength.Long).Show();
				}
			}
		}
		//TODO: add real skus
		public static class SKUs{
			//public static string Gradients_Night_BGPack_SKU = ReservedTestProductIDs.Canceled;
			//public static string Blurs_patterns_BGPack_SKU = ReservedTestProductIDs.Purchased;
			//public static string Fonts_Pack_SKU = ReservedTestProductIDs.Refunded;

            public static string Gradients_Night_BGPack_SKU = "bundle.gradient_night";
            public static string Blurs_patterns_BGPack_SKU = "bundle.blurs_patterns";
            public static string Fonts_Pack_SKU = "bundle.unlock_all_fonts";
		}

        public class UserPremiunStatus {
            private string _Status;

            public string status
            {
                get { return _Status; }
                set { _Status = value; }
            }

            public UserPremiunStatus()
            {
                this._Status = StatusFlags.STATUS_FREE; 
            }

            public static class StatusFlags
            {
                public static string STATUS_PREMIUM = "STATUS_PREMIUM";
                public static string STATUS_FREE = "STATUS_FREE";
                public static string ACTIVITY_FLAG = "PREMIUM_STATUS_FLAG";
            }
            
        }

	}
}

