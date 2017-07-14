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

	public int ScenesGone = 0;
	public int ScenesBetweenAds;

	void Awake() {
		Global = this;
	}

	void Start() {
		if (PlayerPrefs.GetInt("NoAds")==1) {
			ShowAds = false;
			return;
		}

		MainController.Global.OnSceneChanged += DisplayAd;
	}

	public void DisplayAd()
	{
		#if UNITY_ANDROID
		ScenesGone++;

		if (ShowAds && ScenesGone > ScenesBetweenAds)
		{
			AdsManager.Global.RequestAd();
			Debug.Log("Drawing ad.");
			ScenesGone=0;
		}
		#endif
	}
}
