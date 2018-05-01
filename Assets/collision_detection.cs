using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_detection : MonoBehaviour {
    public footsteps8 footsteps;

    void Start()
    {
        footsteps = GameObject.FindObjectOfType<footsteps8>();
    }
    void OnTriggerEnter(Collider collision)
    {

        Physics.GetIgnoreLayerCollision(0, 9);
        Physics.GetIgnoreLayerCollision(9, 9);

        //Debug.Log(collision.gameObject.name);
        //Debug.Log(gameObject.transform.position);
        if (collision.gameObject.name == "Terrain")
        {
            
            Debug.Log("B");

            //gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(this.gameObject);
        }
    }

}
