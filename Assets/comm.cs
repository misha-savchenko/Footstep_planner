using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class comm : MonoBehaviour {

    //dimension are a pain lets say that each unity unit is a meter 
    //maps 500 x 500 meters
    //foot size length: 25 cm width: 10 cm
    //global node delta: 1 meter
    //footstep node delta .002 cm

    public footsteps7 footsteps;
    //public steps_A_E global_RRT;
    //public global_t_rrt_variant global_RRT;
    public trrt_interpolation global_RRT;

    public terrain_color_2 terrain_color;
    public bool global_path_generated = false;
    public bool debug = true;

    public int step_d = 10;
    public float hcrit = .4F;
    void Start()
    {


        //global_RRT = GameObject.FindObjectOfType<steps_A_E>();
        //global_RRT = GameObject.FindObjectOfType<global_t_rrt_variant>();
        global_RRT = GameObject.FindObjectOfType<trrt_interpolation>();
        footsteps = GameObject.FindObjectOfType<footsteps7>();
        //footsteps.global_RRT = GameObject.FindObjectOfType<trrt>();



        terrain_color = GameObject.FindObjectOfType<terrain_color_2>();


        global_RRT.d = step_d;
        terrain_color.d = step_d;

        global_RRT.hcrit = hcrit;
        terrain_color.hcrit = hcrit;


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


        if (global_RRT.path_found && (!GameObject.Find("left(Clone)") || !GameObject.Find("right(Clone)")))
        {
            footsteps.path = global_RRT.path;
            footsteps.start_position = footsteps.path[footsteps.path.Count-1];
            //footsteps.temp_goal_location = footsteps.path[footsteps.path.Count - 2];
            footsteps.goal_location = footsteps.path[footsteps.path.Count-2];
            footsteps.tRRT = global_RRT;
            
            /*
            footsteps.left_foot.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
            footsteps.right_foot.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
            footsteps.left_foot_after_image.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
            footsteps.right_foot_after_image.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
            */
            footsteps.enabled = true;

            Debug.Log("Yup");
        }


    }


    Ray myRay;
    RaycastHit hit;

    void SpawnStartEndPoints(GameObject start_or_end_point)
    {
        myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(myRay, out hit))
        { // here I ask : if myRay hits something, store all the info you can find in the raycasthit varible.
          // things like the position where the hit happend, the name of the object that got hit etc..etc..

            if (Input.GetMouseButtonDown(0))
            {

                start_or_end_point = GameObject.Instantiate(start_or_end_point, hit.point, Quaternion.identity);// instatiate a prefab on the position where the ray hits the floor.
                LineRenderer lineRenderer = start_or_end_point.AddComponent<LineRenderer>();


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
