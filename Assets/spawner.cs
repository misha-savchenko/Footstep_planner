using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void OnTriggerEnter(Collider collision)
    {

        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "Terrain")
        {
            Debug.Log(collision.gameObject.name);
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
