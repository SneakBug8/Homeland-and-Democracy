using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI
;

public class MinigameButton : MonoBehaviour {
	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener(onClick);
	}
	void onClick () {
		MinigameController.Global.Pressed(GetComponentInChildren<Text>().text ?? "");
	}
}
