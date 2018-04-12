using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class footsteps : MonoBehaviour {

    public global_t_rrt global_RRT;

    public List<Vector3> path;

    //public Camera Main;
    //Game objects for the feet
    public GameObject left_foot;
    public GameObject left_foot_after_image;
    public GameObject right_foot;
    public GameObject right_foot_after_image;
    public GameObject point;

    public GameObject goal;
    public Vector3 start_location = new Vector3(0, 0, 0);
    public Vector3 goal_location = new Vector3(40, 0, 40);
    public Vector3 temp_goal_location = new Vector3(75, 0, 75);
    bool temp_goal_reached = false;
    int current_mid_point = 1;

    void Start () {

        Vector3 foot_size = left_foot.GetComponent<Renderer>().bounds.size;

        Vector3 left_loc = new Vector3(-foot_size[2], 0, 0);
        left_foot = GameObject.Instantiate(left_foot, start_location+left_loc, Quaternion.Euler(0,90,0)) as GameObject; //spawn new intial point 

        Vector3 right_loc = new Vector3(foot_size[2], 0, 0);
        right_foot = GameObject.Instantiate(right_foot, start_location+right_loc, Quaternion.Euler(0, 90, 0)) as GameObject; //spawn new intial point 

    }

    bool left = true;
    public bool go = false;

    void Update () {
        
        if (go)
        {
            if (left)
            {
                left_foot_after_image = GameObject.Instantiate(left_foot_after_image, left_foot.transform.position, Quaternion.Euler(left_foot.transform.eulerAngles)) as GameObject; //spawn new intial point 
                Footstep(left_foot, right_foot, "left");
                left = !left;
            }
            else
            {
                right_foot_after_image = GameObject.Instantiate(right_foot_after_image, right_foot.transform.position, Quaternion.Euler(right_foot.transform.eulerAngles)) as GameObject; //spawn new intial point 
                Footstep(right_foot, left_foot, "right");
                left = !left;
            }
            //go = false;
        }

        if (Vector3.Distance(right_foot.transform.position, temp_goal_location) <= 10 || Vector3.Distance(left_foot.transform.position, temp_goal_location) <= 10)
        {
            if(goal_location == temp_goal_location)
            {
                temp_goal_reached = true;
                go = false;
            }
            else
            {
                temp_goal_location = path[path.Count - 1 - current_mid_point];
                current_mid_point++;
                //Debug.Log(temp_goal_location);

            }
        }
        else
        {
            go = true;
        }

        //Main.transform.position = left_foot.transform.position+new Vector3(0,90,0);
	}


    

    List<Node> tRRT = new List<Node>();


    class Node
    {
        public Vector3 position;
        public Vector3 parent_position;
        public int parent_index;

        public Node(Vector3 pos, Vector3 p_pos, int pi)
        {
            position = pos;
            parent_position = p_pos;
            parent_index = pi;
        }
    }

    public float delta = .2F;
    public int local_tree_nodes = 200;

    Vector3 localRRT(GameObject other_foot, string foot)
    {
        //Debug.Log(foot);
        Vector2 other_foot_pos= new Vector2(other_foot.transform.position[0], other_foot.transform.position[2]);
        //Vector2 foot_pos = new Vector2(foot.transform.position[0], foot.transform.position[2]);
        
        List <Node> local_tree = new List<Node>();
        Vector2 start_loc = new Vector2(r_min, 0); 
        if(foot == "left") { start_loc *= -1; }
        start_loc += other_foot_pos;
        //start_loc = start_loc - new Vector2(other_foot.transform.position[0], other_foot.transform.position[2]);

        Node root = new Node(start_loc, start_loc, 0);
        local_tree.Add(root);
        //keep track of the lowest cost (in this case the most forward node 
        float lowest_cost = 0;
        int best_node_ind = 0;

        int nodes = 0;
        while (nodes < local_tree_nodes)
        {
            nodes++;
            //get random point in the convex bounds 
            Vector2 rand_loc = getRandLoc(r_min, r_max, ang_min, ang_max, foot) + other_foot_pos;
            //initialize the closest point with absurd values
            //Vector2 closest_node = start_loc;
            Vector2 closest_node = new Vector2(9999, 9999);
            float dist_to_closest = 9999;
            //find clossest point 
            for (int i = 0; i < local_tree.Count; i++)
            {
                if (closest_node != new Vector2(9999,9999))
                {

                    Vector2 tree_node = (Vector2)local_tree[i].position;
                    if (Vector2.Distance(rand_loc, tree_node) <= dist_to_closest)
                    {
                        dist_to_closest = Vector2.Distance(rand_loc, tree_node);
                        closest_node = tree_node;
                    }
                }

                else
                {
                    dist_to_closest = Vector2.Distance(rand_loc, start_loc);
                    closest_node = start_loc;
                }
            }
            //calculate new node 
            Vector2 new_node = closest_node + (rand_loc-closest_node).normalized * delta;


            //==========================
            //check new node
            //==========================
            float node_cost = getCost(new_node);

            //add new node to the tree
            int parent_ind = 0;
            while ((Vector2)local_tree[parent_ind].position != closest_node) { parent_ind++; }
            Node leaf = new Node(new_node, closest_node, parent_ind);
            local_tree.Add(leaf);
            if (node_cost > lowest_cost) {
                lowest_cost = node_cost;
                best_node_ind = local_tree.Count;
            }

            //spawn the sphere at the new node location 
            // pe = Instantiate(point, new Vector3(new_node[0],0,new_node[1]), Quaternion.identity) as GameObject; //spawn new intial point 
            /*Renderer prend = p.GetComponent<Renderer>();
            prend.material = new Material(Shader.Find("Sprites/Default"));
            prend.material.color = Color.red;*/
        }
        Vector3 step = global_RRT.getPoint((local_tree[best_node_ind - 1].position[0])/ global_RRT.x_max, (local_tree[best_node_ind - 1].position[1])/ global_RRT.y_max);
        GameObject p = Instantiate(point, step, Quaternion.identity) as GameObject; //spawn new intial point 
                                                                                                                                                                            //spawn the sphere at the new node location 
        Renderer prend = p.GetComponent<Renderer>();
        prend.material = new Material(Shader.Find("Sprites/Default"));
        prend.material.color = Color.red;
        return step;
    }
    //cost function 
    float getCost(Vector2 node_candidate)
    {
        float cost = 1/Vector2.Distance(node_candidate,new Vector2(temp_goal_location[0], temp_goal_location[2]));
        return cost;
    }


    void Footstep(GameObject foot, GameObject other_foot, string foot_name) {
        

        foot.transform.position = localRRT(other_foot,foot_name);
        Vector2 local_goal_loc = new Vector2(temp_goal_location[0], temp_goal_location[2]) - new Vector2(foot.transform.position[0], foot.transform.position[2]);
        float angle = Vector2.Angle(foot.transform.eulerAngles, local_goal_loc);
        
        //Debug.Log(angle);



        foot.transform.eulerAngles = new Vector3(0, foot.transform.eulerAngles[1]-angle, 0);

    }


    public float reachability_radius = 4;



    //Sweeping foot convex region 

    float r_min = 20*Mathf.Sqrt(2);
    float r_max = 40F;
    float ang_min = -1.2F;
    float ang_max = 1.2F;

    Vector2 getRandLoc(float r_min, float r_max, float ang_min, float ang_max, string foot)
    {
        float radius = Random.Range(r_min, r_max);
        //Debug.Log(radius);
        float angle = Random.Range(ang_min, ang_max);
        //Debug.Log(angle);
        Vector2 rand_loc = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
        if (foot == "left") { rand_loc *= -1; }
        return rand_loc;
    }
}
