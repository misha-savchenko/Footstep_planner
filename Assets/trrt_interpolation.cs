﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trrt_interpolation: MonoBehaviour
{

    public bool path_found = false;
    public float node_line_width = 1;
    public float path_width = 3;
    List<Node> tRRT = new List<Node>();

    public bool go = false;

    float inf_radius;
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

    public GameObject starting_point;
    public GameObject point;
    public GameObject final_point;

    public int max_num_point = 5000;
    public int curr_num_points = 0;

    public float x_max = 100;
    public float y_max = 100;


    public float delta = 20;

    public Vector3 goal_location;
    public Vector3 starting_location;

    //Transition test elements
    int failed_queries = 0;
    int maximum_failed_queries = 50;

    float alpha = 2;
    float T = 5;
    float K_cost = 10;

    float cost_max = 1;

    GameObject terrain_gm;
    Terrain terrain;
    public TerrainData terrainData;

    Ray myRay;
    RaycastHit hit;

    void Start()
    {
        terrain_gm = GameObject.Find("Terrain");
        terrain = terrain_gm.GetComponent<Terrain>();
        terrainData = terrain.terrainData;


        x_max = terrainData.bounds.size[0];
        y_max = terrainData.bounds.size[2];

        inf_radius = 9999999;

        Physics.gravity = new Vector3(0, 0, 0);
    }

    public float epsilon = 1;
    public float R_multiplier = 10;

    public Vector3 foot_size = new Vector3(2, 0.1f, 1);

    void Update()
    {
        if (GameObject.Find("Starting point(Clone)"))
        {
            Node root = new Node(starting_location, starting_location, -1);
            tRRT.Add(root);
            curr_num_points++;
        }
        if (GameObject.Find("Starting point(Clone)") && GameObject.Find("Final point(Clone)"))
        {

            bool redo = false;
            if (curr_num_points <= max_num_point && go)
            {


                Node closest_node = tRRT[0];
                Vector2 rand_loc = new Vector2(x_max * Random.value, y_max * Random.value);
                Vector2 closest_node_loc = new Vector2(tRRT[0].position[0], tRRT[0].position[2]);
                float closest_distance = Vector2.Distance(new Vector2(closest_node.position[0], closest_node.position[2]), rand_loc);

                for (int i = 0; i < tRRT.Count; i++)
                {
                    Node curr_node = tRRT[i];
                    Vector2 curr_node_loc = new Vector2(curr_node.position[0], curr_node.position[2]);

                    if (Vector2.Distance(curr_node_loc, rand_loc) < closest_distance)
                    {
                        closest_node = tRRT[i];
                        closest_node_loc = new Vector2(tRRT[i].position[0], tRRT[i].position[2]);
                        closest_distance = Vector2.Distance(closest_node_loc, rand_loc);
                    }
                }

                Vector2 new_node_loc = closest_node_loc;

                new_node_loc = Interpolate(closest_node_loc, rand_loc);
                Vector3 new_node_pos = getPoint(new_node_loc[0] / x_max, new_node_loc[1] / y_max);
                if (TransitionTest(closest_node.position, new_node_pos))
                {

                    int parent_ind = 0;

                    while (tRRT[parent_ind].position != closest_node.position) { parent_ind++; }

                    Node leaf = new Node(new_node_pos, closest_node.position, parent_ind);
                    tRRT.Add(leaf);
                    GameObject p = Instantiate(point, new_node_pos, Quaternion.identity) as GameObject; 

                    LineRenderer lineRenderer = p.GetComponent<LineRenderer>();

                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.widthMultiplier = node_line_width;
                    lineRenderer.positionCount = 2;

                    float alpha = 1.0f;
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                        );

                    lineRenderer.colorGradient = gradient;
                    lineRenderer.SetPosition(0, closest_node.position);
                    lineRenderer.SetPosition(1, leaf.position);

                    curr_num_points++;
                    if (Vector3.Distance(goal_location, leaf.position) <= delta)
                    {

                        Node final_node = leaf;
                        build_path(final_node);
                        go = false;
                        path_found = true;
                    }
                    redo = true;

                }
            }
        }
        if (path_found)
        {
            //Debug.Log(path.Count);
        }
    }

    public List<Vector3> path = new List<Vector3> { };

    void build_path(Node final_node)
    {
        Node curr_node = final_node;
        int parent_index = final_node.parent_index;
        path.Add(goal_location);

        while (parent_index != 0)
        {
            path.Add(curr_node.position);
            curr_node = tRRT[parent_index];
            parent_index = curr_node.parent_index;
        }
        path.Add(curr_node.position);
        path.Add(starting_location);

        GameObject p = Instantiate(point, final_node.position, Quaternion.identity) as GameObject; //spawn new intial point 

        LineRenderer lineRenderer = p.GetComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = path_width;
        lineRenderer.positionCount = path.Count;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(new Color(1, 0, 1, 1), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );

        lineRenderer.colorGradient = gradient;

        Vector3[] path2 = path.ToArray();

        curr_num_points++;
        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path2);


    }

    bool TransitionTest(Vector3 qnear, Vector3 qnew)
    {
        cost_max = .8F;
        float cost_near = getCost(qnear, d);// + (Vector3.Distance(qnear, goal_location) / Vector3.Distance(starting_location, goal_location));
        float cost_new = getCost(qnew, d);// +(Vector3.Distance(qnew, goal_location) / Vector3.Distance(starting_location, goal_location));

        if (cost_new > cost_max)
        {
            return false;
        }
        if (cost_new < cost_near)
        {
            return true;
        }


        //float dist = Vector2.Distance(new Vector2(qnear[0],qnear[2]), new Vector2(qnew[0],qnew[2]));
        K_cost = (cost_near + cost_new) / 2;
        T = 30;//current set to 5 
        alpha = 2;//current set to 2
        float dist = Vector3.Distance(qnear, qnew);
        float trans_prob = -((cost_new - cost_near) / dist) / (K_cost * T);
        float rho;
        if (cost_new - cost_new == 0) { rho = 1; }
        else { rho = Mathf.Exp((float)trans_prob); }

        if (Random.value < rho)
        {
            T /= alpha;
            failed_queries = 0;
            return true;
        }
        else
        {
            if (failed_queries > maximum_failed_queries)
            {
                T *= alpha;
                failed_queries = 0;
            }
            else
            {
                failed_queries += 1;
            }
            return false;
        }
    }

    public Vector3 getPoint(float x, float y)
    {
        float x_corr = x * x_max;
        float y_corr = y * y_max;
        float z_corr = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth), Mathf.RoundToInt(y * terrainData.heightmapHeight));
        z_corr += foot_size[1];

        Vector3 location = new Vector3(x_corr, z_corr, y_corr);
        return location;
    }

    public float getCost(Vector3 position, int d)
    {

        /*
        Node closest_node = tRRT[0];
        Vector2 closest_node_loc = new Vector2(tRRT[0].position[0], tRRT[0].position[2]);
        float closest_distance = Vector2.Distance(new Vector2(closest_node.position[0], closest_node.position[2]), position);

        for (int i = 0; i < tRRT.Count; i++)
        {
            Node curr_node = tRRT[i];
            Vector2 curr_node_loc = new Vector2(curr_node.position[0], curr_node.position[2]);

            if (Vector2.Distance(curr_node_loc, position) < closest_distance)
            {
                closest_node = tRRT[i];
                closest_node_loc = new Vector2(tRRT[i].position[0], tRRT[i].position[2]);
                closest_distance = Vector2.Distance(closest_node_loc, position);
            }
        }


        //new node 
        //closest node
        //parent node

        Debug.Log("AAAAAAA");
        Debug.Log(closest_node.position);
        Debug.Log(position);
        if (closest_node.parent_index != -1)
        {
            Debug.Log(tRRT[closest_node.parent_index].position);
            Vector2 C = new Vector2(position[0], position[2]);
            Vector2 B = new Vector2(position[0], closest_node.position[2]);
            Vector2 A = new Vector2(tRRT[closest_node.parent_index].position[0], tRRT[closest_node.parent_index].position[2]);

            float angle = Vector2.Angle(A - B, C - B);
            Debug.Log(angle);

        }
        */


        float height = position[1];
        float steepness = terrainData.GetSteepness(position[0] / x_max, position[2] / y_max);


        float slope_weight = 0.5F;
        float height_weight = 0.25F;
        float roughness_weight = 0.25F;
        //float euclidean_distance_weight = .4F;
        //float angle_weight = 0.10F

        float[] weights = { slope_weight, height_weight, roughness_weight, 1 };// , euclidean_distance_weight };
        float[] crits = { scrit, hcrit, 1, 9999 };// , Vector3.Distance(starting_location,goal_location)*2F};

        //Slope of the terrain
        float s = steepness;
        //Increases with height 
        float h = getH(position, d);
        //Metric for roughness
        float r = roughness_weight * Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight));
        //Eucldian distance 
        //float eucl = Vector3.Distance(position,goal_location);
        //Angle difference
        float[] costs = new float[] { s, h, r };//, eucl};
        float cost = 0;

        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i] < crits[i])
            {
                cost += weights[i] * costs[i] / crits[i];
            }
            else
            {
                cost = 1;
                break;
            }
        }
        return cost;
    }

    int ncrit = 130;
    public float hcrit = 4; //40 cm, ~ the height of two steps
    float scrit = 30; //degrees, approximation

    public int d = 10;

    float getH(Vector3 position, int d)
    {

        float hmax = 0;
        int nst = 0;



        int y = Mathf.RoundToInt(position[2] / y_max * terrainData.alphamapHeight);
        int x = Mathf.RoundToInt(position[0] / x_max * terrainData.alphamapWidth);

        int xl = x - d;
        int xh = x + d;
        int yl = y - d;
        int yh = y + d;


        //float steepness = terrainData.GetSteepness(position[2] / y_max, position[0] / x_max);
        float steepness = terrainData.GetSteepness(position[0] / x_max, position[2] / y_max);
        float height = position[1];
        
        for (int i = xl; i < xh; i++)
        {
            for (int j = yl; j < yh; j++)
            {
                if (i >= 0 && i <= terrainData.alphamapWidth && j >= 0 && j <= terrainData.alphamapHeight)
                {
                    //float step_height = Mathf.Abs(terrainData.GetHeight(Mathf.RoundToInt(j/ terrainData.alphamapHeight), Mathf.RoundToInt(i)/terrainData.alphamapWidth) - height);
                    float step_height = Mathf.Abs(terrainData.GetHeight(i , j ) - height);
                    //Debug.Log("A");
                    //Debug.Log(step_height);
                    //if (step_height > hcrit && Mathf.Abs(terrainData.GetSteepness(j / (float)terrainData.alphamapHeight, i / (float)terrainData.alphamapWidth) - steepness) > scrit)
                    if (step_height > hcrit && Mathf.Abs(terrainData.GetSteepness(i / (float)terrainData.alphamapWidth, j / (float)terrainData.alphamapHeight) - steepness) > scrit)
                    {
                        if (step_height > hmax) { hmax = step_height; }
                    }
                    if (step_height > hcrit)
                    {
                        nst++;
                    }
                }
            }
        }

        if (hmax > hmax * nst / ncrit)
        {
            hmax = hmax * nst / ncrit;
        }
        //Debug.Log(hmax);
        return hmax;





    }
    
    Vector2 Interpolate(Vector2 close_location, Vector2 random_location)
    {

        Vector2 final_location = random_location;
        Vector2 mid_location = close_location;

        if (Vector2.Distance(close_location, random_location) > delta)
        {
            final_location = close_location + (random_location- close_location).normalized * delta;
        }

        float f = 0f;

        while(Vector2.Distance(close_location,mid_location) < Vector2.Distance(close_location,final_location))
        {

            Vector2 temp_mid_location = Vector2.Lerp(close_location, final_location, f);
            f+=.2f;

            if (getCost(getPoint(temp_mid_location[0] / x_max, temp_mid_location[1] / y_max),d)>= 0.8)
            {
                break;
            }
            else
            {
                mid_location = temp_mid_location;
            }

        }
        return mid_location;

    }

  
}
