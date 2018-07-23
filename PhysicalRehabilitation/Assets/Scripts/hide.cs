using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hide : MonoBehaviour {
    public GameObject hideThis;
	// Use this for initialization
	void Start () {
        hideThis.GetComponent<Renderer>().enabled = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
