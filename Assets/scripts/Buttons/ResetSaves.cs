using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetSaves : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
	}
	
	// Update is called once per frame
	void OnClick () {
		PlayerPrefs.DeleteAll();
	}
}
