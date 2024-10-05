using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField]
    Material fireMaterial;

    [SerializeField]
    Light fireLight;

    [SerializeField]
    Terrain terrain;

    [SerializeField]
    int meltItterationsPerFrame = 10;

    [SerializeField]
    float meltScale = 0.5f;

    [SerializeField]
    TerrainSurfaceChanger terrainSurfaceChanger;

    public float intensity = 2f;

    [SerializeField]
    float radius = 12f;

    // Start is called before the first frame update
    void Start()
    {
        fireLight.color = fireMaterial.GetColor("_Colour");
        StartCoroutine(meltSnow());
    }

    // Update is called once per frame
    void Update()
    {
        fireLight.intensity = Random.Range(0.9f * intensity, 1.1f * intensity); // make the light appear to flicker
    }

    IEnumerator meltSnow()
    {
        yield return new WaitForSeconds(0.3f); // wait for terrain surface to finish start function
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        int x = (int)((transform.position.x - terrainPos.x) / terrainData.size.x * terrainData.heightmapResolution);
        int z = (int)((transform.position.z - terrainPos.z) / terrainData.size.z * terrainData.heightmapResolution);

        int iterations = meltItterationsPerFrame;

        while (true)
        {
            // get the terrainlayers
            float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution);
            float meltRadius = (radius * intensity / fireLight.intensity) / terrainData.size.x;
            for (int i = 0; i < terrainData.heightmapResolution - 1; i++)
            {
                for (int j = 0; j < terrainData.heightmapResolution - 1; j++)
                {
                    float distance = Vector2.Distance(new Vector2(i, j), new Vector2(x, z)) / terrainData.heightmapResolution;
                    if (distance < meltRadius)
                    {
                        float meltAmount = Mathf.Lerp(1, 0, Mathf.Pow(distance / meltRadius, 2)) * meltScale; // scale how much it melts based on distance

                        splatmapData[j, i, 0] -= splatmapData[j, i, 0] * meltAmount; // index 0 which is the snow 
                        splatmapData[j, i, 1] -= splatmapData[j, i, 1] * meltAmount * 0.2f; // index 1 is ice rock melts slower than snow
                        splatmapData[j, i, 2] += splatmapData[j, i, 2] * meltAmount; // index 2 is rock which is uncovered when melting ssnow

                        // pass the changes to the terrainSurfaceChanger
                        terrainSurfaceChanger.splatmapData[j, i, 0] = splatmapData[j, i, 0];
                        terrainSurfaceChanger.splatmapData[j, i, 1] = splatmapData[j, i, 1];
                        terrainSurfaceChanger.splatmapData[j, i, 2] = splatmapData[j, i, 2];
                    }
                    iterations--;
                    if (iterations == 0)
                    {
                        iterations = meltItterationsPerFrame;
                        meltRadius = (radius * intensity / fireLight.intensity) / terrainData.size.x; // change the range each flame have an effect where it melts more when flickering
                        yield return null;
                    }
                }
            }
        }
    }
}
