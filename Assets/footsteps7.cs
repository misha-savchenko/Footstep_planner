using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class footsteps7 : MonoBehaviour
{

    List<List<Node>> list_of_trees = new List<List<Node>> { }; 
    public List<Vector3> path = new List<Vector3> { };
    public bool go = true;

    public GameObject left_foot;
    public GameObject right_foot;

    public Vector3 start_position = new Vector3(0, 0, 0);
    public Vector3 goal_location = new Vector3(40, 0, 40);

    public trrt_interpolation tRRT;

    void Start()
    {

        Vector3 gamma1 = new Vector3(0.08F, 0.0F, 0.1F);
        float theta1 = 0;
        Vector3 gamma2 = new Vector3(0.08F, 0, 0.05F);
        float theta2 = 0;
        Vector3 gamma3 = new Vector3(0.08F, 0, 0);
        float theta3 = 0;
        Vector3 gamma4 = new Vector3(0.07F, 0, 0.07F);
        float theta4 = -30;
        Vector3 gamma5 = new Vector3(0.09F, 0, 0.07F);
        float theta5 = 30;
        Vector3 gamma6 = new Vector3(0.09F, 0, 0);
        float theta6 = 30;
        Vector3 gamma7 = new Vector3(0.08F, 0, -0.04F);
        float theta7 = 10;

        Vector3 gamma1l = new Vector3(-0.08F, 0.0F, 0.1F);
        float theta1l = 0;
        Vector3 gamma2l = new Vector3(-0.08F, 0, 0.05F);
        float theta2l = 0;
        Vector3 gamma3l = new Vector3(-0.08F, 0, 0);
        float theta3l = 0;
        Vector3 gamma4l = new Vector3(-0.07F, 0, 0.07F);
        float theta4l = 30;
        Vector3 gamma5l = new Vector3(-0.09F, 0, 0.07F);
        float theta5l = -30;
        Vector3 gamma6l = new Vector3(-0.09F, 0, 0);
        float theta6l = -30;
        Vector3 gamma7l = new Vector3(-0.08F, 0, -0.04F);
        float theta7l = -10;
        /*
        gamma1[1] = Vector3.Magnitude(gamma1);
        gamma2[1] = Vector3.Magnitude(gamma2);
        gamma3[1] = Vector3.Magnitude(gamma3);
        gamma4[1] = Vector3.Magnitude(gamma4);
        gamma5[1] = Vector3.Magnitude(gamma5);
        gamma6[1] = Vector3.Magnitude(gamma6);
        gamma7[1] = Vector3.Magnitude(gamma7);

        gamma1l[1] = Vector3.Magnitude(gamma1);
        gamma2l[1] = Vector3.Magnitude(gamma2);
        gamma3l[1] = Vector3.Magnitude(gamma3);
        gamma4l[1] = Vector3.Magnitude(gamma4);
        gamma5l[1] = Vector3.Magnitude(gamma5);
        gamma6l[1] = Vector3.Magnitude(gamma6);
        gamma7l[1] = Vector3.Magnitude(gamma7);
        */
        gamma_position_right.Add(gamma1 * gamma_mod_value * multiplier) ;
        gamma_position_right.Add(gamma2 * gamma_mod_value * multiplier);
        gamma_position_right.Add(gamma3 * gamma_mod_value * multiplier);
        gamma_position_right.Add(gamma4 * gamma_mod_value * multiplier);
        gamma_position_right.Add(gamma5 * gamma_mod_value * multiplier);
        gamma_position_right.Add(gamma6 * gamma_mod_value * multiplier);
        gamma_position_right.Add(gamma7 * gamma_mod_value * multiplier);

        gamma_theta_right.Add(theta1);
        gamma_theta_right.Add(theta2);
        gamma_theta_right.Add(theta3);
        gamma_theta_right.Add(theta4);
        gamma_theta_right.Add(theta5);
        gamma_theta_right.Add(theta6);
        gamma_theta_right.Add(theta7);

        gamma_position_left.Add(gamma1l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma2l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma3l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma4l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma5l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma6l * gamma_mod_value * multiplier);
        gamma_position_left.Add(gamma7l * gamma_mod_value * multiplier);

        gamma_theta_left.Add(theta1l);
        gamma_theta_left.Add(theta2l);
        gamma_theta_left.Add(theta3l);
        gamma_theta_left.Add(theta4l);
        gamma_theta_left.Add(theta5l);
        gamma_theta_left.Add(theta6l);
        gamma_theta_left.Add(theta7l);

        goal_threshold *= multiplier;
    }

    public float multiplier = 1F;
    public float gamma_mod_value = 25.7F;

    bool footsteps_displayed = false;
    bool global_foot_paths = false;
    public int goal_index = 3;
    void Update()
    {

        if (GameObject.FindObjectOfType<trrt_interpolation>().path_found && !GameObject.Find("left(Clone)"))
        {
            Vector3 foot_size = left_foot.GetComponent<Renderer>().bounds.size;

            Vector3 left_loc = new Vector3(0F, 0, 0);
            left_foot = GameObject.Instantiate(left_foot, start_position, Quaternion.Euler(0, 90, 0)) as GameObject;
            left_foot.transform.localScale *= multiplier;

            Vector3 right_loc = start_position + new Vector3(2 * foot_size[2], 0, 0);
            right_foot = GameObject.Instantiate(right_foot, right_loc, Quaternion.Euler(0, 90, 0)) as GameObject;
            right_foot.transform.localScale *= multiplier;

            Node first_node = new Node(right_loc, VecIn2D(right_loc), left_loc, 0, 0, 90, "left");
            tree.Add(first_node);
            Debug.Log(goal_location);
        }

        if (!goal_reached)
        {
            HierarchialRRT(start_position, goal_location);
            Debug.Log("still working");
        }
        else if (!global_foot_paths && goal_reached)
        {
            Node closest_node = NearestNeighbor(VecIn2D(goal_location), tree);
            
            Node node = closest_node;
            int num_of_footsteps = 0;
            while (node.self_index != 0)
            {


                //Debug.Log(node.self_index);
                if (node.foot == "left")
                {
                    right_foot = GameObject.Instantiate(right_foot, node.position, Quaternion.Euler(0, node.theta, 0));
                    right_foot.tag = "FinalFootSteps";
                    right_foot.name = "footstep" + (num_of_footsteps+2).ToString();
                }
                else
                {
                    left_foot = GameObject.Instantiate(left_foot, node.position, Quaternion.Euler(0, node.theta, 0));
                    left_foot.tag = "FinalFootSteps";
                    left_foot.name = "footstep" + (num_of_footsteps + 2).ToString();
                }
                Debug.Log("A");
                //Debug.Log(node.parent_index);
                node = tree[node.parent_index];
                num_of_footsteps++;
            }
            
            /*
            for (int i = 0; i < tree.Count; i++)
            {
                Debug.Log("NEW NODE");
                Debug.Log(i);
                Debug.Log(tree[i].self_index);
                Debug.Log(tree[i].parent_index);

            }
            */
            //footsteps_displayed = true;
            if (path.Count - goal_index >= 0)
            {
                goal_location = path[path.Count - goal_index];
                goal_index++;
                goal_reached = false;
                list_of_trees.Add(tree);
                start_position = closest_node.position;
                Debug.Log(closest_node.position);
                tree.Clear();
                closest_node.parent_index = 0;
                closest_node.self_index = 0;
                tree.Add(closest_node);
            }
            else
            {
                global_foot_paths = true;
            }
            
            Debug.Log("goal_reached");
            //Debug.Log(num_of_footsteps);
        }
        else if (global_foot_paths)
        {
            GameObject[] footsteps = GameObject.FindGameObjectsWithTag("PrelimFootSteps");
            foreach(GameObject footstep in footsteps)
            {
                Destroy(footstep);
            }

            Debug.Log("DONE");
        }
    }


    class Node
    {
        public Vector3 position;
        public Vector2 position2d;
        public Vector3 parent_position;
        public int parent_index;
        public int self_index;
        public float theta;
        public string foot;

        public Node(Vector3 pos, Vector2 pos2d, Vector3 p_pos, int pi, int si, float angle, string foot_name)
        {
            position = pos;
            position2d = pos2d;
            parent_position = p_pos;
            parent_index = pi;
            self_index = si;
            theta = angle;
            foot = foot_name;
        }
    }

    List<Node> tree = new List<Node> { };

    public float goal_threshold = 2;
    public float goal_probability = .7F;
    public bool goal_reached = false;

    List<Vector3> gamma_position_right = new List<Vector3> { };
    List<float> gamma_theta_right = new List<float> { };
    List<Vector3> gamma_position_left = new List<Vector3> { };
    List<float> gamma_theta_left = new List<float> { };

    List<Vector4> gamma_adjust;

    int rand_points_created = 0;
    void HierarchialRRT(Vector3 sinit, Vector3 sgoal)// Node sinit, Node sgoal)
    {
        Vector2 srand_location = RandomConfig(sinit, sgoal);
        float rand_float = Random.value;
        if(rand_float > goal_probability)
        {
            srand_location = VecIn2D(sgoal);
        }
        rand_points_created++;
        //point = Instantiate(point, new Vector3(srand_location[0], 0, srand_location[1]), Quaternion.Euler(0, 90, 0)) as GameObject; //spawn new intial point 
        //point.name = "point" + rand_points_created.ToString();
        Node snear = NearestNeighbor(srand_location,tree);
        
        List<Node> snew = GetPossibleFoot(snear, gamma_position_right, gamma_theta_right);
        if(snear.foot == "left")
        {
            snew = GetPossibleFoot(snear, gamma_position_left, gamma_theta_left);
        }

        for (int i = 0; i < snew.Count; i++)
        {
            if (Validate(snew[i]))
            {

                snew[i].self_index = tree.Count;
                tree.Add(snew[i]);

                //For Debugging in the future
                if (snew[i].foot == "left")
                {
                    right_foot = GameObject.Instantiate(right_foot, snew[i].position, Quaternion.Euler(0, snew[i].theta, 0));
                    right_foot.tag = "PrelimFootSteps";
                    right_foot.name = "right_foot" + rand_points_created.ToString();
                }
                else
                {
                    left_foot = GameObject.Instantiate(left_foot, snew[i].position, Quaternion.Euler(0, snew[i].theta, 0));
                    left_foot.tag = "PrelimFootSteps";
                    left_foot.name = "left_foot" + rand_points_created.ToString();
                }
                

                if (Vector2.Distance(VecIn2D(snew[i].position),VecIn2D(sgoal)) < goal_threshold)
                {
                    
                    goal_reached = true;
                    break;
                }
            }
            else
            {
                List<Node> adjusted_node = AlignExtend(snew[i],gamma_adjust);
                for(int j = 0; j < adjusted_node.Count; j++)
                {
                    if (Validate(adjusted_node[j]))
                    {
                        adjusted_node[j].self_index = tree.Count - 1;
                        tree.Add(adjusted_node[j]);
                        break;
                    }
                }
            }
            }
        
    }

    Vector2 RandomConfig(Vector3 sinit, Vector3 sgoal)
    {
        Vector2 sinit2d = VecIn2D(sinit);
        Vector2 sgoal2d = VecIn2D(sgoal);

        float radius = Vector2.Distance(sinit2d, sgoal2d)/2*1.25F;
        Vector2 center = Vector2.Lerp(sinit2d, sgoal2d, 0.5f);

        float rand_x = Mathf.Cos(Random.Range(0, 360) * Mathf.Deg2Rad) * Random.Range(0, radius) ;
        float rand_y = Mathf.Sin(Random.Range(0, 360) * Mathf.Deg2Rad) * Random.Range(0, radius);

        Vector2 ret_vector = center + new Vector2(rand_x, rand_y);
        return ret_vector;
    }

    Node NearestNeighbor(Vector2 srand_location, List<Node> tree)
    {
        int nearest_index = 0;
        Vector2 nearest_location = VecIn2D(tree[0].position);
        float nearest_distance = Vector2.Distance(nearest_location,srand_location);

        for (int i = 0; i < tree.Count; i++)
        {
            Vector2 current_node = VecIn2D(tree[i].position);
            if (Vector2.Distance(current_node, srand_location) < nearest_distance){
                nearest_location = VecIn2D(tree[i].position);
                nearest_distance = Vector2.Distance(current_node, srand_location);
                nearest_index = i;
            }
        }
        return tree[nearest_index];
    }

    List<Node> GetPossibleFoot(Node snear, List<Vector3> gamma_position, List<float> gamma_theta)
    {
                
        List<Node> possible_feet = new List<Node> { };
        for(int i = 0; i < gamma_position.Count; i++)
        {
            float angle = snear.theta + gamma_theta[i];
            //Vector3 snew_pos = snear.position + transform.TransformVector(gamma_position[i]);//(Quaternion.Euler(0,gamma_theta[i],0)*gamma_position[i]);
            Vector3 snew_pos = snear.position + (Quaternion.Euler(0,snear.theta-90,0)*gamma_position[i]);
            Vector2 snew_pos2d = new Vector2(snew_pos[0]*Mathf.Cos(Mathf.Deg2Rad* gamma_theta[i]), snew_pos[2] * Mathf.Sin(Mathf.Deg2Rad * gamma_theta[i]));

            snew_pos = tRRT.getPoint(snew_pos[0]/tRRT.x_max,snew_pos[2] / tRRT.y_max);
            Vector3 p_pos = snear.position;
            int pi = snear.self_index;
            int si = tree.Count + possible_feet.Count;
            string foot_name = "left";
            if (snear.foot == "left") { foot_name = "right"; }
            Node new_node = new Node(snew_pos,snew_pos2d,p_pos,pi,si,angle,foot_name);
            possible_feet.Add(new_node);
        }

        return possible_feet;

    }

    bool Validate(Node node)
    {
        return true;
    }
    
    List<Node> AlignExtend(Node snew, List<Vector4> gamma_adjust)
    {
        Debug.Log("AlignExtend reaches");
        return tree;
    }

    Vector2 VecIn2D(Vector3 vector){return new Vector2(vector[0], vector[2]);}

    float ReachableAreaCost()
    {

        return 0;
    }

    float AdjustableAreaCost()
    {
        return 0;
    }

    float FootAreaCost()
    {
        return 0;
    }
    
    float x_max;
    float y_max;

}
