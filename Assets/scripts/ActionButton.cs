using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour {

	public Action action;

	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
	}
	public void OnClick () {
		action.Execute();
	}

	public void DeleteSelf() {
		Destroy(gameObject);
	}
}