using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class footsteps : MonoBehaviour {


    //Game objects for the feet
    public GameObject left_foot;
    public GameObject left_foot_after_image;
    public GameObject right_foot;
    public GameObject right_foot_after_image;
    public GameObject point;

	void Start () {
        Vector3 foot_size = left_foot.GetComponent<Renderer>().bounds.size;


        Vector3 left_loc = new Vector3(0F, 0, 0);
        left_foot = GameObject.Instantiate(left_foot, left_loc, Quaternion.Euler(0,90,0)) as GameObject; //spawn new intial point 

        Vector3 right_loc = new Vector3(2 * foot_size[2], 0, 0);
        right_foot = GameObject.Instantiate(right_foot, right_loc, Quaternion.Euler(0, 90, 0)) as GameObject; //spawn new intial point 

    }

    bool left = true;
    public bool go = true;

    void Update () {
        
        while (go)
        {
            if (left)
            {
                left_foot_after_image = GameObject.Instantiate(left_foot_after_image, left_foot.transform.position, Quaternion.Euler(0, 90, 0)) as GameObject; //spawn new intial point 
                Footstep(left_foot, right_foot, "left");
                left = !left;
            }
            else
            {
                right_foot_after_image = GameObject.Instantiate(right_foot_after_image, right_foot.transform.position, Quaternion.Euler(0, 90, 0)) as GameObject; //spawn new intial point 
                Footstep(right_foot, left_foot, "right");
                left = !left;
            }
            go = false;
        }
	}


    //Sweeping foot convex region 
    float r_min = 4;
    float r_max = 6;
    float ang_min = -.5F;
    float ang_max = .5F;

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
        Vector2 other_foot_pos= new Vector2(other_foot.transform.position[0], other_foot.transform.position[2]);

        List<Node> local_tree = new List<Node>();
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
            GameObject pe = Instantiate(point, new Vector3(new_node[0],0,new_node[1]), Quaternion.identity) as GameObject; //spawn new intial point 
            /*Renderer prend = p.GetComponent<Renderer>();
            prend.material = new Material(Shader.Find("Sprites/Default"));
            prend.material.color = Color.red;*/
        }
        Vector3 step = new Vector3(local_tree[best_node_ind - 1].position[0],0,local_tree[best_node_ind - 1].position[1]);
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
        float cost = node_candidate[1]/50;
        return cost;
    }


    void Footstep(GameObject foot, GameObject other_foot, string foot_name) {
        
        //Vector3 rand_loc = getRandLoc(r_min, r_max, ang_min, ang_max,foot_name);
        ///foot.transform.position = new Vector3(0F, 0, 0);
        //foot.transform.position += other_foot.transform.position+rand_loc;
        //Vector3 start_loc = new Vector3(r_min, 0, 0);
        // if (foot_name == "left") { start_loc *= -1; }
        foot.transform.position = localRRT(other_foot,foot_name);


    }

    /*
    Vector3 getRandLoc(float r_min, float r_max, float ang_min, float ang_max, string foot)
    {   
        float radius = Random.Range(r_min,r_max);
        Debug.Log(radius);
        float angle = Random.Range(ang_min, ang_max);
        Debug.Log(angle);
        Vector2 rand_loc_2d = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
        Vector3 rand_loc = new Vector3(rand_loc_2d[0], 0, rand_loc_2d[1]);
        if (foot == "left") { rand_loc *= -1; }
        return rand_loc;
    }
    */

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
