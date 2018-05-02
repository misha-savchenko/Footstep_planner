using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrain_color_2 : MonoBehaviour
{
    public float radius = 5;

    public float hcrit = 4; //40 cm, ~ the height of two steps
    public float scrit = 30; //degrees, approximation
    public int ncrit = 50;

    public int nst = 0;
    public int d = 10;


    public float a1 = 0.5F;
    public float a2 = 0.25F;
    public float a3 = 0.25F;


    public float[] crits;
    public float[] weights;

    void Start()
    {

        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;


                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                /*
                int ncrit = 50;
                int nst = 0;
                int d = 10;

                float hcrit = 4; //40 cm, ~ the height of two steps
                float scrit = 30; //degrees, approximation
                */
                int xl = x - d;
                int xh = x + d;
                int yl = y - d;
                int yh = y + d;


                float hmax = 0;

                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));
                //List<float> temp_step_height = new List<float>{ 0 };
                
                for (int i = xl; i < xh; i++)
                {
                    for (int j = yl; j < yh; j++)
                    {
                        if (i >= 0 && i <= terrainData.alphamapWidth && j >= 0 && j <= terrainData.alphamapHeight)
                        {
                            float step_height = Mathf.Abs(terrainData.GetHeight(j, i)-height);
                            if (step_height > hcrit && Mathf.Abs(terrainData.GetSteepness(j / (float)terrainData.alphamapHeight, i / (float)terrainData.alphamapWidth) - steepness) > scrit)
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
                

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                //Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                // Texture[0] has constant influence
                splatWeights[3] = 0.5f;

                //Weights for the traversibility of the terrain 
                /*
                float a1 = 0.5F;
                float a2 = 0.25F;
                float a3 = 0.25F;
                */
                //float[] weights = { a1, a2, a3, 1 };
                //float[] crits = { scrit, hcrit, 1, 9999};
                //Slope of the terrain
                splatWeights[0] = steepness;// Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));
                //Increases with height 
                //splatWeights[1] = a2 * height /terrainData.bounds.size[1];
                splatWeights[1] = hmax;
                //Metric for roughness
                splatWeights[2] = Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight ));

                splatWeights[3] = 0;

                float z = 0;
                for (int i = 0; i < splatWeights.Length; i++)
                {
                    if (splatWeights[i] < crits[i])
                    {
                        z += weights[i]*splatWeights[i]/crits[i];
                    }
                    else
                    {
                        z = 1;
                        break;

                    }
                }

                splatWeights[0] = 0;
                splatWeights[1] = 0;
                splatWeights[2] = 0;
                splatWeights[3] = 0;

                if (z >= 1)
                {
                    splatWeights[1] = 1;
                }
                else if (z >= .5) {
                    splatWeights[1] = z;
                    splatWeights[2] = 1 - z;
                }
                else {
                    splatWeights[2] = z;
                    splatWeights[3] = 1 - z;
                }
                

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    //splatWeights[i] /= 2;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private float getSTD(TerrainData terrainData, int x, int y)
    {

        int d = Mathf.RoundToInt(radius);

        List<float> circle = new List<float> { };
        float avg = 0;

        for (int i = x - d; i < x + d; i++)
        {
            for(int j = y - d; j < y + d; j++)
            {
                if(i >= 0 && j >=0 && i <= terrainData.heightmapWidth && j <= terrainData.heightmapHeight)
                {
                    if (Vector2.Distance(new Vector2(i,j),new Vector2(x,y)) <= radius)
                    {
                        float h = terrainData.GetHeight(j, i);
                        circle.Add(h);
                        avg += h;
                    }
                }

            }
        }

        float std = 0; 


        avg /= circle.Count;
        for (int i = 0; i < circle.Count; i++)
        {
            std += ((circle[i] - avg) * (circle[i] - avg));
        }

        std = Mathf.Sqrt(std / circle.Count);

        return Mathf.Abs((terrainData.GetHeight(y, x) - avg)/std);
        
    }

}