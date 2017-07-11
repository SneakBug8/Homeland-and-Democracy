using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorController : MonoBehaviour {

	public static ErrorController Global;

	void Awake() {
		Global = this;
	}

	public void NoSuchLocation(string LocationName) {
		Debug.LogError("No such location exists " + LocationName);
	}

	public void EmptyLocation() {
		Debug.LogError("Tried to load empty location");
	}

	public void NoActionOnButton(string ButtonText) {
		Debug.LogError("No assigned actions on button "+ButtonText);
	}
}
