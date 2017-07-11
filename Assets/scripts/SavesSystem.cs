using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavesSystem : MonoBehaviour {

	public static SavesSystem Global;

	void Awake() {
		Global = this;
		DontDestroyOnLoad(gameObject);
	}

	public void Set(string key, string value)
	{
		PlayerPrefs.SetString(key,value);
		Save();
	}

	public string Get(string key)
	{
		if (!PlayerPrefs.HasKey(key))
			return null;
		return PlayerPrefs.GetString(key, null);
	}

	public void Save() {
		PlayerPrefs.Save();
	}
	
	/*** Inventory System ***/
	public static List<string> Items {
		get {
			return SavesSystem.Global.GetItems();
		}
		set {
			Items = value;
			SavesSystem.Global.SaveItems(Items);
		}
	}

	public void SaveItems(List<string> items) {
		PlayerPrefs.SetInt("items_count",Items.Count);

		int itemid = 0;
		foreach (string item in Items) {
			itemid = Items.IndexOf(item);
			Set("items_"+itemid,Items[itemid]);
		}
	}
	public List<string> GetItems() {
		List<string> items = new List<string>();

		if (!PlayerPrefs.HasKey("items_count"))
			return items;
		
		int count = PlayerPrefs.GetInt("items_count");

		for (int i=0; i<count; i++) {
			items.Add(Get("items_"+i));
		}
		return items;
	}

	/*** Removing Data ***/
	public void ResetSaves() {
		PlayerPrefs.DeleteAll();
	}

	public void ResetItems() {
		SaveItems(new List<string>());
	}
}
