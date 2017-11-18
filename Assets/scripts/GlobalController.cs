using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class GlobalController : MonoBehaviour {

    public static GlobalController Global;

    public delegate void EventDelegate ();
    public event EventDelegate OnEarlyLoad;
    public event EventDelegate OnLoad;
    public event EventDelegate OnLateLoad;

    public static GameOptions gameoptions;
    void Awake () {
        Global = this;

        XmlNode rootxml = MainController.Global.LoadXmlFile("Settings");
        gameoptions = new GameOptions {
            Geography = (rootxml["geography"]!=null) ? true : false,
            RandomEncounters = (rootxml["randomencounters"]!=null) ? true : false,
            Inventory = (rootxml["inventory"] != null) ? true : false
        };
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

public class GameOptions {
    public bool Geography;

    public bool RandomEncounters;

    public bool Inventory;
}