using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class steps_A_E : MonoBehaviour {

    List<Node> tRRT = new List<Node>();

    public bool go = false;

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
    int curr_num_points = 0;


    public float x_max = 100;
    public float y_max = 100;

    public int grid_size = 10;

    public float delta = 3;

    public Vector3 goal_location;

    List<Vector3>[][] grid_map;




    //Transition test elements
    int failed_queries = 0;
    int maximum_failed_queries = 50;

    double alpha = 2;
    double temper = 5;
    double K_cost = 10;

    double cost_max = .50;

    GameObject terrain_gm;
    Terrain terrain;
    TerrainData terrainData;

    void Start() {

        terrain_gm = GameObject.Find("Terrain");
        terrain = terrain_gm.GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        //float y_01 = (float)y / (float)terrainData.alphamapHeight;
        //float x_01 = (float)x / (float)terrainData.alphamapWidth;

        //float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

        x_max = terrainData.bounds.size[0];
        y_max = terrainData.bounds.size[2];
        //h = terrainData.bounds.size[1];


        goal_location = getPoint(Random.value, Random.value);


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

        Vector3 location = getPoint(Random.value, Random.value);
        addToGrid(location);


        starting_point = GameObject.Instantiate(starting_point, location, Quaternion.identity) as GameObject; //spawn new intial point 
        final_point = GameObject.Instantiate(final_point, goal_location, Quaternion.identity) as GameObject; //spawn new intial point 



        Node root = new Node(location, location, 0);
        tRRT.Add(root);

        LineRenderer lineRenderer = starting_point.AddComponent<LineRenderer>();

        curr_num_points++;

    }

    void Update() {
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
                        Vector2 grid_vec = new Vector2(grid_map[i][j][k][0],grid_map[i][j][k][2]);
                        Vector2 loc_vec = new Vector2(location[0], location[2]);

                        if (closest_node != new Vector3(9999, 9999, 9999)) {
                            if (Vector2.Distance(loc_vec, grid_vec ) <= closest)
                            {
                                closest = Vector3.Distance(location, grid_map[i][j][k]);
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


            Vector3 new_node = closest_node + (location - closest_node).normalized * delta;
            new_node = getPoint(new_node[0]/x_max, new_node[2]/y_max); //recalculate the the new node location
            addToGrid(new_node); //Add new point to the grid
            
            int parent_ind = 0;

            while (tRRT[parent_ind].position != closest_node)
            {
                parent_ind++;
            }

            Node leaf = new Node(new_node, closest_node, parent_ind);
            tRRT.Add(leaf);
            GameObject p = Instantiate(point, new_node, Quaternion.identity) as GameObject; //spawn new intial point 

            LineRenderer lineRenderer = p.GetComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.2f;
            lineRenderer.positionCount = 2;

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                );

            lineRenderer.colorGradient = gradient;

            lineRenderer.SetPosition(0, closest_node);
            lineRenderer.SetPosition(1, new_node);

            curr_num_points++;

            if (Vector3.Distance(goal_location, leaf.position) <= delta)
            {
                Node final_node = leaf;
                build_path(final_node);
                go = false;
            }
        }
    }

    void build_path(Node final_node)
    {

        List<Vector3> path = new List<Vector3>();
        Node curr_node = final_node;
        int parent_index = final_node.parent_index;

        while (parent_index != 0)
        {
            path.Add(curr_node.position);
            curr_node = tRRT[parent_index];
            parent_index = curr_node.parent_index;
        }

        GameObject p = Instantiate(point, final_node.position, Quaternion.identity) as GameObject; //spawn new intial point 

        LineRenderer lineRenderer = p.GetComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 2f;
        lineRenderer.positionCount = path.Count;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
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
        double cost_near = 1;
        double cost_new = 1;

        if (cost_new > cost_max) { return false; }
        if (cost_new < cost_near) { return true; }


        double dist = Vector3.Distance(qnear, qnew);
        double trans_prob = -((cost_new - cost_near) / dist) / (K_cost * temper);
        double rho = Mathf.Exp((float)trans_prob);

        if (Random.value < rho) {
            temper /= alpha;
            failed_queries = 0;
            return true;
        }
        else
        {
            if (failed_queries > maximum_failed_queries) {
                temper *= alpha;
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
        float z_corr = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapHeight), Mathf.RoundToInt(y * terrainData.heightmapHeight));


        Vector3 location = new Vector3(x_corr, z_corr, y_corr);
        return location;
    }

    void addToGrid(Vector3 location)
    {

        //choose cell storage 
        int l = Mathf.RoundToInt(location[0]) / grid_size;
        int m = Mathf.RoundToInt(location[2]) / grid_size;
        //Add new point to the grid


        grid_map[l][m].Add(location);
    }



}
