using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class steps_A_E : MonoBehaviour {

    public bool path_found = false;
    public float node_line_width = 1;
    public float path_width = 3;
    List<Node> tRRT = new List<Node>();

    public bool go = false;

    class Node
    {
        public Vector3 position;
        public Vector3 parent_position;
        public int parent_index;
        public float radius;
        public Node(Vector3 pos, Vector3 p_pos, int pi)
        {
            position = pos;
            parent_position = p_pos;
            parent_index = pi;
            //radius = R;
        }
    }

    public GameObject starting_point;
    public GameObject point;
    public GameObject final_point;

    public int max_num_point = 5000;
    public int curr_num_points = 0;


    public float x_max = 100;
    public float y_max = 100;

    public int grid_size = 10;

    public float delta = 3;


    public Vector3 goal_location;
    public Vector3 starting_location;

    public List<Vector3>[][] grid_map;




    //Transition test elements
    int failed_queries = 0;
    int maximum_failed_queries = 50;

    float alpha = 2;
    float T = 5;
    float K_cost = 10;

    float cost_max = 1;

    GameObject terrain_gm;
    Terrain terrain;
    TerrainData terrainData;

    Ray myRay;
    RaycastHit hit;

    void Start() {
        terrain_gm = GameObject.Find("Terrain");
        terrain = terrain_gm.GetComponent<Terrain>();
        terrainData = terrain.terrainData;


        x_max = terrainData.bounds.size[0];
        y_max = terrainData.bounds.size[2];

        //goal_location = getPoint(Random.value, Random.value);

        grid_map = new List<Vector3>[Mathf.RoundToInt(x_max * 2) / grid_size + 1][];
        for (int i = 0; i < Mathf.RoundToInt(x_max * 2) / grid_size + 1; i++)
        {
            grid_map[i] = new List<Vector3>[Mathf.RoundToInt(y_max * 2) / grid_size + 1];
            for (int j = 0; j < Mathf.RoundToInt(y_max * 2) / grid_size + 1; j++)
            {
                grid_map[i][j] = new List<Vector3> { };
            }
        }

        Physics.gravity = new Vector3(0, 0, 0);

        //Vector3 location = getPoint(Random.value, Random.value);
        //addToGrid(location);

        //starting_point = GameObject.Instantiate(starting_point, location, Quaternion.identity) as GameObject; //spawn new intial point 

        //final_point = GameObject.Instantiate(final_point, goal_location, Quaternion.identity) as GameObject; //spawn new intial point 



        //Node root = new Node(location, location, 0);
        //tRRT.Add(root);

        //LineRenderer lineRenderer = starting_point.AddComponent<LineRenderer>();

        //curr_num_points++;

    }

    
    void Update() {
        if (GameObject.Find("Starting point(Clone)"))
        {
            addToGrid(starting_location);
            Node root = new Node(starting_location, starting_location, -1);
            tRRT.Add(root);
            curr_num_points++;

        }

        if (GameObject.Find("Starting point(Clone)") && GameObject.Find("Final point(Clone)"))
        {

            bool redo = false;
            if (curr_num_points <= max_num_point && go)
            {
                float closest = 100000;

                Vector3 closest_node = new Vector3(9999, 9999, 9999);

                Vector3 location = getPoint(Random.value, Random.value);
                //search for closest node
                for (int i = 0; i < grid_map.Length; i++)
                {
                    for (int j = 0; j < grid_map[i].Length; j++)
                    {
                        for (int k = 0; k < grid_map[i][j].Count; k++)
                        {
                            Vector2 grid_vec = new Vector2(grid_map[i][j][k][0], grid_map[i][j][k][2]);
                            Vector2 loc_vec = new Vector2(location[0], location[2]);

                            if (closest_node != new Vector3(9999, 9999, 9999))
                            {
                                if (Vector2.Distance(loc_vec, grid_vec) <= closest)
                                {
                                    //closest = Vector2.Distance(new Vector2(location[0],location[2]), grid_map[i][j][k]);
                                    closest = Vector2.Distance(loc_vec, grid_vec);
                                    closest_node = grid_map[i][j][k];
                                }
                            }

                            else
                            {
                                closest = Vector2.Distance(loc_vec, grid_vec);
                                closest_node = grid_map[i][j][k];
                            }
                        }
                    }
                }

                //Vector3 new_node = closest_node + (location - closest_node).normalized * delta;

                Vector3 new_node = closest_node + new Vector3(location[0] - closest_node[0], 0, location[2] - closest_node[2]).normalized * delta;

                new_node = getPoint(new_node[0] / x_max, new_node[2] / y_max); //recalculate the the new node height
                //Debug.Log(TransitionTest(closest_node, new_node));
                if (TransitionTest(closest_node, new_node))
                //if (true)
                {
                    //Debug.Log("point added");
                    addToGrid(new_node); //Add new point to the grid

                    int parent_ind = 0;

                    while (tRRT[parent_ind].position != closest_node) { parent_ind++; }

                    Node leaf = new Node(new_node, closest_node, parent_ind);
                    tRRT.Add(leaf);
                    GameObject p = Instantiate(point, new_node, Quaternion.identity) as GameObject; //spawn new intial point 





                    LineRenderer lineRenderer = p.GetComponent<LineRenderer>();

                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.widthMultiplier = node_line_width;
                    lineRenderer.positionCount = 2;

                    // A simple 2 color gradient with a fixed alpha of 1.0f.
                    float alpha = 1.0f;
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                        );

                    lineRenderer.colorGradient = gradient;
                    lineRenderer.SetPosition(0, closest_node);
                    lineRenderer.SetPosition(1, new_node);

                    curr_num_points++;
                    //Debug.Log(Vector3.Distance(goal_location, leaf.position));
                    if (Vector3.Distance(goal_location, leaf.position) <= delta*3)
                    {

                        Node final_node = leaf;
                        build_path(final_node);
                        go = false;
                        path_found = true;
                    }
                    redo = true;
                }
                else
                {
                    redo = false;
                }

            }
        }
    }

    public List<Vector3> path = new List<Vector3> { }; 

    void build_path(Node final_node)
    {

        //path = new List<Vector3>();
        Node curr_node = final_node;
        int parent_index = final_node.parent_index;
        path.Add(goal_location);

        while (parent_index != 0)
        {
            path.Add(curr_node.position);
            curr_node = tRRT[parent_index];
            parent_index = curr_node.parent_index;
        }
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
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(new Color(1,0,1,1), 1.0f) },
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
        float cost_near = getCost(qnear);
        float cost_new = getCost(qnew);

        if (cost_new > cost_max) {
            //Debug.Log("cost fail");
            return false; }
        if (cost_new < cost_near) {
            //if(cost_near != 0) { Debug.Log(cost_new); }
            return true; }


        //float dist = Vector2.Distance(new Vector2(qnear[0],qnear[2]), new Vector2(qnew[0],qnew[2]));
        K_cost = 10;//currently set to 10
        T = 30;//current set to 5 
        alpha = 2;//current set to 2
        float dist = Vector3.Distance(qnear, qnew);
        float trans_prob = -((cost_new - cost_near) / dist) / (K_cost * T);
        float rho = Mathf.Exp((float)trans_prob);

        if (Random.value < rho) {
            T /= alpha;
            failed_queries = 0;
            return true;
        }
        else
        {
            if (failed_queries > maximum_failed_queries) {
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

    Vector3 getPoint(float x, float y)
    {
        float x_corr = x * x_max;
        float y_corr = y * y_max;
        //float z_corr = terrainData.GetHeight(Mathf.RoundToInt(y * terrainData.heightmapHeight), Mathf.RoundToInt(x * terrainData.heightmapWidth));
        float z_corr = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth), Mathf.RoundToInt(y * terrainData.heightmapHeight));

        Vector3 location = new Vector3(x_corr, z_corr, y_corr);
        return location;
    }

    public void addToGrid(Vector3 location)
    {

        //choose cell storage 
        int l = Mathf.RoundToInt(location[0]) / grid_size;
        int m = Mathf.RoundToInt(location[2]) / grid_size;
        //Add new point to the grid


        grid_map[l][m].Add(location);
    }

    float getCost(Vector3 position)
    {
        float height = position[1];
        //float steepness = terrainData.GetSteepness(position[2] / y_max, position[0] / x_max);
        float steepness = terrainData.GetSteepness( position[0] / x_max, position[2] / y_max);

        
        float slope_weight = 0.5F;
        float height_weight = 0.25F;
        float roughness_weight= 0.25F;

        float[] weights = { slope_weight, height_weight, roughness_weight, 1 };
        float[] crits = { scrit, hcrit, 9999, 9999 };

        //Slope of the terrain
        float s = steepness;
        //Increases with height 
        float h = getH(position);
        //float h = height_weight * height / terrainData.bounds.size[1];
        //Metric for roughness
        float r = roughness_weight * Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight));

        float[] costs = new float[] { s, h, r };        
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
        
        //Debug.Log(cost);
        return cost;
    }

    int ncrit = 50;
    public float hcrit = 4; //40 cm, ~ the height of two steps
    float scrit = 30; //degrees, approximation

    public int d = 10;

    float getH(Vector3 position)
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
                    float step_height = Mathf.Abs(terrainData.GetHeight(Mathf.RoundToInt(i)/terrainData.alphamapWidth, Mathf.RoundToInt(j / terrainData.alphamapHeight)) - height);
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

}
