using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AdColony {

    /// <summary>
    /// Main class for Ads within AdColony SDK
    /// </summary>
    /// <example>
    /// <code>
    ///    AdColony.InterstitialAd _ad = null;
    ///    bool _configured = false;
    ///
    ///    void ConfigureAds() {
    ///        AdColony.Ads.Configure("your_app_id", null, "your_zone_1", "your_zone_2");
    ///
    ///        AdColony.Ads.OnConfigurationCompleted += (List<AdColony.Zone> zones) => {
    ///            if (zones == null || zones.Count <= 0) {
    ///                _configured = false;
    ///            } else {
    ///                _configured = true;
    ///            }
    ///        };
    ///
    ///        AdColony.Ads.OnRequestInterstitial += (AdColony.InterstitialAd ad) => {
    ///            _ad = ad;
    ///        };
    ///    }
    ///
    ///    void RequestAd() {
    ///        if (_configured) {
    ///            AdColony.Ads.RequestInterstitialAd("your_zone_1", null);
    ///        }
    ///    }
    ///
    ///    void PlayAd() {
    ///        if (_ad != null) {
    ///            AdColony.Ads.ShowAd(_ad);
    ///        }
    ///    }
    /// </code>
    /// </example>
    public class Ads : MonoBehaviour {

        /// <summary>
        /// Event that is triggered after a call to Configure has completed.
        /// </summary>
        /// On Android, the Zone objects aren't fully populated by the time this is called, use GetZone() after a delay if required.
        /// If the configuration is not successful, the list of zones will be empty
        public static event Action<List<Zone>> OnConfigurationCompleted;

        /// <summary>
        /// Event that is triggered after a call to RequestInterstitialAd has completed successfully. The InterstitialAd object returned can be used to show an interstitial ad when ready.
        /// </summary>
        public static event Action<InterstitialAd> OnRequestInterstitial;

        /// <summary>
        /// Event triggered after a call to RequestInterstitialAd has failed.
        /// Parameter 1: zoneId
        /// </summary>
        public static event Action<string> OnRequestInterstitialFailedWithZone;

        /// <summary>
        /// Event triggered after a custom message is received by the SDK.
        /// Parameter 1: type
        /// Parameter 2: message
        /// </summary>
        public static event Action<string, string> OnCustomMessageReceived;

        /// <summary>
        /// Event triggered after an InterstitialAd is opened.
        /// </summary>
        public static event Action<InterstitialAd> OnOpened;

        /// <summary>
        /// Event triggered after an InterstitialAd is closed.
        /// </summary>
        public static event Action<InterstitialAd> OnClosed;

        /// <summary>
        /// Event triggered after an InterstitialAd has been marked to expire.
        /// </summary>
        public static event Action<InterstitialAd> OnExpiring;

        /// <summary>
        /// Event triggered after an InterstitialAd's audio has started.
        /// </summary>
        public static event Action<InterstitialAd> OnAudioStarted;

        /// <summary>
        /// Event triggered after an InterstitialAd's audio has stopped.
        /// </summary>
        public static event Action<InterstitialAd> OnAudioStopped;

        /// <summary>
        /// Event triggered after the ad triggers an IAP opportunity.
        /// Parameter 1: ad
        /// Parameter 2: IAP product ID
        /// Parameter 3: engagement type
        /// </summary>
        public static event Action<InterstitialAd, string, AdsIAPEngagementType> OnIAPOpportunity;

        // Params: zoneId, success, name, amount
        /// <summary>
        /// Event triggered after V4VC ad has been completed.
        /// Client-side reward implementations should consider incrementing the user's currency balance in this method.
        /// Server-side reward implementations should consider the success parameter and then contact the game server to determine the current total balance for the virtual currency.
        /// Parameter 1: zone ID
        /// Parameter 2: success
        /// Parameter 3: name of reward type
        /// Parameter 4: reward quantity
        /// </summary>
        public static event Action<string, bool, string, int> OnRewardGranted;

        // ---------------------------------------------------------------------------

        /// <summary>
        /// Configures AdColony specifically for your app; required for usage of the rest of the API.
        /// </summary>
        /// This method returns immediately; any long-running work such as network connections are performed in the background.
        /// AdColony does not begin preparing ads for display or performing reporting until after it is configured by your app.
        /// The required appId and zoneIds parameters for this method can be created and retrieved at the [Control Panel](http://clients.adcolony.com).
        /// If either of these are `null`, app will be unable to play ads and AdColony will only provide limited reporting and install-tracking functionality.
        /// You should not start requesting ads until `OnConfigurationCompleted` event has fired.
        /// If there is a configuration error, the set of zones passed to the completion handler will be null.
        /// <param name="appId">The AdColony app ID for your app.</param>
        /// <param name="zoneIds">An array of at least one AdColony zone ID string.</param>
        /// <param name="options">(optional) Configuration options for your app.</param>
        /// <see cref="AppOptions" />
        /// <see cref="Zone" />
        /// <see cref="OnConfigurationCompleted" />
        public static void Configure(string appId, AppOptions options, params string[] zoneIds) {
            // Using SharedInstance to make sure the MonoBehaviour is instantiated
            if (SharedInstance == null) {
                Debug.LogWarning(Constants.AdsMessageSDKUnavailable);
                return;
            }
            SharedInstance.Configure(appId, options, zoneIds);
            _initialized = true;
        }

        /// <summary>
        /// Requests an AdColonyInterstitial.
        /// </summary>
        /// This method returns immediately, before the ad request completes.
        /// If the request is successful, an AdColonyInterstitial object will be passed to the success block.
        /// If the request is unsuccessful, the failure block will be called and an AdColonyAdRequestError will be passed to the handler.
        /// <param name="zoneId">The zone identifier string indicating which zone the ad request is for.</param>
        /// <param name="adOptions">An AdOptions object used to set configurable aspects of the ad request.</param>
        /// <see cref="AdOptions" />
        /// <see cref="InterstitialAd" />
        /// <see cref="OnRequestInterstitial" />
        /// <see cref="OnRequestInterstitialFailedWithZone" />
        public static void RequestInterstitialAd(string zoneId, AdOptions adOptions) {
            if (IsInitialized()) {
                SharedInstance.RequestInterstitialAd(zoneId, adOptions);
            }
        }

        /// <summary>
        /// Shows an interstitial ad.
        /// </summary>
        /// <param name="ad">The interstitial ad returned from RequestInterstitialAd.</param>
        /// <see cref="OnOpened" />
        /// <see cref="OnClosed" />
        /// <see cref="OnExpiring" />
        /// <see cref="OnAudioStarted" />
        /// <see cref="OnAudioStopped" />
        public static void ShowAd(InterstitialAd ad) {
            if (IsInitialized()) {
                SharedInstance.ShowAd(ad);
            }
        }

        /// <summary>
        /// Sets the current, global set of AppOptions.
        /// </summary>
        /// Use the object's option-setting methods to configure currently-supported options.
        /// <param name="options">The AppOptions object to be used for configuring global options such as a custom user identifier.</param>
        /// <see cref="AppOptions" />
        public static void SetAppOptions(AppOptions options) {
            if (IsInitialized()) {
                SharedInstance.SetAppOptions(options);
            }
        }

        /// <summary>
        /// Returns the current, global set of AppOptions.
        /// </summary>
        /// Use this method to obtain the current set of app options used to configure SDK behavior.
        /// If no options object has been set, this method will return `null`.
        /// <returns>The current AppOptions object being used by the SDK.</returns>
        /// <see cref="AppOptions" />
        public static AppOptions GetAppOptions() {
            if (IsInitialized()) {
                return SharedInstance.GetAppOptions();
            }
            return null;
        }

        /// <summary>
        /// Retrieve a string-based representation of the SDK version.
        /// </summary>
        /// The returned string will be in the form of "<Major Version>.<Minor Version>.<External Revision>.<Internal Revision>"
        /// <returns>The current AdColony SDK version string.</returns>
        public static string GetSDKVersion() {
            if (IsInitialized()) {
                return SharedInstance.GetSDKVersion();
            }
            return null;
        }

        // Asynchronously request an Interstitial Ad
        // see OnRequestInterstitial, OnRequestInterstitialFailed

        /// <summary>
        /// Retrieves a Zone object.
        /// </summary>
        /// AdColonyZone objects aggregate informative data about unique AdColony zones such as their identifiers, whether or not they are configured for rewards, etc.
        /// AdColony zone IDs can be created and retrieved at the [Control Panel](http://clients.adcolony.com).
        /// <param name="zoneId">The AdColony zone identifier string indicating which zone to return.</param>
        /// <returns>A Zone object. Returns `null` if an invalid zone ID is passed.</returns>
        /// <see cref="Zone" />
        public static Zone GetZone(string zoneId) {
            if (IsInitialized()) {
                return SharedInstance.GetZone(zoneId);
            }
            return null;
        }

        /// <summary>
        /// Retrieves a custom identifier for the current user if it has been set.
        /// </summary>
        /// This is an arbitrary, application-specific identifier string for the current user.
        /// To configure this identifier, use the `AppOptions.UserId` property of the object passed to `Configure()`.
        /// Note that if this method is called before `Configure()`, it will return an empty string.
        /// <returns>The identifier for the current user.</returns>
        /// <see cref="AppOptions" />
        public static string GetUserID() {
            if (IsInitialized()) {
                return SharedInstance.GetUserID();
            }
            return null;
        }

        /// <summary>
        /// Sends a custom message to the AdColony SDK.
        /// </summary>
        /// Use this method to send custom messages to the AdColony SDK.
        /// <param name="type">The type of the custom message. Must be 128 characters or less.</param>
        /// <param name="content">The content of the custom message. Must be 1024 characters or less.</param>
        /// <param name="reply">A block of code to be executed when a reply is sent to the custom message.</param>
        public static void SendCustomMessage(string type, string content) {
            if (IsInitialized()) {
                SharedInstance.SendCustomMessage(type, content);
            }
        }

        /// <summary>
        /// Reports IAPs within your application.
        /// </summary>
        /// Note that this API can be used to report standard IAPs as well as those triggered by AdColony’s IAP Promo (IAPP) advertisements.
        /// Leveraging this API will improve overall ad targeting for your application.
        /// <param name="transactionID">An string representing the unique SKPaymentTransaction identifier for the IAP. Must be 128 chars or less.</param>
        /// <param name="productID">An string identifying the purchased product. Must be 128 chars or less.</param>
        /// <param name="price">(optional) Total price of the items purchased in micro-cents, $0.99 = 990000</param>
        /// <param name="currencyCode">(optional) An string indicating the real-world, three-letter ISO 4217 (e.g. USD) currency code of the transaction.</param>
        public static void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode) {
            if (IsInitialized()) {
                SharedInstance.LogInAppPurchase(transactionId, productId, purchasePriceMicro, currencyCode);
            }
        }

        /// <summary>
        /// Cancels the interstitial and returns control back to the application.
        /// </summary>
        /// Call this method to cancel the interstitial. Canceling interstitials before they finish will diminish publisher revenue.
        /// Note this has no affect on Android.
        /// <param name="ad">The interstitial ad returned from RequestInterstitialAd.</param>
        public static void CancelAd(InterstitialAd ad) {
            if (IsInitialized()) {
                SharedInstance.CancelAd(ad);
            }
        }

#region Internal Methods - do not call these

        public static Ads SharedGameObject {
            get {
                if (!_sharedGameObject) {
                    _sharedGameObject = (Ads)FindObjectOfType(typeof(Ads));
                }

                if (!_sharedGameObject) {
                    GameObject singleton = new GameObject();
                    _sharedGameObject = singleton.AddComponent<Ads>();
                    singleton.name = Constants.AdsManagerName;
                    DontDestroyOnLoad(singleton);

                    if (_sharedGameObject._sharedInstance != null) {
                        Debug.LogWarning(Constants.AdsMessageAlreadyInitialized);
                    } else {
                        _sharedGameObject._sharedInstance = null;
#if UNITY_EDITOR

#elif UNITY_ANDROID
                        _sharedGameObject._sharedInstance = new AdsAndroid(singleton.name);
#elif UNITY_IOS
                        _sharedGameObject._sharedInstance = new AdsIOS(singleton.name);
#elif UNITY_WP8

#elif UNITY_METRO

#endif
                    }
                }

                return _sharedGameObject;
            }
        }

        private static IAds SharedInstance {
            get {
                IAds ret = null;
                Ads gameObject = SharedGameObject;
                if (gameObject != null) {
                    ret = gameObject._sharedInstance;
                }
                if (ret == null) {
                    Debug.LogError("Platform-specific implemenation not set");
                }
                return ret;
            }
        }

        private static bool IsSupportedOnCurrentPlatform() {
            // Using SharedInstance to make sure the MonoBehaviour is instantiated
            if (SharedInstance == null) {
                return false;
            }
            return true;
        }

        private static bool IsInitialized() {
            if (!IsSupportedOnCurrentPlatform()) {
                return false;
            } else if (!_initialized) {
                Debug.LogError(Constants.AdsMessageNotInitialized);
                return false;
            }
            return true;
        }

        void Awake() {
            if (gameObject == SharedGameObject.gameObject) {
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        void Update() {
            if (_updateOnMainThreadActions.Count > 0) {
                System.Action action;
                do {
                    action = null;
                    lock (_updateOnMainThreadActionsLock) {
                        if (_updateOnMainThreadActions.Count > 0) {
                            action = _updateOnMainThreadActions.Dequeue();
                        }
                    }
                    if (action != null) {
                        action.Invoke();
                    }
                } while (action != null);
            }
        }

        public void EnqueueAction(System.Action action) {
            lock (_updateOnMainThreadActionsLock) {
                _updateOnMainThreadActions.Enqueue(action);
            }
        }

        public static void DestroyAd(String id) {
            if (IsInitialized()) {
                SharedInstance.DestroyAd(id);
            }
        }

        public void _OnConfigure(string paramJson) {
            List<Zone> zoneList = new List<Zone>();
            ArrayList zoneJsonList = (AdColonyJson.Decode(paramJson) as ArrayList);
            foreach (string zoneJson in zoneJsonList) {
                Hashtable zoneValues = (AdColonyJson.Decode(zoneJson) as Hashtable);
                zoneList.Add(new Zone(zoneValues));
            }

            if (Ads.OnConfigurationCompleted != null) {
                Ads.OnConfigurationCompleted(zoneList);
            }
        }


        public void _OnRequestInterstitial(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnRequestInterstitial != null) {
                Ads.OnRequestInterstitial(ad);
            }
        }

        public void _OnRequestInterstitialFailed(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            string zoneId = "";
            if (values != null && values.ContainsKey("zone_id")) {
                zoneId = values["zone_id"] as string;
            }

            if (Ads.OnRequestInterstitialFailedWithZone != null) {
                Ads.OnRequestInterstitialFailedWithZone(zoneId);
            }
        }

        public void _OnOpened(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnOpened != null) {
                Ads.OnOpened(ad);
            }
        }

        public void _OnClosed(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnClosed != null) {
                Ads.OnClosed(ad);
            }

            _ads.Remove(ad.Id);
        }

        public void _OnExpiring(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnExpiring != null) {
                Ads.OnExpiring(ad);
            }

            _ads.Remove(ad.Id);
        }

        public void _OnAudioStarted(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnAudioStarted != null) {
                Ads.OnAudioStarted(ad);
            }
        }

        public void _OnAudioStopped(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            InterstitialAd ad = GetAdFromHashtable(values);

            if (Ads.OnAudioStopped != null) {
                Ads.OnAudioStopped(ad);
            }
        }

        public void _OnIAPOpportunity(string paramJson) {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            Hashtable valuesAd = null;
            string iapProductId = null;
            AdsIAPEngagementType engagement = AdsIAPEngagementType.AdColonyIAPEngagementEndCard;

            if (values.ContainsKey(Constants.OnIAPOpportunityAdKey)) {
                valuesAd = values[Constants.OnIAPOpportunityAdKey] as Hashtable;
            }
            if (values.ContainsKey(Constants.OnIAPOpportunityEngagementKey)) {
                engagement = (AdsIAPEngagementType)Convert.ToInt32(values[Constants.OnIAPOpportunityEngagementKey]);
            }
            if (values.ContainsKey(Constants.OnIAPOpportunityIapProductIdKey)) {
                iapProductId = values[Constants.OnIAPOpportunityIapProductIdKey] as string;
            }

            InterstitialAd ad = GetAdFromHashtable(valuesAd);

            if (Ads.OnIAPOpportunity != null) {
                Ads.OnIAPOpportunity(ad, iapProductId, engagement);
            }
        }

        public void _OnRewardGranted(string paramJson) {
            string zoneId = null;
            bool success = false;
            string productId = null;
            int amount = 0;

            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values != null) {
                if (values.ContainsKey(Constants.OnRewardGrantedZoneIdKey)) {
                    zoneId = values[Constants.OnRewardGrantedZoneIdKey] as string;
                }
                if (values.ContainsKey(Constants.OnRewardGrantedSuccessKey)) {
                    success = Convert.ToBoolean(Convert.ToInt32(values[Constants.OnRewardGrantedSuccessKey]));
                }
                if (values.ContainsKey(Constants.OnRewardGrantedNameKey)) {
                    productId = values[Constants.OnRewardGrantedNameKey] as string;
                }
                if (values.ContainsKey(Constants.OnRewardGrantedAmountKey)) {
                    amount = Convert.ToInt32(values[Constants.OnRewardGrantedAmountKey]);
                }
            }

            if (Ads.OnRewardGranted != null) {
                Ads.OnRewardGranted(zoneId, success, productId, amount);
            }
        }

        public void _OnCustomMessageReceived(string paramJson) {
            string type = null;
            string message = null;

            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values != null) {
                if (values.ContainsKey(Constants.OnCustomMessageReceivedTypeKey)) {
                    type = values[Constants.OnCustomMessageReceivedTypeKey] as string;
                }
                if (values.ContainsKey(Constants.OnCustomMessageReceivedMessageKey)) {
                    message = values[Constants.OnCustomMessageReceivedMessageKey] as string;
                }
            }

            if (Ads.OnCustomMessageReceived != null) {
                Ads.OnCustomMessageReceived(type, message);
            }
        }

        private InterstitialAd GetAdFromHashtable(Hashtable values) {
            string id = null;
            if (values != null && values.ContainsKey("id")) {
                id = values["id"] as string;
            }

            InterstitialAd ad = null;
            if (id != null) {
                if (_ads.ContainsKey(id)) {
                    ad = _ads[id];
                } else {
                    ad = new InterstitialAd(values);
                    _ads[id] = ad;
                }
            }
            return ad;
        }

        private Dictionary<string, InterstitialAd> _ads = new Dictionary<string, InterstitialAd>();
        private static Ads _sharedGameObject;
        private static bool _initialized = false;
        private IAds _sharedInstance = null;
        private System.Object _updateOnMainThreadActionsLock = new System.Object();
        private readonly Queue<System.Action> _updateOnMainThreadActions = new Queue<System.Action>();

#endregion

    }

    public interface IAds {
        void Configure(string appId, AppOptions options, params string[] zoneIds);
        string GetSDKVersion();
        void RequestInterstitialAd(string zoneId, AdOptions adOptions);
        Zone GetZone(string zoneId);
        string GetUserID();
        void SetAppOptions(AppOptions options);
        AppOptions GetAppOptions();
        void SendCustomMessage(string type, string content);
        void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode);
        void ShowAd(InterstitialAd ad);
        void CancelAd(InterstitialAd ad);
        void DestroyAd(string id);
    }
}
