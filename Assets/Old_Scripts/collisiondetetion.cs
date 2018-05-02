using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisiondetetion : MonoBehaviour {


    // Use this for initialization
    void Start () {
        GameObject a = Instantiate(box, new Vector3(100, 0, 100), Quaternion.identity);
        a.tag = "ASD";
        Debug.Log(GameObject.FindWithTag("ASD"));

    }
    public GameObject box;
	// Update is called once per frame
	void Update ()
    {
        Debug.Log(GameObject.FindWithTag("ASD"));
    }
}
