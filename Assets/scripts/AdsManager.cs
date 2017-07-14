using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour {

    public static AdsManager Global;

    string[] zoneIds = new string[] { "vz71ad6974431c45ecb4", "vzc2700ed0702e49e184"};

    public bool TestMode;

    bool AdsConfigured = false;
    bool CallbacksRegistered = false;

    bool AdsReady {
        get {
            if (AdsConfigured && CallbacksRegistered) {
                return true;
            }
            if (!AdsConfigured) {
                ConfigureAds();
            }
            if (!CallbacksRegistered) {
                RegisterForAdsCallbacks();
            }
            return false;
        }
    }

    void Awake() {
        Global = this;
    }

    // Use this for initialization
    void Start () {
        #if UNITY_ANDROID
        Debug.Log("Android");
        
        ConfigureAds();
        RegisterForAdsCallbacks();

        if (TestMode)
        {
            StartCoroutine("InfiniteTestAd");
        }
        #endif
    }

    IEnumerator InfiniteTestAd()
    {
        RequestAd();
        yield return new WaitForSeconds(10f);
    }

    void ConfigureAds()
    {
        #if (UNITY_ANDROID || UNITY_IOS)
        AdColony.AppOptions appOptions = new AdColony.AppOptions();
        appOptions.AdOrientation = AdColony.AdOrientationType.AdColonyOrientationAll;

        AdColony.Ads.Configure("appa8afa95ab01f4b7e82", appOptions, zoneIds);

        AdsConfigured = true;

        #else
        Debug.LogWarning("Ads not supported on this platform");
        #endif
    }

    void RegisterForAdsCallbacks()
    {
        AdColony.Ads.OnRequestInterstitial += PlayAd;

        AdColony.Ads.OnExpiring += RequestAd;

        AdColony.Ads.OnRewardGranted += OnRewardGranted;

        CallbacksRegistered = true;
    }

    /// <summary>
    /// Request new ad.
    /// </summary>
    public void RequestAd()
    {
        #if (UNITY_ANDROID || UNITY_IOS)
        if (AdsReady) {
        AdColony.Ads.RequestInterstitialAd(zoneIds[0], null);
        }
        #else
        Debug.LogWarning("Ads not supported on this platform");
        #endif
    }

    /// <summary>
    ///  Request ads if previous request failed
    /// </summary>
    /// <param name="ad">Failed ad</param>
    void RequestAd(AdColony.InterstitialAd ad)
    {
        #if (UNITY_ANDROID || UNITY_IOS)
        if (AdsReady) {
        AdColony.Ads.RequestInterstitialAd(ad.ZoneId, null);
        }
        #else
        Debug.LogWarning("Ads not supported on this platform");
        #endif
    }


    void RequestRewardedAd()
    {
        #if (UNITY_ANDROID || UNITY_IOS)
        AdColony.AdOptions adOptions = new AdColony.AdOptions();
        adOptions.ShowPrePopup = true;
        adOptions.ShowPostPopup = true;

        if (AdsReady) {
        AdColony.Ads.RequestInterstitialAd(zoneIds[1], adOptions);
        }
        #else
        Debug.LogWarning("Ads not supported on this platform");
        #endif
    }

    void OnRewardGranted(string zoneId, bool success, string name, int amount)
    {

    }

    void PlayAd(AdColony.InterstitialAd _ad)
    {
        if (_ad != null)
        {
            AdColony.Ads.ShowAd(_ad);
        }
    }
}
