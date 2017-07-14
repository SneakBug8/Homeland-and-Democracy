using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour {

    public static GlobalController Global;

    public delegate void EventDelegate ();
    public event EventDelegate OnEarlyLoad;
    public event EventDelegate OnLoad;
    public event EventDelegate OnLateLoad;
    void Awake () {
        Global = this;
    }

    void Start () {

        if (OnEarlyLoad!=null) {
            OnEarlyLoad();
        }
        if (OnLoad!=null) {
            OnLoad();
        }
        if (OnLateLoad!=null) {
            OnLateLoad();
        }

        // Вызов функции инициализации (опционально)
        Functions.Global.SendMessage ("init", SendMessageOptions.DontRequireReceiver);
    }
}