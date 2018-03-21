using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrain_color_2 : MonoBehaviour
{

    float radius = 5;

    

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

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                //Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                // Texture[0] has constant influence
                splatWeights[3] = 0.5f;


                //Weights for the traversibility of the terrain 
                float a1 = 0.5F;
                float a2 = 0.25F;
                float a3 = 0.25F;
                

                //Red 3 
                //Green 2
                //Blue 1 


                //Slope of the terrain
                splatWeights[0] = a1 * steepness/90;// Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));
                //Increases with height 
                splatWeights[1] = a2 * height /terrainData.bounds.size[1];
                //Metric for roughness
                splatWeights[2] = a3 * Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));

                
                

                splatWeights[3] = 0;

                float z = 0;
                for (int i = 0; i < splatWeights.Length; i++)
                {
                    z += splatWeights[i];
                }
                splatWeights[0] = 0;
                splatWeights[1] = z;
                splatWeights[2] = 0;
                splatWeights[3] = 1-z;

                /*
                if (x > 95 && x < 100 && y < 5)
                {

                    Debug.Log('A');

                    Debug.Log(z);
                    Debug.Log(1-z);
                }
                */

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= 2;

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
                        float h = terrainData.GetHeight(i, j);
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

        return Mathf.Abs((terrainData.GetHeight(x, y) - avg)/std);
        
    }
}