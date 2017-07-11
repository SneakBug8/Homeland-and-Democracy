using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {

	public string scene;

	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
	}
	public void OnClick () {
		SceneManager.LoadScene(scene);
	}
}
