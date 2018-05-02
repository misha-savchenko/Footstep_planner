using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class comm : MonoBehaviour {

    public footsteps8 footsteps;
    public trrt_interpolation global_RRT;

    public terrain_color_2 terrain_color;
    public bool global_path_generated = false;
    public bool debug = true;

    public int step_d = 10;

    public float hcrit = .4F;
    public float scrit = 30;
    public float rcrit = 1;

    public float slope_weight = 0.5F;
    public float height_weight = 0.25F;
    public float roughness_weight = 0.25F;

    public float multiplier = 3F;
    public Vector3 foot_size = new Vector3(2, 0.1f, 1);
    
    void Start()
    {
        Physics.GetIgnoreLayerCollision(0, 9);
        Physics.GetIgnoreLayerCollision(9, 9);

        foot_size *= multiplier;

        global_RRT = GameObject.FindObjectOfType<trrt_interpolation>();
        footsteps = GameObject.FindObjectOfType<footsteps8>();

        global_RRT.delta *= multiplier/2;

        terrain_color = GameObject.FindObjectOfType<terrain_color_2>();

        global_RRT.foot_size = foot_size;

        global_RRT.d = step_d;
        terrain_color.d = step_d;


        float[] crits = new float[] { scrit, hcrit, rcrit, 9999 };
        float[] weights = new float[] { slope_weight, height_weight, roughness_weight, 1};

        global_RRT.crits = crits;
        terrain_color.crits = crits;

        global_RRT.weights = weights;
        terrain_color.weights = weights;

        footsteps.multiplier = multiplier;
        footsteps.foot_size = foot_size;

        footsteps.enabled = false;
    }
    void Update()
    {
        if (!GameObject.Find("Starting point(Clone)") || !GameObject.Find("Final point(Clone)"))
        {
            if (!GameObject.Find("Starting point(Clone)"))
            {
                SpawnStartEndPoints(global_RRT.starting_point);
            }
            else
            {
                SpawnStartEndPoints(global_RRT.final_point);
            }
        }
        if (global_RRT.path_found && (!GameObject.Find("left(Clone)") || !GameObject.Find("right(Clone)")) && Input.GetKeyDown(KeyCode.Space))
        {
            footsteps.path = global_RRT.path;
            footsteps.start_position = footsteps.path[footsteps.path.Count-1];
            footsteps.goal_location = footsteps.path[footsteps.path.Count-2];
            footsteps.tRRT = global_RRT;
            footsteps.enabled = true;
        }


    }


    Ray myRay;
    RaycastHit hit;

    void SpawnStartEndPoints(GameObject start_or_end_point)
    {
        myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(myRay, out hit))
        {
            if (Input.GetMouseButtonDown(0))
            {

                start_or_end_point = GameObject.Instantiate(start_or_end_point, hit.point, Quaternion.identity);
                //LineRenderer lineRenderer = start_or_end_point.AddComponent<LineRenderer>();


                if (!GameObject.Find("Final point(Clone)"))
                {
                    global_RRT.starting_location = hit.point;
                }

                else
                {
                    global_RRT.goal_location = hit.point;
                }
            }

        }
    }

}
