using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class SavesSystem : MonoBehaviour {

    public static SavesSystem Global;

    void Awake () {
        Global = this;
        DontDestroyOnLoad (gameObject);
    }

    public void Set (string key, string value) {
        PlayerPrefs.SetString (key, value);
        Save ();
    }

    public string Get (string key) {
        if (!PlayerPrefs.HasKey (key))
            return null;
        return PlayerPrefs.GetString (key, null);
    }

    public void Save () {
        PlayerPrefs.Save ();
    }

    /*** Inventory System ***/
    public static Dictionary<string, int> Items {
        get {
            if (Items == null) {
                Items = SavesSystem.Global.GetItems ();
            }
            return Items;
        }
        set {
            Items = value;
            SavesSystem.Global.SaveItems (Items);
        }
    }

    public void SaveItems (Dictionary<string, int> items) {
        foreach (KeyValuePair<string, int> item in items) {
            PlayerPrefs.SetInt ("items_" + item.Key, item.Value);
        }
    }
    public Dictionary<string, int> GetItems () {
        Dictionary<string, int> items = new Dictionary<string, int> ();

        XmlNode ItemsNode = MainController.Global.LoadXmlFile ("Items");

        foreach (XmlElement Item in ItemsNode.ChildNodes) {
            int itemcount = PlayerPrefs.GetInt ("items_" + Item.InnerText);
            if (itemcount != 0) {
                items.Add (Item.InnerText, itemcount);
            }
        }

        return items;
    }

    /*** Removing Data ***/
    public void ResetSaves () {
        PlayerPrefs.DeleteAll ();
    }

    public void ResetItems () {
        SaveItems (new Dictionary<string, int>());
    }
}