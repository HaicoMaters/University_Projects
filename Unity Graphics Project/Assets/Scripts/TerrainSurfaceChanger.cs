using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSurfaceChanger : MonoBehaviour
{
    Terrain terrain;

    TerrainData terrainData;

    float[,] originalTerrainHeights;

    float[,,] originalSplatmapData;

    public float[,,] splatmapData; // accessed by meteor and any other scripts which chnge the scene to make sure changes persist without being overwritten

    [SerializeField]
    Transform water;

    [SerializeField]
    int snowBuildUpItterationsPerFrame = 40; // for performance reasons do certain number of itterations per frame in snow build up coroutine
                                             // in a similar are the snow will build up in the same sort of itteration cycle due to proximity
                                             // can change alongside the terrain resolution for performance reasons

    [SerializeField]
    float snowBuildUpScale = 10f;



    // Start is called before the first frame update
    void Start()
    {
        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        originalTerrainHeights = terrainData.GetHeights(0, 0, resolution, resolution);
        originalSplatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution);
        splatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution); // have to access each frame
        StartCoroutine(snowBuildUp());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationQuit()
    {
        ResetTerrainData();
    }

    void OnDisable()
    {
        ResetTerrainData();
    }

    // keep the original terrain data without damages or texture changes between disabling playmode
    void ResetTerrainData()
    {
        if (terrainData != null)
        {
            terrainData.SetHeights(0, 0, originalTerrainHeights);
            terrainData.SetAlphamaps(0, 0, originalSplatmapData);
        }
    }

    IEnumerator snowBuildUp()
    {
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float buildUpAmount = Time.deltaTime * snowBuildUpScale;
        float waterHeightNormalized = water.position.y / terrainData.size.y;
        int iterations = snowBuildUpItterationsPerFrame;
        while (true)
        {
            for (int i = 0; i < terrainData.heightmapResolution - 1; i++)
            {
                for (int j = 0; j < terrainData.heightmapResolution - 1; j++)
                {

                    if (heights[j, i] > waterHeightNormalized) // only change texture above water
                    {
                        if (splatmapData[j, i, 2] > splatmapData[j, i, 0])
                        {
                            splatmapData[j, i, 1] += buildUpAmount * 0.5f;
                        }
                        if (splatmapData[j, i, 0] < 1)
                        {
                            splatmapData[j, i, 0] += buildUpAmount;
                        }
                        if (splatmapData[j, i, 2] > 0)
                        {
                            splatmapData[j, i, 2] -= buildUpAmount;
                        }
                    }
                    iterations--;
                    if (iterations == 0)
                    {
                        iterations = snowBuildUpItterationsPerFrame;
                        // Apply the modified splatmap to the terrain at end of each itteration for smoother changes
                        yield return null;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, splatmapData);
        }
    }
}

