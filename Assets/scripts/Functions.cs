using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Functions : MonoBehaviour {
    public static Functions Global;

    void Awake() {
        Global = this;
    }

    public void ReturnToMenu() {
        SceneManager.LoadScene("menu");
    }
}
