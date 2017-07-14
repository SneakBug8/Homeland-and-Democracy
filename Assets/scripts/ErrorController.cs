using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorController : MonoBehaviour {

	public static ErrorController Global;

	void Awake() {
		Global = this;
	}

	public void NoSuchScene(string SceneName) {
		Debug.LogError("No such scene exists " + SceneName);
	}

	public void EmptyScene() {
		Debug.LogError("Tried to load empty Scene");
	}

	public void NoActionOnButton(string ButtonText) {
		Debug.LogError("No assigned actions on button "+ButtonText);
	}
}
