using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour {

	public static GlobalController Global;

	public Items items;

	void Awake() {
		Global = this;

		items = new Items();
	}

	void Start() {
		items.list = SavesSystem.Global.GetItems();
	}
}

public class Items {
	public List<string> list;
	public void Remove(string itemkey) {
		list.Remove(itemkey);
		SavesSystem.Global.SaveItems(list);
		Debug.Log("No such item");
	}

	public void AddItem(string item) {
		list.Add(item);
		SavesSystem.Global.SaveItems(list);
	}
}
