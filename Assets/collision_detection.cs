using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_detection : MonoBehaviour {
    void OnTriggerEnter(Collider collision)
    {

        Physics.GetIgnoreLayerCollision(0, 9);
        Physics.GetIgnoreLayerCollision(9, 9);

        if (collision.gameObject.name == "Terrain")
        {
            Destroy(this.gameObject);
        }
    }

}
