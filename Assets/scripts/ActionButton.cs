using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour {

	public int id;

	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
	}
	public void OnClick () {
		MainController.Global.OnButtonClick(id);
	}

	public void DeleteSelf() {
		MainController.Global.CurrentScene.actions.RemoveAt(id);
		Destroy(gameObject);
	}
}