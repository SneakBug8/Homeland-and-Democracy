using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
#if UNITY_ANDROID
using UnityEngine.Advertisements;
#endif
public class AdsController : MonoBehaviour {

	public static AdsController Global;

	public bool ShowAds = true;

	public int LocsGone = 0;
	public int LocsBetweenAds;

	void Awake() {
		Global = this;
	}

	void Start() {
		if (PlayerPrefs.GetInt("NoAds")==1) {
			ShowAds = false;
		}
	}

	public void DisplayAd()
	{
		#if UNITY_ANDROID
		LocsGone++;

		if (ShowAds && LocsGone > LocsBetweenAds)
		{
			AdsManager.Global.RequestAd();
			Debug.Log("Drawing ad.");
			LocsGone=0;
		}
		#endif
	}
}
