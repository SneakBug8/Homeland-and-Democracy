using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	public Vector3 rotSPD;
	public float rotShift;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.eulerAngles = rotSPD * (Time.time + rotShift);
	}
}
