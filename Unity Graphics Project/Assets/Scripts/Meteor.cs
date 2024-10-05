using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField]
    float timeToCollideAt = 35f; // how many seconds to collide into mountain

    [SerializeField]
    Transform target;

    [SerializeField]
    Terrain terrain;

    [SerializeField]
    TerrainSurfaceChanger terrainSurfaceChanger;

    [SerializeField]
    ParticleSystem system;

    [SerializeField]
    float deformationDepth = 0.1f;

    float radius;

    // Start is called before the first frame update
    void Start()
    {
        system.Stop();
        StartCoroutine(MoveToPosition());
        radius = transform.localScale.x / 2f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            DeformTerrain(collision.GetContact(0).point);
            system.Play();
            Destroy(this);
        }
    }

    private void DeformTerrain(Vector3 point)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int x = (int)((point.x - terrainPos.x) / terrainData.size.x * terrainData.heightmapResolution);
        int z = (int)((point.z - terrainPos.z) / terrainData.size.z * terrainData.heightmapResolution);

        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        float impactRadius = radius * 2 / terrainData.size.x;

        // get the terrainlayers
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution);

        // Modify the heights based on the impact
        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrainData.heightmapResolution; j++)
            {
                float distance = Vector2.Distance(new Vector2(i, j), new Vector2(x, z)) / terrainData.heightmapResolution;
                if (distance < impactRadius)
                {
                    float deformAmount = Mathf.Lerp(1, 0, distance / impactRadius);
                    // swapped beacause x and z coords swapped otherwise
                    heights[j, i] -= deformAmount * deformationDepth;

                    splatmapData[j, i, 0] = splatmapData[j, i, 0] * 0.1f; // index 0 which is the snow 
                    splatmapData[j, i, 1] = splatmapData[j, i, 1] * 0.5f; // ice rock blends with normal rock
                    splatmapData[j, i, 2] = 1f; // 2 which is rock which is uncovered when mountain destroyed

                    // make sure that the damage persists without being overwritten by snowfall in the terrain surface changer class on the first frame
                    terrainSurfaceChanger.splatmapData[j, i, 0] = splatmapData[j, i, 0]; 
                    terrainSurfaceChanger.splatmapData[j, i, 1] = splatmapData[j, i, 1];
                    terrainSurfaceChanger.splatmapData[j, i, 2] = splatmapData[j, i, 2];
                }
            }
        }
        // Apply the modified heights to the terrain
        terrainData.SetHeights(0, 0, heights);

        // Apply the modified splatmap to the terrain on the same frame of the impact
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    IEnumerator MoveToPosition()
    {
        Vector3 originalPos = transform.position;
        float t = 0f;
        Vector3 rotAxis = Random.onUnitSphere;
        float rotAngle = Random.Range(20f, 341f);
        while (t < 1)
        {
            t += Time.deltaTime / timeToCollideAt;
            transform.position = Vector3.Lerp(originalPos, target.position, t);
            transform.Rotate(rotAxis, rotAngle * Time.deltaTime);
            yield return null;
        }
    }
}
