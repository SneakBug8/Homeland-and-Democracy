using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class MinigameController : MonoBehaviour {

	public static MinigameController Global;
	public string[] Words;
	public string ArrayPressed;

	void Awake() {
		Global = this;
	}

	public void Pressed(string letter) {
		ArrayPressed+=letter;

		CheckWords();
	}

	void CheckWords() {
		foreach (string word in Words) {
			if (ArrayPressed == word) {
				GlobalController.Global.items.AddItem(word);
			}
		}
	}
}